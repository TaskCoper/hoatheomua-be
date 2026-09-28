using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class AiContentClient
{
    internal sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> action) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { Calls++; return action(request, cancellationToken); }
    }
    internal static Ai.Service Client(HttpMessageHandler handler, Dictionary<string, string?>? settings = null) => new(
        new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        new ConfigurationBuilder().AddInMemoryCollection(settings ?? new()).Build(), NullLogger<Ai.Service>.Instance);
    internal static HttpResponseMessage Completion(string content = "{\"title\":\"Title\",\"content\":\"Generated content\",\"hashtag\":\"#hoa\"}", string finish = "stop") => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new { id = "chatcmpl-test", model = "chatgpt/gpt-5.6", choices = new[] { new { index = 0, message = new { role = "assistant", content }, finish_reason = finish } } })
    };
    internal static Ai.Request.ContentInput Input() => new("  Prompt Facebook\nExact!  ", "facebook", new("Source title", "Source content", "#source"));

    [Theory]
    [InlineData(null)]
    [InlineData("test-only-key")]
    public async Task SendsExactPromptAndTextOnlySourceWithOptionalAuthentication(string? key)
    {
        var handler = new Handler(async (request, ct) =>
        {
            Assert.Equal("http://chatgpt-proxy-prod:1435/v1/chat/completions", request.RequestUri!.ToString());
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(key, request.Headers.Authorization?.Parameter);
            if (key != null) Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            Assert.Equal("chatgpt/gpt-5.6", body.RootElement.GetProperty("model").GetString());
            var messages = body.RootElement.GetProperty("messages");
            Assert.Equal(2, messages.GetArrayLength());
            Assert.Equal("system", messages[0].GetProperty("role").GetString());
            Assert.Equal(Input().SystemPrompt, messages[0].GetProperty("content")[0].GetProperty("text").GetString());
            var parts = messages[1].GetProperty("content");
            Assert.Equal(1, parts.GetArrayLength()); Assert.Equal("text", parts[0].GetProperty("type").GetString());
            using var source = JsonDocument.Parse(parts[0].GetProperty("text").GetString()!);
            Assert.Equal("Source content", source.RootElement.GetProperty("source").GetProperty("content").GetString());
            Assert.Equal(JsonValueKind.Null, source.RootElement.GetProperty("previousContent").ValueKind);
            Assert.False(source.RootElement.GetProperty("constraints").TryGetProperty("maxLength", out _));
            return Completion();
        });
        var result = await Client(handler, new() { ["AiOption:ApiKey"] = key }).GenerateContent(Input());
        Assert.Equal("Generated content", result.Content); Assert.Equal("chatcmpl-test", result.RequestId);
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("plain reply asking for clarification")]
    [InlineData("![generated image](https://example.com/a.png)")]
    [InlineData("{\"title\":\"t\",\"content\":\"\",\"hashtag\":\"\"}")]
    [InlineData("{\"title\":\"t\",\"content\":\"x\"}")]
    [InlineData("{\"title\":12,\"content\":\"x\",\"hashtag\":\"\"}")]
    [InlineData("null")]
    public async Task RejectsNonContentAndMalformedGeneratedJson(string content)
    {
        var handler = new Handler((_, _) => Task.FromResult(Completion(content)));
        await Assert.ThrowsAsync<BadGatewayException>(() => Client(handler).GenerateContent(Input()));
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("length")]
    [InlineData("content_filter")]
    [InlineData("tool_calls")]
    public async Task RejectsUnfinishedResponses(string finish)
    {
        await Assert.ThrowsAsync<BadGatewayException>(() => Client(new Handler((_, _) => Task.FromResult(Completion(finish: finish)))).GenerateContent(Input()));
    }

    [Theory]
    [InlineData(400)]
    [InlineData(422)]
    [InlineData(401)]
    [InlineData(429)]
    [InlineData(500)]
    public async Task MapsHttpErrorsWithoutRetryOrProviderBodyLeak(int status)
    {
        var handler = new Handler((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent("sensitive provider details") }));
        var error = await Assert.ThrowsAnyAsync<AppException>(() => Client(handler).GenerateContent(Input()));
        Assert.IsType(status is 400 or 422 ? typeof(BadRequestException) : typeof(BadGatewayException), error);
        Assert.DoesNotContain("sensitive", error.ToString()); Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task TimeoutMapsToGatewayButCallerCancellationPropagates()
    {
        var handler = new Handler(async (_, ct) => { await Task.Delay(Timeout.Infinite, ct); return Completion(); });
        await Assert.ThrowsAsync<BadGatewayException>(() => Client(handler, new() { ["AiOption:TimeoutSeconds"] = "1" }).GenerateContent(Input()));
        using var ct = new CancellationTokenSource(); ct.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client(handler).GenerateContent(Input(), ct.Token));
    }

    [Fact]
    public async Task AcceptsSingleJsonFenceAndRejectsInvalidEnvelope()
    {
        var output = await Client(new Handler((_, _) => Task.FromResult(Completion("```json\n{\"title\":\"\",\"content\":\"Body\",\"hashtag\":\"\"}\n```")))).GenerateContent(Input());
        Assert.Equal("Body", output.Content);
        await Assert.ThrowsAsync<BadGatewayException>(() => Client(new Handler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html>Error</html>") }))).GenerateContent(Input()));
    }

    [Fact]
    public void OrderedContentPartsKeepTextBetweenImages()
    {
        var message = new Ai.Request.Message("user", new[] {
            new Ai.Request.ContentPart("text", "Ảnh MỘT:"), new Ai.Request.ContentPart("image_url", ImageUrl: new("https://example.com/1.png")),
            new Ai.Request.ContentPart("text", "Ảnh HAI:"), new Ai.Request.ContentPart("image_url", ImageUrl: new("https://example.com/2.png")),
            new Ai.Request.ContentPart("text", "MỘT màu gì, HAI màu gì?") });
        var json = JsonSerializer.SerializeToElement(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var parts = json.GetProperty("content");
        Assert.Equal(new[] { "text", "image_url", "text", "image_url", "text" }, parts.EnumerateArray().Select(x => x.GetProperty("type").GetString()));
        Assert.Equal("Ảnh HAI:", parts[2].GetProperty("text").GetString());
        Assert.Equal("https://example.com/2.png", parts[3].GetProperty("image_url").GetProperty("url").GetString());
    }
}
