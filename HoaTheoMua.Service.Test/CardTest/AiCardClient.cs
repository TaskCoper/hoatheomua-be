using System.Net;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Test.PostGenTest;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.Test.CardTest;

public class AiCardClient
{
    private static Ai.Request.CardImageInput Input() => new(" exact card prompt\n", "https://example.com/bouquet.png",
        "https://example.com/template.png", "Chúc mừng sinh nhật!", "calligraphy", 10, 15,
        "https://example.com/attachment.png", "https://example.com/background.png");

    [Fact]
    public async Task SendsExactSystemPromptLabeledImagesTextAndPhysicalRatio()
    {
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            var messages = json.RootElement.GetProperty("messages");
            Assert.Equal(Input().SystemPrompt, messages[0].GetProperty("content")[0].GetProperty("text").GetString());
            var policy = messages[0].GetProperty("content")[1].GetProperty("text").GetString();
            Assert.Contains("10:15", policy); Assert.DoesNotContain("không thêm chữ", policy);
            var parts = messages[1].GetProperty("content");
            var images = parts.EnumerateArray().Where(x => x.GetProperty("type").GetString() == "image_url")
                .Select(x => x.GetProperty("image_url").GetProperty("url").GetString()).ToArray();
            Assert.Equal(new[] { Input().BouquetImageUrl, Input().TemplateImageUrl, Input().RawImage, Input().AttachedImageUrl }, images);
            for (var i = 1; i < parts.GetArrayLength(); i++)
                if (parts[i].GetProperty("type").GetString() == "image_url") Assert.Equal("text", parts[i-1].GetProperty("type").GetString());
            using var data = JsonDocument.Parse(parts[1].GetProperty("text").GetString()!);
            Assert.Equal(Input().MessageContent, data.RootElement.GetProperty("messageContent").GetString());
            return AiContentClient.Completion("![card](https://example.com/new.png)");
        });
        var result = await AiContentClient.Client(handler).GenerateCardImage(Input());
        Assert.Equal("https://example.com/new.png", result.ImageUrl); Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("No image")]
    [InlineData("![a](https://example.com/a.png) ![b](https://example.com/b.png)")]
    [InlineData("![a](https://example.com/template.png)")]
    public async Task InvalidOutputFailsWithoutRetry(string output)
    {
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion(output)));
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(handler).GenerateCardImage(Input()));
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task InvalidInputsAndCancellationNeverCallProvider()
    {
        var handler = new AiContentClient.Handler((_, _) => throw new Exception("Must not call"));
        var client = AiContentClient.Client(handler);
        await Assert.ThrowsAsync<BadRequestException>(() => client.GenerateCardImage(Input() with { Width = 0 }));
        await Assert.ThrowsAsync<BadRequestException>(() => client.GenerateCardImage(Input() with { SystemPrompt = " " }));
        await Assert.ThrowsAsync<BadRequestException>(() => client.GenerateCardImage(Input() with { AttachedImageUrl = "file:///tmp/a" }));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GenerateCardImage(Input(), new CancellationToken(true)));
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task ProviderFailureDoesNotRetry()
    {
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(handler).GenerateCardImage(Input()));
        Assert.Equal(1, handler.Calls);
    }
}
