using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Service.PlatformAccount;

namespace HoaTheoMua.Service.PostPublishing;

// No raw response, URL or token is included in exceptions or persisted results.
public sealed class MetaPublishingException(string code, bool retryable = false, bool definitive = false,
    string? providerCode = null, string? requestId = null, int retryAfter = 60) : Exception("Meta publishing request failed")
{
    public bool ReadOnlyRequest { get; internal set; }
    public PublicationResult Result(bool mayHavePublished, bool reconcile = false) => new(
        mayHavePublished && (!definitive || reconcile) ? PublicationOutcome.Unknown : PublicationOutcome.Rejected)
    {
        ErrorCode = code, Retryable = retryable, ConfirmedNotPublished = !reconcile && (!mayHavePublished || definitive),
        ProviderCode = providerCode, ProviderRequestId = requestId, RetryAfterSeconds = retryAfter,
        RequiresReauth = code is "META_TOKEN_INVALID" or "META_PERMISSION_DENIED"
    };
}

public sealed class MetaPublishingClient(HttpClient http)
{
    public static bool Id(string? value) => value != null && Regex.IsMatch(value, @"\A[0-9]{1,100}\z");
    public static bool PostId(string? value) => value != null && Regex.IsMatch(value, @"\A[0-9]{1,100}_[0-9]{1,100}\z");
    public static string? Text(JsonElement json, string key) => json.ValueKind == JsonValueKind.Object && json.TryGetProperty(key, out var v)
        ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.ValueKind == JsonValueKind.Number ? v.GetRawText() : null : null;
    public static string RequiredId(JsonElement json, string key = "id") => Id(Text(json, key)) ? Text(json, key)! : throw new MetaPublishingException("META_INVALID_RESPONSE");
    public Task<JsonElement> Get(PublicationSnapshot s, Credentials c, string path, string fields, CancellationToken ct)
        => Send(s, c, path, new() { ["fields"] = fields }, false, ct);
    public async Task<JsonElement> Send(PublicationSnapshot s, Credentials c, string path, Dictionary<string, string> values, bool post, CancellationToken ct)
    {
        if (!Regex.IsMatch(s.ApiVersion, @"\Av[0-9]+\.[0-9]+\z") || !Regex.IsMatch(path, @"\A[0-9_]+(?:/[a-z_]+)?\z"))
            throw new MetaPublishingException("META_INVALID_TARGET", definitive: true);
        var url = $"https://graph.facebook.com/{s.ApiVersion}/{path}";
        if (!post) url += "?" + string.Join('&', values.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value)));
        using var request = new HttpRequestMessage(post ? HttpMethod.Post : HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.AccessToken);
        if (post) request.Content = new FormUrlEncodedContent(values);
        try
        {
            using var response = await http.SendAsync(request, ct);
            var delay = (int)Math.Clamp(response.Headers.RetryAfter?.Delta?.TotalSeconds
                ?? (response.Headers.RetryAfter?.Date - DateTimeOffset.UtcNow)?.TotalSeconds ?? 60, 1, 3600);
            JsonElement json;
            try { using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct)); json = doc.RootElement.Clone(); }
            catch (JsonException) { throw new MetaPublishingException("META_INVALID_RESPONSE", retryable: true); }
            if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                _ = int.TryParse(Text(error, "code"), out var code);
                _ = int.TryParse(Text(error, "error_subcode"), out var subcode);
                var category = code switch
                {
                    190 or 102 => "META_TOKEN_INVALID", 10 or >= 200 and <= 299 => "META_PERMISSION_DENIED",
                    4 or 17 or 32 or 613 => "META_RATE_LIMITED", 100 => "META_INVALID_MEDIA_OR_PARAMETER",
                    1 or 2 => "META_UNAVAILABLE", _ => "META_REQUEST_REJECTED"
                };
                if ((int)response.StatusCode == 429) category = "META_RATE_LIMITED";
                var definitive = (int)response.StatusCode is >= 400 and < 500
                    && category is "META_TOKEN_INVALID" or "META_PERMISSION_DENIED" or "META_RATE_LIMITED" or "META_INVALID_MEDIA_OR_PARAMETER";
                var trace = Text(error, "fbtrace_id");
                if (trace == null || !Regex.IsMatch(trace, @"\A[A-Za-z0-9_-]{1,200}\z")) trace = null;
                throw new MetaPublishingException(category, category != "META_INVALID_MEDIA_OR_PARAMETER", definitive,
                    $"{(int)response.StatusCode}:{code}:{subcode}", trace, delay);
            }
            if (!response.IsSuccessStatusCode) throw new MetaPublishingException((int)response.StatusCode == 429 ? "META_RATE_LIMITED" : "META_UNAVAILABLE", true, retryAfter: delay);
            if (json.ValueKind != JsonValueKind.Object) throw new MetaPublishingException("META_INVALID_RESPONSE");
            return json;
        }
        catch (MetaPublishingException ex) { ex.ReadOnlyRequest = !post; throw; }
        catch (HttpRequestException) { throw new MetaPublishingException("META_TRANSPORT_ERROR", true) { ReadOnlyRequest = !post }; }
        catch (OperationCanceledException) { throw new MetaPublishingException("META_TIMEOUT", true) { ReadOnlyRequest = !post }; }
    }
}
