using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class FormPromptConfigs
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static string Form(object? value) => JsonSerializer.Serialize(new
    {
        group = "postgen", label = "UI metadata must not reach AI", value,
        options = Array.Empty<object>(), required = false, inputType = "textarea", validations = Array.Empty<object>()
    });

    [Theory]
    [InlineData("Setting")]
    [InlineData("setting")]
    [InlineData(" SETTING ")]
    public async Task AllPlatformsAndRegenerationUseFormValueVerbatimAndKeepSnapshots(string kind)
    {
        await using var db = Db(); var (id, images) = await GenerateImages.Seed(db, 1);
        foreach (var name in new[] { "facebook", "instagram", "zalo" })
            (await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt." + name)).Value = Form("  Prompt " + name + "\n");
        const string instruction = "  Giữ nguyên hoa, mở rộng nền.\n";
        (await db.Configs.SingleAsync(x => x.Key == "postgen.image_instruction")).Value = Form(instruction);
        db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.enabled", Kind = "Setting", Value = Form(true) });
        foreach (var config in db.ChangeTracker.Entries<HoaTheoMua.Repository.Entity.Config>())
            config.Entity.Kind = kind;
        await db.SaveChangesAsync();
        var prompts = new List<string>(); var imageCalls = 0;
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            Assert.DoesNotContain("UI metadata", body.RootElement.ToString());
            var parts = body.RootElement.GetProperty("messages")[0].GetProperty("content");
            if (!GenerateImages.IsImage(body.RootElement))
            {
                prompts.Add(parts[0].GetProperty("text").GetString()!);
                return AiContentClient.Completion();
            }
            imageCalls++;
            Assert.Contains(parts.EnumerateArray(), p => p.TryGetProperty("text", out var text) && text.GetString() == instruction);
            return AiContentClient.Completion("![generated image](https://example.com/result.jpg)");
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        await service.GeneratePlatformPosts(id, GenerateImages.Input(images, PlatformType.Facebook, PlatformType.Instagram, PlatformType.Zalo));
        Assert.Equal(3, prompts.Count); Assert.Equal(1, imageCalls);
        foreach (var platform in await db.PlatformPosts.ToListAsync())
        {
            var prompt = "  Prompt " + platform.Platform.ToString().ToLowerInvariant() + "\n";
            Assert.Contains(prompt, prompts);
            Assert.Equal(prompt, (await db.GeneratedPosts.SingleAsync(x => x.PlatformPostId == platform.Id)).PromptSnapshot);
            await service.RegeneratePlatformContent(id, platform.Platform.ToString().ToLowerInvariant(), new());
            Assert.Equal(prompt, prompts.Last());
        }
        await service.RegenerateImage(id, new() { SourceBasePostImageId = images[0], RatioCode = AspectRatioType.Square1x1 });
        Assert.Equal(2, imageCalls);
    }

    [Theory]
    [InlineData("{\"value\":\"\"}")]
    [InlineData("{\"value\":\"  \"}")]
    [InlineData("{\"value\":null}")]
    [InlineData("{\"value\":123}")]
    [InlineData("{\"value\":{\"text\":\"prompt\"}}")]
    [InlineData("{\"label\":\"Prompt\"}")]
    [InlineData("{\"value\":\"\",\"content\":\"Do not fall back\",\"isActive\":true}")]
    [InlineData("{\"value\":\"prompt\",\"isActive\":\"true\"}")]
    [InlineData("{\"value\":\"prompt\",\"description\":\"{\\\"isActive\\\":\\\"true\\\"}\"}")]
    public async Task InvalidFormPromptsAreRejectedBeforeCallingAi(string value)
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        (await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.facebook")).Value = value; await db.SaveChangesAsync();
        var calls = 0; var handler = new AiContentClient.Handler((_, _) => { calls++; return Task.FromResult(AiContentClient.Completion()); });
        var input = GeneratePlatformPosts.Input(PlatformType.Facebook); input.Mode = "content";
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input));
        Assert.Equal("System prompt for platform facebook is not configured", ex.Message); Assert.Equal(0, calls);
        Assert.Empty(db.GeneratedPosts);
    }

    [Theory]
    [InlineData("{\"value\":\"prompt\",\"isActive\":false}", "content")]
    [InlineData("{\"value\":\"\",\"isActive\":false}", "images")]
    [InlineData("{\"value\":\"prompt\",\"description\":\"{\\\"logo\\\":\\\"https://example.com/icon.png\\\",\\\"isActive\\\":false}\"}", "content")]
    [InlineData("{\"value\":\"\",\"description\":\"{\\\"isActive\\\":false}\"}", "images")]
    public async Task ExplicitDisabledFlagsRemainEffective(string value, string mode)
    {
        await using var db = Db(); var (id, images) = await GenerateImages.Seed(db, 1);
        (await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.facebook")).Value = value; await db.SaveChangesAsync();
        var calls = 0; var handler = new AiContentClient.Handler((_, _) => { calls++; return Task.FromResult(AiContentClient.Completion()); });
        var input = mode == "content" ? GeneratePlatformPosts.Input(PlatformType.Facebook) : GenerateImages.Input(images, PlatformType.Facebook);
        input.Mode = mode;
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input));
        Assert.Equal("Platform facebook is disabled", ex.Message); Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("{\"value\":\"\"}")]
    [InlineData("{\"value\":false}")]
    [InlineData("{\"value\":null}")]
    [InlineData("{\"label\":\"Missing value\"}")]
    public async Task InvalidImageInstructionIsRejectedForRegeneration(string value)
    {
        await using var db = Db(); var (id, images) = await GenerateImages.Seed(db, 1);
        (await db.Configs.SingleAsync(x => x.Key == "postgen.image_instruction")).Value = value; await db.SaveChangesAsync();
        var calls = 0; var handler = new AiContentClient.Handler((_, _) => { calls++; return Task.FromResult(AiContentClient.Completion()); });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id,
            new() { SourceBasePostImageId = images[0], RatioCode = AspectRatioType.Square1x1 }));
        Assert.Equal("Image instruction is not configured", ex.Message); Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("{\"value\":false}", "PostGen is disabled")]
    [InlineData("{\"value\":\"true\"}", "PostGen enabled config is invalid")]
    [InlineData("{\"label\":\"Missing value\"}", "PostGen enabled config is invalid")]
    public async Task FormEnabledUsesStrictBoolean(string value, string message)
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.enabled", Kind = "Setting", Value = value });
        await db.SaveChangesAsync();
        var handler = new AiContentClient.Handler((_, _) => throw new Exception("AI must not be called"));
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler))
            .GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook)));
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public async Task ImagesOnlyAllowsEmptyPlatformPromptInNewForm()
    {
        await using var db = Db(); var (id, images) = await GenerateImages.Seed(db, 1);
        (await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.instagram")).Value = Form(""); await db.SaveChangesAsync();
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion("![image](https://example.com/test.jpg)")));
        var input = GenerateImages.Input(images, PlatformType.Instagram); input.Mode = "images";
        await GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input);
        Assert.Single(db.GeneratedPostImages); Assert.Empty(db.GeneratedPosts);
    }
}
