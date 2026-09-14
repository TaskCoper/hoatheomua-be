using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class GetOfficialVersion
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static HoaTheoMua.Service.PostGeneration.Service Service(AppDbContext db) => GeneratePlatformPosts.Service(db,
        AiContentClient.Client(new AiContentClient.Handler((_, _) => throw new Exception("Official must not call AI"))));

    [Fact]
    public async Task ReturnsExactOldManualAndSelectedImagesInOrderWithoutWritesOrOtherPlatforms()
    {
        await using var db = Db(); var (post, platform, old, image) = await Seed(db);
        var newVersion = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Content = "Newer AI not chosen", Source = ContentSource.Ai, CreatedAt = old.CreatedAt.AddSeconds(1) };
        db.GeneratedPosts.Add(newVersion);
        var alternateImage = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.Id, SourceBasePostImageId = image.SourceBasePostImageId, RatioCode = AspectRatioType.Story9x16, Status = ImageStatus.Succeeded, SourceImageUrl = image.SourceImageUrl, ImageUrl = "https://example.com/story.png", PixelWidth = 1080, PixelHeight = 1920 };
        db.GeneratedPostImages.Add(alternateImage);
        db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id, SourceBasePostImageId = image.SourceBasePostImageId, RatioCode = alternateImage.RatioCode, SelectedGeneratedPostImageId = alternateImage.Id, SortOrder = 1 });
        var other = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = PlatformType.Instagram };
        var foreign = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = other.Id, Content = "Other platform", Source = ContentSource.Ai };
        other.OfficialGenerationId = foreign.Id; db.PlatformPosts.Add(other); db.GeneratedPosts.Add(foreign);
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var before = await Snapshot(db);
        var service = Service(db);
        var result = await service.GetOfficialVersion(post.Id, "FACEBOOK");
        Assert.True(result.IsReady); Assert.Equal("Ready", result.Status); Assert.Empty(result.NotReadyReasons);
        Assert.Equal(old.Id, result.GenerationId); Assert.Equal(old.Title, result.Title); Assert.Equal(old.Content, result.Content); Assert.Equal(old.Hashtag, result.Hashtag);
        Assert.Equal(platform.SelectedAt, result.SelectedAt);
        Assert.Equal(new[] { alternateImage.Id, image.Id }, result.Images.Select(x => x.SelectedGeneratedPostImageId));
        Assert.Equal(new[] { 1, 2 }, result.Images.Select(x => x.SortOrder));
        Assert.Equal("https://example.com/story.png", result.Images[0].ImageUrl);
        Assert.Equal(before, await Snapshot(db)); Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(await service.GetOfficialVersion(post.Id, "facebook")));
        var payload = JsonSerializer.SerializeToElement(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.False(payload.TryGetProperty("versions", out _)); Assert.False(payload.TryGetProperty("baseImages", out _));
        Assert.DoesNotContain("Newer AI not chosen", payload.ToString()); Assert.DoesNotContain("Other platform", payload.ToString());
    }

    [Theory]
    [InlineData(false, false, "Draft")]
    [InlineData(true, false, "Generated")]
    [InlineData(false, true, "Draft")]
    [InlineData(true, true, "Ready")]
    public async Task ReadinessUsesActualOfficialDataInsteadOfStoredReadyFlag(bool content, bool images, string status)
    {
        await using var db = Db(); var (post, platform, _, _) = await Seed(db);
        platform.Status = PlatformPostStatus.Ready;
        if (!content) platform.OfficialGenerationId = null;
        if (!images) (await db.PlatformPostImages.SingleAsync()).SelectedGeneratedPostImageId = null;
        await db.SaveChangesAsync();
        var result = await Service(db).GetOfficialVersion(post.Id, "facebook");
        Assert.Equal(content && images, result.IsReady); Assert.Equal(status, result.Status);
        Assert.Equal(!content, result.NotReadyReasons.Contains("MissingContent"));
        Assert.Equal(!images, result.NotReadyReasons.Contains("MissingImages"));
        Assert.Equal(PlatformPostStatus.Ready, (await db.PlatformPosts.AsNoTracking().SingleAsync()).Status);
        if (!content) { Assert.Null(result.GenerationId); Assert.Null(result.Title); Assert.Null(result.Content); Assert.Null(result.Hashtag); }
    }

    [Theory]
    [InlineData(PlatformPostStatus.Generating, "GenerationInProgress")]
    [InlineData(PlatformPostStatus.Scheduled, "PublishingState")]
    [InlineData(PlatformPostStatus.Publishing, "PublishingState")]
    [InlineData(PlatformPostStatus.Published, "PublishingState")]
    [InlineData(PlatformPostStatus.PublishFailed, "PublishingState")]
    public async Task ReturnsCurrentSelectionButNotReadyDuringGenerationOrPublishing(PlatformPostStatus state, string reason)
    {
        await using var db = Db(); var (post, platform, old, _) = await Seed(db);
        platform.Status = state; await db.SaveChangesAsync();
        var result = await Service(db).GetOfficialVersion(post.Id, "facebook");
        Assert.False(result.IsReady); Assert.Equal(state.ToString(), result.Status); Assert.Equal(new[] { reason }, result.NotReadyReasons);
        Assert.Equal(old.Id, result.GenerationId); Assert.Single(result.Images);
    }

    [Theory]
    [InlineData("failed")]
    [InlineData("empty-url")]
    [InlineData("wrong-ratio")]
    [InlineData("wrong-source")]
    [InlineData("wrong-post")]
    public async Task InvalidSelectedImageNeverMakesPayloadReady(string scenario)
    {
        await using var db = Db(); var (post, _, _, image) = await Seed(db);
        switch (scenario)
        {
            case "failed": image.Status = ImageStatus.Failed; image.ImageUrl = null; break;
            case "empty-url": image.ImageUrl = "   "; break;
            case "wrong-ratio": image.RatioCode = AspectRatioType.Story9x16; break;
            case "wrong-source": image.SourceBasePostImageId = Guid.NewGuid(); break;
            case "wrong-post": image.BasePostId = Guid.NewGuid(); break;
        }
        await db.SaveChangesAsync();
        var result = await Service(db).GetOfficialVersion(post.Id, "facebook");
        Assert.False(result.IsReady); Assert.Equal(new[] { "MissingImages" }, result.NotReadyReasons); Assert.Empty(result.Images);
    }

    [Fact]
    public async Task MissingOrForeignOfficialDoesNotFallbackAndDeletedResourcesReturn404()
    {
        await using var db = Db(); var (post, platform, old, _) = await Seed(db);
        old.PlatformPostId = Guid.NewGuid(); await db.SaveChangesAsync();
        var service = Service(db);
        var missing = await service.GetOfficialVersion(post.Id, "facebook");
        Assert.Null(missing.GenerationId); Assert.Null(missing.Content); Assert.False(missing.IsReady);
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetOfficialVersion(post.Id, "zalo"));
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetOfficialVersion(Guid.NewGuid(), "facebook"));
        await Assert.ThrowsAsync<ValidationExceptionError>(() => service.GetOfficialVersion(post.Id, "0"));
        await Assert.ThrowsAsync<ValidationExceptionError>(() => service.GetOfficialVersion(post.Id, "tiktok"));
        platform.IsDeleted = true; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetOfficialVersion(post.Id, "facebook"));
        post.IsDeleted = true; await db.SaveChangesAsync();
        Assert.Equal("Post not found", (await Assert.ThrowsAsync<NotFoundException>(() => service.GetOfficialVersion(post.Id, "facebook"))).Message);
    }

    [Fact]
    public async Task FailedLastAttemptAndRemovedOriginalDoNotInvalidateSuccessfulOfficialSnapshots()
    {
        await using var db = Db(); var (post, platform, _, image) = await Seed(db);
        platform.Status = PlatformPostStatus.Failed; platform.LastError = "Later generation failed";
        (await db.BasePostImages.SingleAsync()).IsDeleted = true; await db.SaveChangesAsync();
        var result = await Service(db).GetOfficialVersion(post.Id, "facebook");
        Assert.True(result.IsReady); Assert.Equal(image.Id, Assert.Single(result.Images).SelectedGeneratedPostImageId);
        Assert.Equal(PlatformPostStatus.Failed, (await db.PlatformPosts.SingleAsync()).Status);
    }

    private static async Task<(BasePost, PlatformPost, GeneratedPost, GeneratedPostImage)> Seed(AppDbContext db)
    {
        var post = new BasePost { Id = Guid.NewGuid(), Title = "Core title", Content = "Core text must not leak" };
        var source = new BasePostImage { Id = Guid.NewGuid(), BasePostId = post.Id, ImageUrl = "https://example.com/original.png", SortOrder = 1 };
        var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Generated, SelectedAt = DateTimeOffset.UtcNow };
        var content = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Title = "Chosen manual", Content = "Exact body\nSecond line", Hashtag = "#Exact", Source = ContentSource.Manual, CreatedAt = DateTimeOffset.UtcNow };
        platform.OfficialGenerationId = content.Id;
        var image = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.Id, SourceBasePostImageId = source.Id, SourceImageUrl = source.ImageUrl, RatioCode = AspectRatioType.Square1x1, ImageUrl = "https://example.com/chosen.png", Status = ImageStatus.Succeeded, PixelWidth = 1080, PixelHeight = 1080 };
        db.AddRange(post, source, platform, content, image);
        db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id, SourceBasePostImageId = source.Id, RatioCode = image.RatioCode, SelectedGeneratedPostImageId = image.Id, SortOrder = 2 });
        await db.SaveChangesAsync(); return (post, platform, content, image);
    }
    private static async Task<string> Snapshot(AppDbContext db) => JsonSerializer.Serialize(new
    {
        Platforms = await db.PlatformPosts.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
        Content = await db.GeneratedPosts.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
        Images = await db.GeneratedPostImages.AsNoTracking().OrderBy(x => x.Id).ToListAsync(),
        Slots = await db.PlatformPostImages.AsNoTracking().OrderBy(x => x.Id).ToListAsync()
    });
}
