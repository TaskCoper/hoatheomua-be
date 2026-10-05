using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.Flower;

public interface ITaskcoperStorageSession
{
    void ValidateConfiguration();
    Task<string> PresignPngAsync(string key, CancellationToken cancellationToken);
}

// One service-account session per process; serialize refresh so concurrent generations do not
// rotate the same refresh token twice. Tokens/cookies remain in memory and never enter history/logs.
public sealed class TaskcoperStorageSession(IHttpClientFactory clients, IOptions<FlowerImageOptions> settings)
    : ITaskcoperStorageSession, IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CookieContainer _cookies = new();
    private string? _accessToken;
    private string? _refreshToken;
    private bool _authenticated;
    private FlowerImageOptions Options => settings.Value;

    public void ValidateConfiguration()
    {
        if (!Uri.TryCreate(Options.StorageApiBase, UriKind.Absolute, out var uri)
            || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
            || string.IsNullOrWhiteSpace(Options.StorageEmail) || string.IsNullOrWhiteSpace(Options.StoragePassword))
            throw new ServerException("Taskcoper storage account configuration is missing or invalid");
    }

    public async Task<string> PresignPngAsync(string key, CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!_authenticated && !await AuthenticateAsync(refresh: false, cancellationToken)) throw AuthError();
            using var initial = await RequestPresignAsync(key, cancellationToken);
            if (initial.StatusCode != HttpStatusCode.Unauthorized)
                return await ReadPresignAsync(initial, cancellationToken);
            if (!await AuthenticateAsync(refresh: true, cancellationToken)
                && !await AuthenticateAsync(refresh: false, cancellationToken)) throw AuthError();
            using var retry = await RequestPresignAsync(key, cancellationToken);
            if (retry.StatusCode == HttpStatusCode.Unauthorized) _authenticated = false;
            return await ReadPresignAsync(retry, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException
            or CookieException or HttpRequestException) { throw AuthError(); }
        finally { _gate.Release(); }
    }

    private async Task<bool> AuthenticateAsync(bool refresh, CancellationToken cancellationToken)
    {
        if (!refresh) { _cookies = new(); _accessToken = null; _refreshToken = null; }
        var uri = Endpoint(refresh ? "users/refresh_token" : "users/login");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        if (refresh)
        {
            AddAuthentication(request);
            // Cookie-based API is current; JSON supports older token-returning API versions.
            if (_refreshToken != null)
                request.Content = JsonContent.Create(new { accessToken = _accessToken, refreshToken = _refreshToken });
        }
        else request.Content = JsonContent.Create(new { email = Options.StorageEmail, password = Options.StoragePassword });
        using var response = await clients.CreateClient(FlowerImageBranding.StorageClient).SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) { _authenticated = false; return false; }
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            foreach (var cookie in cookies) _cookies.SetCookies(uri, cookie);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var value = body.RootElement.GetProperty("value");
        var scope = Endpoint("storages/presigned-url");
        _accessToken = _cookies.GetCookies(scope)["accessToken"]?.Value ?? ReadToken(value, "accessToken");
        _refreshToken = _cookies.GetCookies(uri)["refreshToken"]?.Value ?? ReadToken(value, "refreshToken");
        _authenticated = !string.IsNullOrWhiteSpace(_accessToken);
        return _authenticated;
    }

    private Task<HttpResponseMessage> RequestPresignAsync(string key, CancellationToken cancellationToken)
    {
        var uri = new Uri(Endpoint("storages/presigned-url") + $"?key={Uri.EscapeDataString(key)}&contentType=image%2Fpng");
        return Send();
        async Task<HttpResponseMessage> Send()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            AddAuthentication(request);
            return await clients.CreateClient(FlowerImageBranding.StorageClient).SendAsync(request, cancellationToken);
        }
    }

    private void AddAuthentication(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        var cookie = _cookies.GetCookieHeader(request.RequestUri!);
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
    }

    private static async Task<string> ReadPresignAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode) throw AuthError();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var value = json.RootElement.GetProperty("value");
        var url = value.GetProperty("presignedUrl").GetString();
        if (value.GetProperty("uploadMethod").GetString() != "PUT"
            || value.GetProperty("contentType").GetString() != "image/png"
            || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https"
            || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment) || uri.Port != 443)
            throw AuthError();
        return url!;
    }

    private Uri Endpoint(string path) => new($"{Options.StorageApiBase.TrimEnd('/')}/api/v1/{path}");
    private static string? ReadToken(JsonElement value, string key) => value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(key, out var token) && token.ValueKind == JsonValueKind.String ? token.GetString() : null;
    private static BadGatewayException AuthError() => new("Taskcoper storage authentication or presign failed");
    public void Dispose() => _gate.Dispose();
}
