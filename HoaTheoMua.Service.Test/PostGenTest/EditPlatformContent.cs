using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class EditPlatformContent
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task ConsecutiveManualSavesCoalesceButReferencedAndUnselectedManualVersionsStayImmutable()
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var first = await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook)); var v1 = first.Platforms[0].Content!.GenerationId;
        var manual = await service.EditPlatformContent(id, "facebook", new() { Content = "Manual A" }); var v2 = manual.Platforms[0].Content!;
        await service.EditPlatformContent(id, "facebook", new() { Title = "Manual title", Content = "Manual B" });
        Assert.Equal(2, await db.GeneratedPosts.CountAsync()); Assert.Equal(v2.CreatedAt, (await db.GeneratedPosts.SingleAsync(x => x.Id == v2.GenerationId)).CreatedAt);
        await service.RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = v2.GenerationId });
        await service.SelectOfficialContent(id, "facebook", new() { GenerationId = v2.GenerationId });
        var edited = await service.EditPlatformContent(id, "facebook", new() { Content = "Manual branch" });
        Assert.Equal(4, await db.GeneratedPosts.CountAsync()); Assert.Equal(v2.GenerationId, edited.Platforms[0].Content!.ParentGeneratedPostId);
        Assert.Equal("Manual B", (await db.GeneratedPosts.SingleAsync(x => x.Id == v2.GenerationId)).Content);
        await service.SelectOfficialContent(id, "facebook", new() { GenerationId = v1 });
        var fromOld = await service.EditPlatformContent(id, "facebook", new() { Content = "Edit oldest" });
        Assert.Equal(5, await db.GeneratedPosts.CountAsync()); Assert.Equal(v1, fromOld.Platforms[0].Content!.ParentGeneratedPostId);
        Assert.Equal("Manual branch", (await db.GeneratedPosts.SingleAsync(x => x.Id == edited.Platforms[0].Content!.GenerationId)).Content);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task ManualEditNeedsNoPromptAndPreservesReadyAndImageSlots()
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook));
        var platform = await db.PlatformPosts.SingleAsync(); platform.Status = PlatformPostStatus.Ready;
        db.Configs.RemoveRange(db.Configs);
        var imageId = Guid.NewGuid(); var sourceId = (await db.BasePostImages.SingleAsync()).Id;
        db.GeneratedPostImages.Add(new() { Id = imageId, BasePostId = id, SourceBasePostImageId = sourceId, RatioCode = AspectRatioType.Square1x1, SourceImageUrl = "https://example.com/source.png", ImageUrl = "https://example.com/out.png", Status = ImageStatus.Succeeded });
        db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id, SourceBasePostImageId = sourceId, RatioCode = AspectRatioType.Square1x1, SelectedGeneratedPostImageId = imageId, SortOrder = 1 });
        await db.SaveChangesAsync();
        var result = await service.EditPlatformContent(id, "facebook", new() { Content = "Manual", Title = null, Hashtag = null });
        Assert.Equal("Ready", result.Platforms[0].Status); Assert.Equal(imageId, Assert.Single(result.Platforms[0].Images).SelectedGeneratedPostImageId); Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData(PlatformPostStatus.Generating)]
    [InlineData(PlatformPostStatus.Scheduled)]
    [InlineData(PlatformPostStatus.Published)]
    public async Task LockedStatesRejectMutations(PlatformPostStatus state)
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()))));
        var original = await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook));
        (await db.PlatformPosts.SingleAsync()).Status = state; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => service.EditPlatformContent(id, "facebook", new() { Content = "Denied" }));
        await Assert.ThrowsAsync<ConflictException>(() => service.SelectOfficialContent(id, "facebook", new() { GenerationId = original.Platforms[0].Content!.GenerationId }));
        await Assert.ThrowsAsync<ConflictException>(() => service.RegeneratePlatformContent(id, "facebook", new()));
        Assert.Single(db.GeneratedPosts);
    }
}
