using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.PostCore;
using HoaTheoMua.Service.PostCore.Validate;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CoreService = HoaTheoMua.Service.PostCore.Service;
namespace HoaTheoMua.Service.Test.PostGenTest;

public class GetPostCoreDetail
{
    [Fact]
    public async Task SharedPostsAreReadableAndOnlyMissingOrDeletedPostsReturnNotFound()
    {
        await using var db = CreateDb();
        var owner = CreateService(db); var post = await owner.CreatePostCore(Input());
        Assert.Equal(post.BasePostId, (await CreateService(db).GetPostCoreDetail(post.BasePostId)).BasePostId);
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService(db).GetPostCoreDetail(Guid.NewGuid()));
        Assert.Equal("Post not found", error.Message);
        await owner.DeletePostCore(post.BasePostId);
        error = await Assert.ThrowsAsync<NotFoundException>(() => owner.GetPostCoreDetail(post.BasePostId));
        Assert.Equal("Post not found", error.Message);
    }
    [Fact]
    public async Task GroupsActivePlatformsAndPreservesImageHistoryWithoutTracking()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var post = await service.CreatePostCore(Input());
        var facebook = Guid.NewGuid();
        var instagram = Guid.NewGuid();
        var deleted = Guid.NewGuid();
        var generation = Guid.NewGuid();
        var image = Guid.NewGuid();
        db.PlatformPosts.AddRange(
            new PlatformPost { Id = instagram, BasePostId = post.BasePostId, Platform = PlatformType.Instagram, Status = PlatformPostStatus.Ready },
            new PlatformPost { Id = facebook, BasePostId = post.BasePostId, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Generated, OfficialGenerationId = generation },
            new PlatformPost { Id = deleted, BasePostId = post.BasePostId, Platform = PlatformType.Zalo, IsDeleted = true });
        db.GeneratedPosts.AddRange(
            new GeneratedPost { Id = generation, PlatformPostId = facebook, Content = "Facebook" },
            new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = deleted, Content = "Deleted" });
        db.GeneratedPostImages.Add(new GeneratedPostImage
        {
            Id = image, BasePostId = post.BasePostId, SourceBasePostImageId = post.BaseImages[0].Id,
            RatioCode = AspectRatioType.Square1x1, Status = ImageStatus.Succeeded,
            SourceImageUrl = post.BaseImages[0].ImageUrl, ImageUrl = "https://example.com/generated.jpg"
        });
        db.PlatformPostImages.AddRange(
            new PlatformPostImage { Id = Guid.NewGuid(), PlatformPostId = facebook, SourceBasePostImageId = post.BaseImages[1].Id, RatioCode = AspectRatioType.Square1x1, SortOrder = 2 },
            new PlatformPostImage { Id = Guid.NewGuid(), PlatformPostId = facebook, SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1, SelectedGeneratedPostImageId = image, SortOrder = 1 },
            new PlatformPostImage { Id = Guid.NewGuid(), PlatformPostId = instagram, SourceBasePostImageId = post.BaseImages[2].Id, RatioCode = AspectRatioType.Landscape16x9, SortOrder = 3 },
            new PlatformPostImage { Id = Guid.NewGuid(), PlatformPostId = deleted, SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1 });
        (await db.BasePostImages.SingleAsync(x => x.Id == post.BaseImages[0].Id)).IsDeleted = true;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await service.GetPostCoreDetail(post.BasePostId);

        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(new[] { 2, 3 }, result.BaseImages.Select(x => x.SortOrder));
        Assert.Equal(image, Assert.Single(result.Images).Id);
        Assert.Equal(new[] { "facebook", "instagram" }, result.Platforms.Select(x => x.Platform));
        Assert.Equal("Partial", result.Platforms[0].Status);
        Assert.Equal("Facebook", result.Platforms[0].Content!.Content);
        Assert.Equal(1, Assert.Single(result.Platforms[0].Versions).VersionNo);
        Assert.Equal(new[] { 1, 2 }, result.Platforms[0].Images.Select(x => x.SortOrder));
        Assert.Equal("Ready", result.Platforms[1].Status);
        Assert.Null(result.Platforms[1].Content);
        Assert.Empty(result.Platforms[1].Versions);
        Assert.Equal("Landscape16x9", Assert.Single(result.Platforms[1].Images).RatioCode);
    }
    [Fact]
    public async Task VersionsAreStablePerPlatformAndImagePairIncludingFailedAttempts()
    {
        await using var db = CreateDb(); var user = Guid.NewGuid(); var service = CreateService(db);
        var post = await service.CreatePostCore(Input()); var platformId = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        var first = Guid.Parse("00000000-0000-0000-0000-000000000001"); var second = Guid.Parse("00000000-0000-0000-0000-000000000002");
        db.PlatformPosts.Add(new PlatformPost { Id = platformId, BasePostId = post.BasePostId, UserId = user, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Generated, OfficialGenerationId = first });
        db.GeneratedPosts.AddRange(new GeneratedPost { Id = second, PlatformPostId = platformId, Content = "new", Source = ContentSource.Manual, CreatedAt = now }, new GeneratedPost { Id = first, PlatformPostId = platformId, Content = "old", CreatedAt = now });
        db.GeneratedPostImages.AddRange(new GeneratedPostImage { Id = second, BasePostId = post.BasePostId, SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1, Status = ImageStatus.Failed, SourceImageUrl = "https://example.com/a.jpg", CreatedAt = now }, new GeneratedPostImage { Id = first, BasePostId = post.BasePostId, SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1, Status = ImageStatus.Succeeded, SourceImageUrl = "https://example.com/a.jpg", ImageUrl = "https://example.com/out.jpg", CreatedAt = now });
        await db.SaveChangesAsync();
        var result = await service.GetPostCoreDetail(post.BasePostId);
        Assert.Equal("old", Assert.Single(result.Platforms).Content!.Content);
        Assert.Equal(new[] { 1, 2 }, result.Platforms[0].Versions.Select(x => x.VersionNo));
        Assert.Equal(new[] { first, second }, result.Images.Select(x => x.Id));
        Assert.Equal(new[] { 1, 2 }, result.Images.Select(x => x.VersionNo));
        var again = await service.GetPostCoreDetail(post.BasePostId);
        Assert.Equal(result.Images.Select(x => (x.Id, x.VersionNo)), again.Images.Select(x => (x.Id, x.VersionNo)));
    }
    private static AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static CoreService CreateService(AppDbContext db) => new(db);
    private static Request.CreatePostCoreRequest Input() => new()
    {
        Title = "Hoa mùa thu", Content = "Nội dung gốc", Hashtag = "#hoa",
        Images = new() {
            new() { ImageUrl = "https://example.com/2.jpg", SortOrder = 2 },
            new() { ImageUrl = "https://example.com/1.jpg", SortOrder = 1 },
            new() { ImageUrl = "https://example.com/3.jpg", SortOrder = 3 }
        }
    };
}
