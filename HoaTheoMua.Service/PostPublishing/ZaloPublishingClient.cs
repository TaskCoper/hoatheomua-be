using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.PlatformAccount;

namespace HoaTheoMua.Service.PostPublishing;

// Provider messages may contain credentials; only stable codes cross this boundary.
public sealed class ZaloPublishingException(string code, bool retryable = false, bool definitive = false,
    string? providerCode = null, int retryAfter = 60) : Exception("Zalo publishing request failed")
{
    public string? ProviderCode { get; } = providerCode;
    public PublicationResult Result(bool mayHavePublished, bool reconcile = false) => new(
        mayHavePublished && (!definitive || reconcile) ? PublicationOutcome.Unknown : PublicationOutcome.Rejected)
    {
        ErrorCode = code, Retryable = retryable, ProviderCode = providerCode, RetryAfterSeconds = retryAfter,
        ConfirmedNotPublished = !reconcile && (!mayHavePublished || definitive),
        RequiresReauth = code is "ZALO_TOKEN_INVALID" or "ZALO_PERMISSION_DENIED"
    };
}

public sealed class ZaloPublishingClient(HttpClient http)
{
    public async Task<JsonElement> Send(string path, Credentials credentials, object? body, CancellationToken ct)
    {
        // Only fixed OA endpoints are allowed, never a response-provided URL.
        if (path is not ("oa/getoa" or "article/create" or "article/verify") && !path.StartsWith("article/getdetail?id=", StringComparison.Ordinal))
            throw new ZaloPublishingException("ZALO_INVALID_TARGET", definitive: true);
        using var request = new HttpRequestMessage(body == null ? HttpMethod.Get : HttpMethod.Post, "https://openapi.zalo.me/v2.0/" + path);
        request.Headers.Add("access_token", credentials.AccessToken);
        if (body != null) request.Content = JsonContent.Create(body);
        try
        {
            using var response = await http.SendAsync(request, ct);
            var delay = (int)Math.Clamp(response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60, 1, 3600);
            if ((int)response.StatusCode is 401 or 403 or 429)
                throw new ZaloPublishingException((int)response.StatusCode switch
                { 401 => "ZALO_TOKEN_INVALID", 403 => "ZALO_PERMISSION_DENIED", _ => "ZALO_RATE_LIMITED" },
                    true, true, ((int)response.StatusCode).ToString(), delay);
            if (!response.IsSuccessStatusCode) throw new ZaloPublishingException("ZALO_UNAVAILABLE", true);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var error) || error.ValueKind != JsonValueKind.Number || !error.TryGetInt32(out var code))
                throw new ZaloPublishingException("ZALO_INVALID_RESPONSE", true);
            // Documented parameter rejections are different from an ambiguous transport failure.
            // Keep only allowlisted field names, never the raw message (which may echo secrets).
            if (code is -201 or -210)
                throw new ZaloPublishingException(InvalidParameterCode(root), definitive: true, providerCode: code.ToString());
            if (code == -214) throw new ZaloPublishingException("ZALO_PROCESSING", true, providerCode: code.ToString(), retryAfter: 30);
            // Unknown application errors are not evidence that a create request had no effect.
            if (code == -124) throw new ZaloPublishingException("ZALO_TOKEN_INVALID", true, true, code.ToString());
            if (code != 0) throw new ZaloPublishingException("ZALO_REQUEST_REJECTED", providerCode: code.ToString());
            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
                throw new ZaloPublishingException("ZALO_INVALID_RESPONSE", true);
            return data.Clone();
        }
        catch (JsonException) { throw new ZaloPublishingException("ZALO_INVALID_RESPONSE", true); }
        catch (HttpRequestException) { throw new ZaloPublishingException("ZALO_TRANSPORT_ERROR", true); }
        catch (OperationCanceledException) { throw new ZaloPublishingException("ZALO_TIMEOUT", true); }
    }

    private static string InvalidParameterCode(JsonElement root)
    {
        if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
        {
            var text = message.GetString()?.Trim();
            foreach (var field in new[] { "type", "title", "author", "description", "cover", "cover.photo_url", "photo_url", "body", "related_medias", "tracking_link", "status", "comment" })
                if (string.Equals(text, field + " is invalid!", StringComparison.OrdinalIgnoreCase))
                    return "ZALO_INVALID_PARAMETER_" + field.Replace('.', '_').ToUpperInvariant();
        }
        return "ZALO_INVALID_MEDIA_OR_PARAMETER";
    }
}
