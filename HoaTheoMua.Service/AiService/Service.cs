using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.AiService;

public partial class Service(HttpClient httpClient, IConfiguration configuration, ILogger<Service> logger) : IService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Response.GeneratedContent> GenerateContent(Request.ContentInput input, CancellationToken cancellationToken = default)
    {
        var options = new AiOption();
        configuration.GetSection(nameof(AiOption)).Bind(options);
        if (!Uri.TryCreate(options.ChatCompletionsUrl, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("https" or "http") || !string.IsNullOrEmpty(uri.UserInfo)
            || string.IsNullOrWhiteSpace(options.Model) || options.TimeoutSeconds is < 1 or > 180)
            throw new ServerException("AI service configuration is invalid");

        var source = JsonSerializer.Serialize(new
        {
            platform = input.Platform, source = input.Source, previousContent = input.PreviousContent,
            feedback = input.Feedback, constraints = new { language = "vi" }
        }, JsonOptions);
        var payload = new Request.ChatCompletion(options.Model, new[]
        {
            new Request.Message("system", new[] { new Request.ContentPart("text", input.SystemPrompt) }),
            new Request.Message("user", new[] { new Request.ContentPart("text", source) })
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(payload, options: JsonOptions) };
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
        var watch = Stopwatch.StartNew();
        using var json = await SendAsync(request, timeout.Token, cancellationToken);
        try
        {
            var root = json.RootElement;
            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() != 1) throw InvalidResponse();
            var choice = choices[0];
            if (choice.GetProperty("finish_reason").GetString() != "stop") throw InvalidResponse();
            var content = choice.GetProperty("message").GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(content)) throw InvalidResponse();
            // Tolerate a single JSON code fence, but never extract arbitrary braces from prose.
            var text = content.Trim();
            if (text.StartsWith("```json\n", StringComparison.OrdinalIgnoreCase) && text.EndsWith("```")) text = text[8..^3].Trim();
            else if (text.StartsWith("```\n") && text.EndsWith("```")) text = text[4..^3].Trim();
            using var generated = JsonDocument.Parse(text);
            var title = generated.RootElement.GetProperty("title").GetString();
            var body = generated.RootElement.GetProperty("content").GetString();
            var hashtag = generated.RootElement.GetProperty("hashtag").GetString();
            var model = OptionalString(root, "model");
            var requestId = OptionalString(root, "id");
            if (title == null || string.IsNullOrWhiteSpace(body) || hashtag == null
                || title.Length > 500 || hashtag.Length > 1000 || model?.Length > 100 || requestId?.Length > 200)
                throw InvalidResponse();
            logger.LogInformation("PostGen AI content completed: request {RequestId}, duration {DurationMs}ms", requestId, watch.ElapsedMilliseconds);
            return new(title, body, hashtag, model, requestId, (int)watch.ElapsedMilliseconds);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw InvalidResponse();
        }
    }

    private async Task<JsonDocument> SendAsync(HttpRequestMessage request, CancellationToken timeout, CancellationToken caller)
    {
        try
        {
            // No retry: a lost response may still have incurred generation cost.
            using var response = await httpClient.SendAsync(request, timeout);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("PostGen AI HTTP request failed: status {Status}", (int)response.StatusCode);
                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity)
                    throw new BadRequestException("AI service rejected the request");
                throw new BadGatewayException("AI service is unavailable");
            }
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout));
        }
        catch (OperationCanceledException) when (!caller.IsCancellationRequested)
        {
            throw new BadGatewayException("AI service is unavailable");
        }
        catch (HttpRequestException) { throw new BadGatewayException("AI service is unavailable"); }
        catch (JsonException) { throw InvalidResponse(); }
    }

    private static string? OptionalString(JsonElement value, string key) => value.TryGetProperty(key, out var item) ? item.GetString() : null;
    private static BadGatewayException InvalidResponse() => new("AI service returned an invalid response");
}
