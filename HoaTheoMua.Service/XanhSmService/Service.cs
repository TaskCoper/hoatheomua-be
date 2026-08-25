using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.XanhSmService;

/// <summary>
/// Lớp trung gian gọi Xanh SM. Credential của đối tác chỉ tồn tại ở backend, tuyệt đối không trả về FE.
/// </summary>
public sealed class Service : IService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient;
    private readonly IXanhSmTokenProvider _tokenProvider;
    private readonly XanhSmOption _option;
    private readonly ILogger<Service> _logger;

    /// <summary>
    /// Khởi tạo service khi endpoint Xanh SM được gọi lần đầu. Cấu hình Xanh SM được bind tại đây,
    /// không cần đăng ký Options trong Program.cs. HttpClient vẫn do DI quản lý connection pool và timeout.
    /// </summary>
    public Service(HttpClient httpClient, IXanhSmTokenProvider tokenProvider, IConfiguration configuration,
        ILogger<Service> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _option = new XanhSmOption();
        configuration.GetSection(nameof(XanhSmOption)).Bind(_option);
        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, _option.TimeoutSeconds));
        _logger = logger;
    }

    /// <summary>
    /// Gửi request tìm dịch vụ khả dụng tới endpoint <c>/list-available-services</c>.
    /// API upstream trả <c>{ data: [...] }</c>; hàm tách riêng mảng data để FE nhận trực tiếp danh sách dịch vụ.
    /// </summary>
    public async Task<List<Response.AvailableService>> GetAvailableServices(Request.GetAvailableServicesRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<Response.Envelope<List<Response.AvailableService>>>("available-services",
            _option.AvailableServicesPath, request, cancellationToken);
        return response.Data ?? [];
    }

    /// <summary>
    /// Gọi endpoint estimate. Nếu upstream trả HTTP 200 nhưng thiếu data, xem như response không hợp lệ
    /// thay vì trả null gây lỗi ở FE.
    /// </summary>
    public async Task<Response.EstimateData> EstimateOrder(Request.EstimateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<Response.Envelope<Response.EstimateData>>("estimate", _option.EstimateOrderPath,
            request, cancellationToken);
        return response.Data ?? throw InvalidPartnerResponse("estimate");
    }

    /// <summary>
    /// Gọi endpoint tạo đơn. Hàm không tự sửa fee_id, order_time hay addons để Xanh SM có thể đối chiếu
    /// chính xác với estimate do FE đã chọn.
    /// </summary>
    public async Task<Response.CreateOrderData> CreateOrder(Request.CreateXanhSmOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<Response.Envelope<Response.CreateOrderData>>("create-order", _option.CreateOrderPath,
            request, cancellationToken);
        return response.Data ?? throw InvalidPartnerResponse("create-order");
    }

    /// <summary>
    /// Gọi endpoint hủy đơn của Xanh SM và trả message xác nhận từ đối tác.
    /// </summary>
    public async Task<Response.CancelOrderData> CancelOrder(Request.CancelOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<Response.Envelope<Response.CancelOrderData>>("cancel-order", _option.CancelOrderPath,
            request, cancellationToken);
        return response.Data ?? throw InvalidPartnerResponse("cancel-order");
    }

    /// <summary>
    /// Luồng POST dùng chung cho cả bốn API Xanh SM.
    /// Các bước: kiểm tra BaseUrl, lấy token cache, gửi request, refresh/retry một lần nếu 401,
    /// rồi deserialize response hoặc chuyển lỗi upstream thành XanhSmException.
    /// </summary>
    /// <typeparam name="T">Kiểu DTO toàn bộ body response từ Xanh SM.</typeparam>
    private async Task<T> PostAsync<T>(string operation, string path, object requestBody, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        try
        {
            var token = await _tokenProvider.GetAccessToken(cancellationToken);
            using var initialRequest = BuildPostRequest(path, requestBody, token);
            using var initialResponse = await _httpClient.SendAsync(initialRequest, cancellationToken);

            // Nếu token còn hợp lệ, xử lý response ngay.
            if (initialResponse.StatusCode != HttpStatusCode.Unauthorized)
                return await ReadResponse<T>(operation, initialResponse, cancellationToken);

            // Token có thể bị Xanh SM thu hồi trước hạn. Xóa cache và chỉ thử lại đúng một lần.
            // Không lặp vô hạn để tránh gửi trùng request tạo đơn.
            _tokenProvider.Invalidate();
            var refreshedToken = await _tokenProvider.GetAccessToken(cancellationToken);
            using var retryRequest = BuildPostRequest(path, requestBody, refreshedToken);
            using var retryResponse = await _httpClient.SendAsync(retryRequest, cancellationToken);
            return await ReadResponse<T>(operation, retryResponse, cancellationToken, authenticationRetried: true);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new XanhSmException(504, "XANH_SM_TIMEOUT", "Delivery provider did not respond in time.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Xanh SM {Operation} could not reach the delivery provider", operation);
            throw new XanhSmException(502, "XANH_SM_UNAVAILABLE", "Delivery provider is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Tạo một HTTP POST request mới có đủ header Xanh SM.
    /// HttpRequestMessage chỉ gửi được một lần, nên request retry sau 401 phải tạo object mới.
    /// </summary>
    /// <param name="path">Đường dẫn nghiệp vụ tương đối, ví dụ /estimate/order.</param>
    /// <param name="requestBody">DTO đã có JsonPropertyName đúng snake_case.</param>
    /// <param name="token">OAuth access token hiện tại của Xanh SM.</param>
    private HttpRequestMessage BuildPostRequest(string path, object requestBody, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.AcceptLanguage.ParseAdd(_option.AcceptLanguage);
        request.Content = JsonContent.Create(requestBody);
        return request;
    }

    /// <summary>
    /// Đọc body response một lần. Với HTTP thành công, deserialize JSON sang DTO T;
    /// với HTTP lỗi, gọi mapper để chuẩn hóa code/message mà FE nhận được.
    /// </summary>
    private async Task<T> ReadResponse<T>(string operation, HttpResponseMessage response, CancellationToken cancellationToken,
        bool authenticationRetried = false)
    {
        // Đọc body một lần để dùng cho cả deserialize thành công và mapping lỗi.
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw ToPartnerException(operation, response.StatusCode, body, authenticationRetried);

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOptions)
                   ?? throw InvalidPartnerResponse(operation);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Xanh SM {Operation} returned an unreadable success response", operation);
            throw InvalidPartnerResponse(operation);
        }
    }

    /// <summary>
    /// Chuyển HTTP status và body lỗi của Xanh SM thành lỗi nghiệp vụ thống nhất của Hoa Theo Mùa.
    /// Không đưa raw response, token hoặc secret vào exception trả cho FE.
    /// </summary>
    private XanhSmException ToPartnerException(string operation, HttpStatusCode statusCode, string body,
        bool authenticationRetried)
    {
        Response.Error? error = null;
        try
        {
            error = JsonSerializer.Deserialize<Response.Error>(body, JsonOptions);
        }
        catch (JsonException)
        {
            // Proxy có thể trả HTML thay vì JSON; phía dưới vẫn chuẩn hóa lỗi an toàn cho FE.
        }

        _logger.LogWarning("Xanh SM {Operation} returned HTTP {StatusCode}, partner code {PartnerCode}",
            operation, (int)statusCode, error?.Code);

        if (statusCode == HttpStatusCode.Unauthorized)
        {
            return new XanhSmException(502,
                authenticationRetried ? "XANH_SM_API_TOKEN_REJECTED_AFTER_REFRESH" : "XANH_SM_API_TOKEN_REJECTED",
                "GSM từ chối access token khi gọi API nghiệp vụ.");
        }

        if (statusCode == HttpStatusCode.Forbidden)
        {
            return new XanhSmException(502, "XANH_SM_API_FORBIDDEN",
                "GSM không cho phép gọi API này. Kiểm tra scope, môi trường sandbox và IP whitelist.");
        }

        // Chỉ giữ các status có ý nghĩa với FE; lỗi hạ tầng đối tác được gom về 502.
        var publicStatus = statusCode switch
        {
            HttpStatusCode.NotFound => 404,
            HttpStatusCode.Conflict => 409,
            (HttpStatusCode)429 => 429,
            _ when (int)statusCode is >= 400 and < 500 => 400,
            _ => 502
        };
        var messageCode = error?.Code is { } code ? $"XANH_SM_{code}" : "XANH_SM_REQUEST_FAILED";
        var detail = string.IsNullOrWhiteSpace(error?.Message)
            ? "Delivery provider could not process the request."
            : error.Message;
        return new XanhSmException(publicStatus, messageCode, detail);
    }

    /// <summary>Ghép BaseUrl cấu hình với path mà không tạo dấu gạch chéo kép.</summary>
    private Uri BuildUrl(string path)
    {
        var url = $"{_option.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        return new Uri(url);
    }

    /// <summary>
    /// Chặn gọi HTTP nếu BaseUrl chưa được cấu hình. Lỗi trả về rõ ràng hơn UriFormatException
    /// và giúp nhận diện deployment chưa có XANH_SM_BASE_URL.
    /// </summary>
    private void EnsureConfigured()
    {
        if (!Uri.TryCreate(_option.BaseUrl, UriKind.Absolute, out _))
        {
            throw new XanhSmException(502, "XANH_SM_NOT_CONFIGURED", "Delivery provider is not configured.");
        }
    }

    /// <summary>Tạo lỗi 502 dùng chung khi Xanh SM trả 2xx nhưng body thiếu hoặc sai cấu trúc JSON mong đợi.</summary>
    private static XanhSmException InvalidPartnerResponse(string operation) =>
        new(502, "XANH_SM_INVALID_RESPONSE", $"Delivery provider returned an invalid {operation} response.");
}
