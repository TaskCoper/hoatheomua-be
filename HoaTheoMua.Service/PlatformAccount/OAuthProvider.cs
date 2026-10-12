using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PlatformAccount;

public sealed class OAuthProvider(HttpClient http, IOptions<PlatformAccountOptions> settings, TimeProvider clock) : IOAuthProvider
{
    private PlatformAccountOptions Options => settings.Value;
    public Task<List<ProviderAccount>> Exchange(PlatformType platform, string code, string verifier, CancellationToken ct)
        => Bounded(token => ExchangeCore(platform, code, verifier, token), ct);
    public Task<ProviderAccount> Refresh(PlatformType platform, string externalId, string? pageId, Credentials credentials, CancellationToken ct)
        => Bounded(token => RefreshCore(platform, externalId, pageId, credentials, token), ct);
    private async Task<T> Bounded<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(Options.RequestTimeoutSeconds * 3));
        try { return await operation(budget.Token); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { throw new AccountException(504, "PLATFORM_PROVIDER_TIMEOUT", "Hết thời gian xử lý kết nối nền tảng."); }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        { throw InvalidResponse(); }
    }
    private string Graph(string path) => $"https://graph.facebook.com/{Options.Meta.GraphApiVersion}/{path}";
    public static string[] RequiredScopes(PlatformType platform) => platform == PlatformType.Instagram
        ? ["pages_show_list", "pages_read_engagement", "instagram_basic", "instagram_content_publish"]
        : ["pages_show_list", "pages_read_engagement", "pages_manage_posts"];

    // Request analytics consent without making it mandatory for connecting/publishing.
    public static string[] RequestedScopes(PlatformType platform) =>
        [.. RequiredScopes(platform), platform == PlatformType.Instagram ? "instagram_manage_insights" : "read_insights"];

    public string AuthorizeUrl(PlatformType platform, string state, string verifier)
    {
        Options.RequireConfigured(platform);
        if (platform == PlatformType.Zalo)
            return Query("https://oauth.zaloapp.com/v4/oa/permission", new()
            {
                ["app_id"] = Options.Zalo.AppId, ["redirect_uri"] = Query(Options.Callback(platform), new() { ["state"] = state }),
                ["code_challenge"] = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)))
            });
        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = Options.Meta.AppId, ["redirect_uri"] = Options.Callback(platform),
            ["state"] = state, ["response_type"] = "code", ["auth_type"] = "rerequest"
        };
        if (Options.Meta.ConfigId.Length > 0)
            parameters["config_id"] = Options.Meta.ConfigId;
        else
            parameters["scope"] = string.Join(',', RequestedScopes(platform));
        return Query($"https://www.facebook.com/{Options.Meta.GraphApiVersion}/dialog/oauth", parameters);
    }

    private async Task<List<ProviderAccount>> ExchangeCore(PlatformType platform, string code, string verifier, CancellationToken ct)
    {
        Options.RequireConfigured(platform);
        if (platform == PlatformType.Zalo)
        {
            var tokens = await ZaloToken(new() { ["code"] = code, ["code_verifier"] = verifier, ["grant_type"] = "authorization_code" }, ct);
            using var info = await Get("https://openapi.zalo.me/v2.0/oa/getoa", tokens.Credentials.AccessToken, true, ct);
            var data = info.RootElement.GetProperty("data");
            tokens.ExternalId = Required(data, "oa_id"); tokens.Name = Required(data, "name");
            return [tokens];
        }
        using var shortToken = await Get(Query(Graph("oauth/access_token"), new()
        {
            ["client_id"] = Options.Meta.AppId, ["client_secret"] = Options.Meta.AppSecret,
            ["redirect_uri"] = Options.Callback(platform), ["code"] = code
        }), null, false, ct);
        using var longToken = await Get(Query(Graph("oauth/access_token"), new()
        {
            ["client_id"] = Options.Meta.AppId, ["client_secret"] = Options.Meta.AppSecret,
            ["grant_type"] = "fb_exchange_token", ["fb_exchange_token"] = Required(shortToken.RootElement, "access_token")
        }), null, false, ct);
        var token = Required(longToken.RootElement, "access_token");
        var granted = new List<string>();
        string? cursor = null;
        for (var page = 0; page < 20; page++)
        {
            using var permissions = await Get(Query(Graph("me/permissions"), new() { ["limit"] = "100", ["after"] = cursor }), token, false, ct);
            foreach (var item in permissions.RootElement.GetProperty("data").EnumerateArray())
                if (Text(item, "status") == "granted") granted.Add(Required(item, "permission"));
            cursor = NextCursor(permissions.RootElement);
            if (cursor == null) break;
            if (page == 19) throw InvalidResponse();
        }
        if (RequiredScopes(platform).Except(granted).Any()) throw AuthError();
        var expires = Expiry(longToken.RootElement, "expires_in");
        var result = new List<ProviderAccount>(); cursor = null;
        var discoveredPages = 0;
        var contentPages = 0;
        for (var page = 0; page < 20; page++)
        {
            using var pages = await Get(Query(Graph("me/accounts"), new()
            {
                ["fields"] = "id,name,access_token,tasks,instagram_business_account{id,username}", ["limit"] = "100", ["after"] = cursor
            }), token, false, ct);
            foreach (var item in pages.RootElement.GetProperty("data").EnumerateArray())
            {
                discoveredPages++;
                if (!item.TryGetProperty("tasks", out var tasks) || !tasks.EnumerateArray().Any(x => x.GetString() is "CREATE_CONTENT" or "MANAGE"
                    or "PROFILE_PLUS_CREATE_CONTENT" or "PROFILE_PLUS_MANAGE" or "PROFILE_PLUS_FULL_CONTROL")) continue;
                contentPages++;
                var pageId = Required(item, "id");
                var account = new ProviderAccount
                {
                    ExternalId = pageId, PageId = pageId, Name = Required(item, "name"),
                    Scopes = granted.Distinct().Order().ToArray(), ExpiresAt = expires,
                    Credentials = new() { AccessToken = Required(item, "access_token") }
                };
                if (platform == PlatformType.Instagram)
                {
                    if (!item.TryGetProperty("instagram_business_account", out var ig) || ig.ValueKind != JsonValueKind.Object) continue;
                    account.ExternalId = Required(ig, "id"); account.Name = Text(ig, "username") ?? account.Name;
                }
                result.Add(account);
            }
            cursor = NextCursor(pages.RootElement);
            if (cursor == null) break;
            if (page == 19) throw InvalidResponse();
        }
        if (result.Count == 0)
        {
            var detail = discoveredPages == 0
                ? "Meta không trả về Trang nào cho phiên cấp quyền này. Hãy kết nối lại, chọn Chỉnh sửa phần cài đặt và kiểm tra Trang được cấp quyền cùng quyền truy cập Trang của tài khoản Facebook."
                : contentPages == 0
                    ? $"Meta trả về {discoveredPages} Trang nhưng không có Trang nào có quyền tạo hoặc quản lý nội dung được hỗ trợ. Hãy kiểm tra quyền trên Trang và quyền được cấp qua doanh nghiệp."
                    : $"Meta trả về {contentPages} Trang có quyền nội dung nhưng không có tài khoản Instagram chuyên nghiệp liên kết. Hãy kiểm tra liên kết Instagram với Trang đã chọn.";
            throw new AccountException(400, "PLATFORM_NO_ELIGIBLE_ACCOUNT", detail);
        }
        return result;
    }

    private async Task<ProviderAccount> RefreshCore(PlatformType platform, string externalId, string? pageId, Credentials credentials, CancellationToken ct)
    {
        Options.RequireConfigured(platform);
        if (platform == PlatformType.Zalo)
        {
            if (string.IsNullOrWhiteSpace(credentials.RefreshToken)) throw AuthError();
            var account = await ZaloToken(new() { ["refresh_token"] = credentials.RefreshToken, ["grant_type"] = "refresh_token" }, ct);
            account.ExternalId = externalId;
            // Persist the rotated pair immediately; don't risk losing it to a subsequent profile request.
            return account;
        }
        using var debug = await Get(Query(Graph("debug_token"), new() { ["input_token"] = credentials.AccessToken }),
            $"{Options.Meta.AppId}|{Options.Meta.AppSecret}", false, ct);
        var tokenData = debug.RootElement.GetProperty("data");
        if (!tokenData.TryGetProperty("is_valid", out var valid) || valid.ValueKind != JsonValueKind.True
            || Text(tokenData, "app_id") != Options.Meta.AppId) throw AuthError();
        var scopes = tokenData.TryGetProperty("scopes", out var scopeList) ? scopeList.EnumerateArray().Select(x => x.GetString()!).ToArray() : [];
        // Page tokens may omit pages_show_list (a user-token permission).
        if (RequiredScopes(platform).Where(x => x != "pages_show_list").Except(scopes).Any()) throw AuthError();
        if (string.IsNullOrWhiteSpace(pageId)) throw InvalidResponse();
        using var profile = await Get(Query(Graph(Uri.EscapeDataString(pageId)), new() { ["fields"] = "id,name,instagram_business_account{id,username}" }), credentials.AccessToken, false, ct);
        var body = profile.RootElement;
        var id = Required(body, "id"); var name = Required(body, "name");
        if (id != pageId) throw AuthError();
        if (platform == PlatformType.Instagram)
        {
            var ig = body.GetProperty("instagram_business_account");
            id = Required(ig, "id"); name = Text(ig, "username") ?? name;
        }
        if (id != externalId) throw AuthError();
        return new()
        {
            ExternalId = id, PageId = pageId, Name = name, Scopes = scopes, Credentials = credentials,
            ExpiresAt = MinExpiry(UnixExpiry(tokenData, "expires_at"), UnixExpiry(tokenData, "data_access_expires_at"))
        };
    }

    private async Task<ProviderAccount> ZaloToken(Dictionary<string, string?> form, CancellationToken ct)
    {
        form["app_id"] = Options.Zalo.AppId;
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.zaloapp.com/v4/oa/access_token");
        request.Headers.Add("secret_key", Options.Zalo.AppSecret);
        request.Content = new FormUrlEncodedContent(form.Where(x => x.Value != null).Select(x => new KeyValuePair<string, string>(x.Key, x.Value!)));
        using var result = await Send(request, ct);
        return new()
        {
            Credentials = new() { AccessToken = Required(result.RootElement, "access_token"), RefreshToken = Required(result.RootElement, "refresh_token") },
            ExpiresAt = Expiry(result.RootElement, "expires_in") ?? throw InvalidResponse(),
            RefreshExpiresAt = Expiry(result.RootElement, "refresh_token_expires_in")
        };
    }
    private async Task<JsonDocument> Get(string url, string? token, bool zalo, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (token != null)
        {
            if (zalo) request.Headers.Add("access_token", token);
            else request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await Send(request, ct);
    }
    private async Task<JsonDocument> Send(HttpRequestMessage request, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(Options.RequestTimeoutSeconds));
        try
        {
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseContentRead, timeout.Token);
            if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new AccountException(429, "PLATFORM_RATE_LIMITED", "Nền tảng đang giới hạn yêu cầu; thử lại sau.");
            if ((int)response.StatusCode >= 500) throw new AccountException(502, "PLATFORM_PROVIDER_UNAVAILABLE", "Nền tảng tạm thời không phản hồi thành công.");
            if (!response.IsSuccessStatusCode) throw AuthError();
            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
            if (doc.RootElement.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null
                && !(error.ValueKind == JsonValueKind.Number && error.TryGetInt32(out var n) && n == 0))
            {
                doc.Dispose(); throw AuthError();
            }
            return doc;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { throw new AccountException(504, "PLATFORM_PROVIDER_TIMEOUT", "Hết thời gian chờ nền tảng; không tự thử lại trao đổi token."); }
        catch (HttpRequestException) { throw new AccountException(502, "PLATFORM_PROVIDER_UNAVAILABLE", "Không thể kết nối nền tảng."); }
        catch (JsonException) { throw InvalidResponse(); }
    }
    private DateTimeOffset? Expiry(JsonElement json, string key)
    {
        var text = Text(json, key);
        if (text == null) return null;
        if (!long.TryParse(text, out var seconds) || seconds <= 0 || seconds > 315360000) throw InvalidResponse();
        return clock.GetUtcNow().AddSeconds(seconds);
    }
    private static DateTimeOffset? UnixExpiry(JsonElement json, string key)
    {
        var value = Text(json, key);
        if (value == null || value == "0") return null;
        if (!long.TryParse(value, out var seconds) || seconds < 0 || seconds > 253402300799) throw InvalidResponse();
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }
    private static DateTimeOffset? MinExpiry(DateTimeOffset? a, DateTimeOffset? b) => a == null ? b : b == null ? a : a < b ? a : b;
    private static string? NextCursor(JsonElement root) => root.TryGetProperty("paging", out var paging) && paging.TryGetProperty("next", out _)
        ? Required(paging.GetProperty("cursors"), "after") : null;
    private static string? Text(JsonElement json, string key) => json.TryGetProperty(key, out var value) ? value.ValueKind switch
    { JsonValueKind.String => value.GetString(), JsonValueKind.Number => value.GetRawText(), _ => null } : null;
    private static string Required(JsonElement json, string key) => !string.IsNullOrWhiteSpace(Text(json, key)) ? Text(json, key)! : throw InvalidResponse();
    private static AccountException AuthError() => new(400, "PLATFORM_REAUTH_REQUIRED", "Nền tảng từ chối xác thực hoặc chưa cấp đủ quyền; hãy kết nối lại.");
    private static AccountException InvalidResponse() => new(502, "PLATFORM_INVALID_RESPONSE", "Phản hồi nền tảng không đúng contract.");
    private static string Query(string url, Dictionary<string, string?> values) => url + "?" + string.Join('&', values.Where(x => x.Value != null).Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value!)));
    private static string Base64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
