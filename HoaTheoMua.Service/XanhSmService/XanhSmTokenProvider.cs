using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.XanhSmService;

public interface IXanhSmTokenProvider
{
    /// <summary>
    /// Lấy access token còn hạn từ cache hoặc gọi OAuth token endpoint nếu cache trống/hết hạn.
    /// </summary>
    Task<string> GetAccessToken(CancellationToken cancellationToken = default);

    /// <summary>Xóa token cache khi Xanh SM từ chối token bằng HTTP 401.</summary>
    void Invalidate();
}

/// <summary>
/// Lấy và cache OAuth token của Xanh SM.
/// Khóa RefreshLock chặn nhiều request cùng lúc gọi token endpoint. Cache hiện là in-memory,
/// phù hợp deployment một instance; sau này có thể thay bằng Redis mà không ảnh hưởng Service.
/// </summary>
public sealed class XanhSmTokenProvider : IXanhSmTokenProvider
{
    private const string CacheKey = "xanh-sm.access-token";
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly XanhSmOption _option;
    private readonly ILogger<XanhSmTokenProvider> _logger;

    /// <summary>
    /// Nhận các dependency do DI tạo. Cấu hình chỉ được bind khi TokenProvider được endpoint Xanh SM sử dụng;
    /// access token không lưu trong appsettings mà chỉ lấy từ GSM lúc GetAccessToken chạy.
    /// </summary>
    public XanhSmTokenProvider(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration,
        ILogger<XanhSmTokenProvider> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _option = new XanhSmOption();
        configuration.GetSection(nameof(XanhSmOption)).Bind(_option);
        _logger = logger;
    }

    /// <summary>
    /// Lấy token OAuth. Ưu tiên token còn hạn trong memory cache; nếu không có, chỉ một request được phép
    /// gọi token endpoint nhờ RefreshLock. Các request khác chờ và tái sử dụng token vừa được cache.
    /// </summary>
    public async Task<string> GetAccessToken(CancellationToken cancellationToken = default)
    {
        // Kiểm tra trước khi gọi mạng để lỗi thiếu biến môi trường rõ ràng, không bị nhầm là lỗi GSM.
        if (string.IsNullOrWhiteSpace(_option.ClientId)
            || string.IsNullOrWhiteSpace(_option.ClientSecret)
            || string.IsNullOrWhiteSpace(_option.TokenUrl)
            || string.IsNullOrWhiteSpace(_option.Scope))
        {
            throw new XanhSmException(503, "XANH_SM_CREDENTIAL_NOT_CONFIGURED",
                "Chưa cấu hình đầy đủ Client ID, Client Secret, scope hoặc Token URL của Xanh SM.");
        }

        // Có token còn hạn thì dùng ngay, không gọi sang Xanh SM.
        if (_cache.TryGetValue<string>(CacheKey, out var cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
            return cachedToken;

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Sau khi chờ lock phải đọc cache lần nữa: request khác có thể đã refresh token trong lúc chờ.
            if (_cache.TryGetValue<string>(CacheKey, out cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
                return cachedToken;

            // Token endpoint dùng chuẩn OAuth client_credentials với form-url-encoded.
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = _option.GrantType,
                ["client_id"] = _option.ClientId,
                ["client_secret"] = _option.ClientSecret,
                ["scope"] = _option.Scope
            });
            using var response = await _httpClient.PostAsync(_option.TokenUrl, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Không log request/body vì có client_secret. Status là đủ để phân biệt sai credential (401/400)
                // với lỗi quyền truy cập/whitelist IP (403) hoặc lỗi hạ tầng (5xx).
                _logger.LogWarning(
                    "Xanh SM token endpoint returned HTTP {StatusCode}. Check client credentials, scope and GSM IP whitelist.",
                    (int)response.StatusCode);
                // Token endpoint không trả cho FE body nguyên gốc. Mỗi status được chuyển thành một mã ổn định
                // để FE/dev biết cần kiểm tra credential, IP whitelist, rate limit hay tình trạng GSM.
                throw response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => new XanhSmException(502, "XANH_SM_TOKEN_BAD_REQUEST",
                        "GSM không xử lý được yêu cầu lấy token. Kiểm tra grant type, Client ID và scope."),
                    HttpStatusCode.Unauthorized => new XanhSmException(502, "XANH_SM_TOKEN_UNAUTHORIZED",
                        "GSM từ chối Client ID hoặc Client Secret. Kiểm tra credential sandbox."),
                    HttpStatusCode.Forbidden => new XanhSmException(502, "XANH_SM_TOKEN_FORBIDDEN",
                        "GSM không cho phép lấy token. Kiểm tra IP public đã whitelist và scope được cấp."),
                    (HttpStatusCode)429 => new XanhSmException(429, "XANH_SM_TOKEN_RATE_LIMITED",
                        "GSM đang giới hạn số lần lấy token. Vui lòng thử lại sau."),
                    _ when (int)response.StatusCode >= 500 => new XanhSmException(502,
                        "XANH_SM_TOKEN_PROVIDER_UNAVAILABLE", "GSM token endpoint đang tạm thời không khả dụng."),
                    _ => new XanhSmException(502, "XANH_SM_TOKEN_REQUEST_FAILED",
                        "Không thể lấy access token từ GSM.")
                };
            }

            var token = JsonSerializer.Deserialize<Response.Token>(body);
            if (string.IsNullOrWhiteSpace(token?.AccessToken) || token.ExpiresIn <= 0)
                throw new XanhSmException(502, "XANH_SM_TOKEN_INVALID_RESPONSE",
                    "GSM trả về access token không hợp lệ hoặc thiếu thời hạn token.");

            // Cache ngắn hơn expires_in một khoảng đệm để không dùng token sát thời điểm hết hạn.
            var lifetime = Math.Max(1, token.ExpiresIn - _option.TokenRefreshBufferSeconds);
            _cache.Set(CacheKey, token.AccessToken, TimeSpan.FromSeconds(lifetime));
            return token.AccessToken;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    /// <summary>Xóa token khi upstream trả 401 để lần gọi kế tiếp bắt buộc lấy token mới.</summary>
    public void Invalidate() => _cache.Remove(CacheKey);
}
