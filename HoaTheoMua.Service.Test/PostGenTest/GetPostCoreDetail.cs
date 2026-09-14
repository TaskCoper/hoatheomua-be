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

        Assert.Equal(new[] { "facebook", "instagram" }, result.Platforms.Select(x => x.Platform));
        Assert.Equal("Partial", result.Platforms[0].Status);
        Assert.Equal("Facebook", result.Platforms[0].Content!.Content);
        Assert.Equal(1, result.Platforms[0].Content!.VersionNo);
        var selectedImage = Assert.Single(result.Platforms[0].Images);
        Assert.Equal(image, selectedImage.SelectedGeneratedPostImageId);
        Assert.Equal("https://example.com/generated.jpg", selectedImage.ImageUrl);
        Assert.Equal(1, selectedImage.SortOrder);
        Assert.Equal("Ready", result.Platforms[1].Status);
        Assert.Null(result.Platforms[1].Content);
        Assert.Empty(result.Platforms[1].Images);
    }
    [Fact]
    public async Task OnlyOfficialContentIsReturnedWhileAllHistoryRemainsStored()
    {
        await using var db = CreateDb(); var user = Guid.NewGuid(); var service = CreateService(db);
        var post = await service.CreatePostCore(Input()); var platformId = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        var first = Guid.Parse("00000000-0000-0000-0000-000000000001"); var second = Guid.Parse("00000000-0000-0000-0000-000000000002");
        db.PlatformPosts.Add(new PlatformPost { Id = platformId, BasePostId = post.BasePostId, UserId = user, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Generated, OfficialGenerationId = first });
        db.GeneratedPosts.AddRange(new GeneratedPost { Id = second, PlatformPostId = platformId, Content = "new", Source = ContentSource.Manual, CreatedAt = now }, new GeneratedPost { Id = first, PlatformPostId = platformId, Content = "old", CreatedAt = now });
        db.GeneratedPostImages.AddRange(new GeneratedPostImage { Id = second, BasePostId = post.BasePostId, SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1, Status = ImageStatus.Failed, SourceImageUrl = "https://example.com/a.jpg", CreatedAt = now }, new GeneratedPostImage { Id = first, BasePostId = post.BasePostId, SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1, Status = ImageStatus.Succeeded, SourceImageUrl = "https://example.com/a.jpg", ImageUrl = "https://example.com/out.jpg", CreatedAt = now });
        await db.SaveChangesAsync();
        db.PlatformPostImages.Add(new PlatformPostImage { Id = Guid.NewGuid(), PlatformPostId = platformId,
            SourceBasePostImageId = post.BaseImages[0].Id, RatioCode = AspectRatioType.Square1x1,
            SelectedGeneratedPostImageId = second, SortOrder = 1 }); // A failed image is never returned as official.
        await db.SaveChangesAsync();
        var result = await service.GetPostCoreDetail(post.BasePostId);
        Assert.Equal("old", Assert.Single(result.Platforms).Content!.Content);
        Assert.Equal(1, result.Platforms[0].Content!.VersionNo);
        var json = System.Text.Json.JsonSerializer.SerializeToElement(result, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        Assert.False(json.GetProperty("platforms")[0].TryGetProperty("versions", out _));
        Assert.Equal(2, await db.GeneratedPosts.CountAsync()); // Response does not delete history.
        (await db.PlatformPosts.SingleAsync()).OfficialGenerationId = second;
        await db.SaveChangesAsync();
        var selectedSecond = await service.GetPostCoreDetail(post.BasePostId);
        Assert.Equal(second, selectedSecond.Platforms[0].Content!.GenerationId);
        Assert.Equal(2, selectedSecond.Platforms[0].Content!.VersionNo);
        (await db.PlatformPosts.SingleAsync()).OfficialGenerationId = null;
        await db.SaveChangesAsync();
        Assert.Null((await service.GetPostCoreDetail(post.BasePostId)).Platforms[0].Content);
        Assert.False(json.TryGetProperty("images", out _));
        Assert.Empty(result.Platforms[0].Images);
        Assert.Equal(2, await db.GeneratedPostImages.CountAsync());
        var again = await service.GetPostCoreDetail(post.BasePostId);
        Assert.Empty(again.Platforms[0].Images);
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
