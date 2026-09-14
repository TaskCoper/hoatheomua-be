using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class AiImageClient
{
    private static Ai.Request.ImageInput Input() => new("https://example.com/source.png", "Square1x1", 1080, 1080, " Custom shared instruction\n");
    [Fact]
    public async Task SendsOrderedSourceAndSharedInstructionAndParsesMarkdownWithMetadata()
    {
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            var messages = json.RootElement.GetProperty("messages"); Assert.Equal(1, messages.GetArrayLength());
            var parts = messages[0].GetProperty("content");
            Assert.Equal(new[] { "text", "image_url", "text", "text" }, parts.EnumerateArray().Select(x => x.GetProperty("type").GetString()));
            Assert.Equal(Input().SourceImageUrl, parts[1].GetProperty("image_url").GetProperty("url").GetString());
            Assert.Equal(Input().Instruction, parts[2].GetProperty("text").GetString());
            Assert.Contains("1080x1080", parts[3].GetProperty("text").GetString());
            Assert.DoesNotContain("facebook", json.RootElement.ToString());
            return AiContentClient.Completion("Đã tạo ảnh:\n![generated image](https://example.com/out.png?x=1&y=2)");
        });
        var result = await AiContentClient.Client(handler, new() { ["AiOption:ApiKey"] = "test-only-key" }).GenerateImage(Input());
        Assert.Equal("https://example.com/out.png?x=1&y=2", result.ImageUrl); Assert.Equal("chatcmpl-test", result.RequestId); Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("Bạn muốn tạo ảnh gì?")]
    [InlineData("[link](https://example.com/out.png)")]
    [InlineData("![image](data:image/png;base64,xxx)")]
    [InlineData("![image](https://user:password@example.com/out.png)")]
    [InlineData("![one](https://example.com/1.png) ![two](https://example.com/2.png)")]
    public async Task RejectsMissingAmbiguousAndInvalidImageUrlsWithoutRetry(string content)
    {
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion(content)));
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(handler).GenerateImage(Input()));
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task ImageTimeoutBecomesGatewayErrorAndUnfinishedOutputIsRejected()
    {
        var handler = new AiContentClient.Handler(async (_, ct) => { await Task.Delay(Timeout.Infinite, ct); return AiContentClient.Completion(); });
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(handler, new() { ["AiOption:TimeoutSeconds"] = "1" }).GenerateImage(Input()));
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion("![image](https://example.com/out.png)", "length")))).GenerateImage(Input()));
    }
}
