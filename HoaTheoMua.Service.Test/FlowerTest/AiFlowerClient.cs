using System.Net;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Test.PostGenTest;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class AiFlowerClient
{
    internal static Ai.Request.FlowerImageInput Input() => new("  Create prompt\n", " Mockup prompt\n", " Guard exact\n",
        new(Guid.NewGuid(), "Bó hồng", "https://example.com/product.png"),
        new[] { new Ai.Request.FlowerMaterial(Guid.NewGuid(), "Hồng", "https://example.com/rose.png", 10, 2, 12, "cành", "đỏ") },
        new(Guid.NewGuid(), "Gói tròn", "https://example.com/bundle.png", "Gói tròn", "Nếp gấp thấp"),
        new(Guid.NewGuid(), "Giấy", "https://example.com/paper.png", "trắng", "giấy kraft"),
        new(Guid.NewGuid(), "Nơ", "https://example.com/ribbon.png", "hồng", "lụa"),
        new(Guid.NewGuid(), "Cô gái", "https://example.com/mockup.png", "Người cầm bó hoa"));

    [Fact]
    public async Task SendsBothExactPromptsLabeledImagesAndFinalQuantitiesThenUsesGeneratedFlowerForMockup()
    {
        var input = Input();
        var payloads = new List<JsonElement>();
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            Assert.Equal("test-only", request.Headers.Authorization?.Parameter);
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            payloads.Add(json.RootElement.Clone());
            return AiContentClient.Completion($"![generated image](https://example.com/output{payloads.Count}.png)");
        });
        var result = await AiContentClient.Client(handler, new() { ["AiOption:ApiKey"] = "test-only" }).GenerateFlowerImage(input);
        Assert.Equal(2, handler.Calls);
        Assert.Equal("https://example.com/output1.png", result.FlowerImageUrl);
        Assert.Equal("https://example.com/output2.png", result.FlowerWithMockupImageUrl);
        Assert.Equal("chatcmpl-test", result.FlowerGeneration!.RequestId);
        Assert.Equal("chatcmpl-test", result.MockupGeneration!.RequestId);
        for (var i = 0; i < 2; i++)
        {
            var messages = payloads[i].GetProperty("messages");
            Assert.Equal("system", messages[0].GetProperty("role").GetString());
            Assert.Equal(i == 0 ? input.CreateSystemPrompt : input.MockupSystemPrompt,
                messages[0].GetProperty("content")[0].GetProperty("text").GetString());
            Assert.Equal(input.GuardSystemPrompt, messages[0].GetProperty("content")[1].GetProperty("text").GetString());
            Assert.Contains("không thêm logo", messages[0].GetProperty("content")[2].GetProperty("text").GetString());
            var parts = messages[1].GetProperty("content");
            for (var j = 0; j < parts.GetArrayLength(); j++)
                if (parts[j].GetProperty("type").GetString() == "image_url")
                    Assert.Equal("text", parts[j - 1].GetProperty("type").GetString());
        }
        var createParts = payloads[0].GetProperty("messages")[1].GetProperty("content");
        using var data = JsonDocument.Parse(createParts[1].GetProperty("text").GetString()!);
        var mat = data.RootElement.GetProperty("materials")[0];
        Assert.Equal(10, mat.GetProperty("baseQuantity").GetDecimal());
        Assert.Equal(2, mat.GetProperty("additionalQuantity").GetDecimal());
        Assert.Equal(12, mat.GetProperty("finalQuantity").GetDecimal());
        Assert.Equal(5, Images(createParts).Length);
        Assert.DoesNotContain(input.Mockup.ImageUrl, Images(createParts));
        Assert.Equal(new[] { result.FlowerImageUrl, input.Mockup.ImageUrl },
            Images(payloads[1].GetProperty("messages")[1].GetProperty("content")));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FailureStopsPipelineWithoutRetry(int failedStage)
    {
        var calls = 0;
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(++calls == failedStage
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : AiContentClient.Completion("![image](https://example.com/flower.png)")));
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(handler).GenerateFlowerImage(Input()));
        Assert.Equal(failedStage, handler.Calls);
    }

    [Theory]
    [InlineData("Không thể tạo theo dữ liệu này")]
    [InlineData("![a](https://example.com/a.png) ![b](https://example.com/b.png)")]
    [InlineData("![a](https://example.com/product.png)")]
    public async Task RejectsRefusalMultipleImagesAndEchoedReference(string output)
    {
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion(output)));
        await Assert.ThrowsAsync<BadGatewayException>(() => AiContentClient.Client(handler).GenerateFlowerImage(Input()));
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task ValidatesAllReferencesAndQuantitiesBeforeSpendingFirstCall()
    {
        var handler = new AiContentClient.Handler((_, _) => throw new Exception("Must not call provider"));
        var client = AiContentClient.Client(handler);
        var input = Input();
        await Assert.ThrowsAsync<BadRequestException>(() => client.GenerateFlowerImage(input with { GuardSystemPrompt = " " }));
        await Assert.ThrowsAsync<BadRequestException>(() => client.GenerateFlowerImage(input with { Mockup = input.Mockup with { ImageUrl = "" } }));
        await Assert.ThrowsAsync<BadRequestException>(() => client.GenerateFlowerImage(input with
        { Materials = new[] { input.Materials[0] with { FinalQuantity = 14 } } }));
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task CancellationBetweenStagesDoesNotCallMockup()
    {
        using var cts = new CancellationTokenSource();
        var handler = new AiContentClient.Handler((_, _) =>
        {
            cts.Cancel();
            return Task.FromResult(AiContentClient.Completion("![image](https://example.com/flower.png)"));
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AiContentClient.Client(handler).GenerateFlowerImage(Input(), cts.Token));
        Assert.Equal(1, handler.Calls);
    }

    private static string[] Images(JsonElement parts) => parts.EnumerateArray()
        .Where(x => x.GetProperty("type").GetString() == "image_url")
        .Select(x => x.GetProperty("image_url").GetProperty("url").GetString()!).ToArray();
}
