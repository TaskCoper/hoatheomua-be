using System.Net;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class GenerationModes
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContentThenImagesNeverRegeneratesOrChangesOfficialContent(bool imageFails)
    {
        await using var db = Db(); var (id, imageIds) = await GenerateImages.Seed(db, 1);
        var textCalls = 0; var imageCalls = 0;
        var handler = new AiContentClient.Handler(async (r, ct) =>
        {
            using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct));
            if (!GenerateImages.IsImage(json.RootElement)) { textCalls++; return AiContentClient.Completion(); }
            imageCalls++;
            return imageFails ? new(HttpStatusCode.InternalServerError) : AiContentClient.Completion("![image](https://example.com/out.png)");
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var contentInput = GeneratePlatformPosts.Input(PlatformType.Facebook); contentInput.Mode = "content";
        var before = await service.GeneratePlatformPosts(id, contentInput);
        var selectedAt = (await db.PlatformPosts.SingleAsync()).SelectedAt;
        // No content prompt body is required when only generating images, but activation still is.
        (await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.facebook")).Value = "{\"isActive\":true}";
        await db.SaveChangesAsync();
        var imageInput = GenerateImages.Input(imageIds, PlatformType.Facebook); imageInput.Mode = "images";
        var after = await service.GeneratePlatformPosts(id, imageInput);
        Assert.Equal(1, textCalls); Assert.Equal(1, imageCalls); Assert.Single(db.GeneratedPosts);
        Assert.Equal(JsonSerializer.Serialize(before.Platforms[0].Content), JsonSerializer.Serialize(after.Platforms[0].Content));
        Assert.Equal(selectedAt, (await db.PlatformPosts.SingleAsync()).SelectedAt);
        Assert.Equal(before.Platforms[0].Content!.GenerationId, (await db.PlatformPosts.SingleAsync()).OfficialGenerationId);
        Assert.Equal(imageFails ? "Failed" : "Generated", after.Platforms[0].Status);
    }

    [Fact]
    public async Task ImagesBeforeContentCreatesNoContentAndContentOnlyKeepsImageSelections()
    {
        await using var db = Db(); var (id, imageIds) = await GenerateImages.Seed(db, 1);
        var textCalls = 0; var imageCalls = 0;
        var handler = new AiContentClient.Handler(async (r, ct) =>
        {
            using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct));
            if (!GenerateImages.IsImage(json.RootElement)) { textCalls++; return AiContentClient.Completion(); }
            imageCalls++; return AiContentClient.Completion("![image](https://example.com/out.png)");
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var images = GenerateImages.Input(imageIds, PlatformType.Facebook); images.Mode = "images";
        var before = await service.GeneratePlatformPosts(id, images);
        Assert.Empty(db.GeneratedPosts); Assert.Null(before.Platforms[0].Content); Assert.Equal(0, textCalls);
        var slotsBefore = await db.PlatformPostImages.AsNoTracking().ToListAsync();
        var instruction = await db.Configs.SingleAsync(x => x.Key == "postgen.image_instruction"); instruction.IsDeleted = true; await db.SaveChangesAsync();
        var content = GeneratePlatformPosts.Input(PlatformType.Facebook); content.Mode = "content";
        var after = await service.GeneratePlatformPosts(id, content);
        Assert.Equal(1, imageCalls); Assert.Equal(1, textCalls); Assert.Single(db.GeneratedPostImages);
        Assert.Equal(JsonSerializer.Serialize(before.Platforms[0].Images), JsonSerializer.Serialize(after.Platforms[0].Images));
        Assert.Equal(JsonSerializer.Serialize(slotsBefore), JsonSerializer.Serialize(await db.PlatformPostImages.AsNoTracking().ToListAsync()));
        Assert.Equal(1, after.Platforms[0].Content!.VersionNo);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("omitted")]
    public async Task AllAndOmittedModePreserveExistingCombinedFlow(string mode)
    {
        await using var db = Db(); var (id, imageIds) = await GenerateImages.Seed(db, 1);
        var handler = new AiContentClient.Handler(async (r, ct) =>
        {
            using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct));
            return GenerateImages.IsImage(json.RootElement) ? AiContentClient.Completion("![image](https://example.com/out.png)") : AiContentClient.Completion();
        });
        var input = GenerateImages.Input(imageIds, PlatformType.Facebook);
        if (mode != "omitted") input.Mode = mode;
        await GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input);
        Assert.Equal(2, handler.Calls); Assert.Single(db.GeneratedPosts); Assert.Single(db.GeneratedPostImages);
    }

    [Theory]
    [InlineData("unknown", true)]
    [InlineData(null, true)]
    [InlineData("content", true)]
    [InlineData("images", false)]
    public async Task InvalidModesOrConflictingSelectionsFailBeforeClaim(string? mode, bool hasImages)
    {
        await using var db = Db(); var (id, imageIds) = await GenerateImages.Seed(db, 1);
        var input = hasImages ? GenerateImages.Input(imageIds, PlatformType.Facebook) : GeneratePlatformPosts.Input(PlatformType.Facebook);
        input.Mode = mode!;
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        await Assert.ThrowsAsync<ValidationExceptionError>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input));
        Assert.Equal(0, handler.Calls); Assert.Empty(db.PlatformPosts);
    }

    [Fact]
    public async Task ImageOnlyStillRejectsDisabledPlatforms()
    {
        await using var db = Db(); var (id, imageIds) = await GenerateImages.Seed(db, 1);
        (await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.facebook")).Value = "{\"isActive\":false}"; await db.SaveChangesAsync();
        var input = GenerateImages.Input(imageIds, PlatformType.Facebook); input.Mode = "images";
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        await Assert.ThrowsAsync<BadRequestException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input));
        Assert.Equal(0, handler.Calls); Assert.Empty(db.PlatformPosts);
    }
}
