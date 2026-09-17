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

public class GetPostCoreList
{
    [Fact]
    public async Task ChildrenIncludeAllPublicationStatesAndPreservePublishedSnapshot()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var post = await service.CreatePostCore(Input());
        var unpublished = await service.CreatePostCore(Input());
        var facebook = Publication(post.BasePostId, PlatformType.Facebook, "Succeeded", 1);
        var instagram = Publication(post.BasePostId, PlatformType.Instagram, "Succeeded", 2);
        db.PostPublications.AddRange(facebook, instagram);
        foreach (var status in new[] { "Queued", "Scheduled", "Publishing", "Failed", "Cancelled", "NeedsReview", "Closed" })
            db.PostPublications.Add(Publication(post.BasePostId, PlatformType.Facebook, status, 3));
        await db.SaveChangesAsync();

        var result = await service.GetPostCoreList(new());
        Assert.Equal(2, result.Value.TotalCount);
        var items = result.Value.Items.Cast<Response.PostCoreSummary>().ToList();
        Assert.Empty(items.Single(x => x.BasePostId == unpublished.BasePostId).Children);
        var children = items.Single(x => x.BasePostId == post.BasePostId).Children;
        Assert.Equal(9, children.Count);
        Assert.Equal(new Guid?[] { instagram.Id, facebook.Id }, children.Take(2).Select(x => x.PublicationId));
        Assert.All(children.Skip(2), x => Assert.Null(x.PublishedAt));
        Assert.Equal(new[] { "instagram", "facebook" }, children.Take(2).Select(x => x.Platform));
        var child = children[0];
        Assert.Equal(instagram.PlatformPostId, child.PlatformPostId);
        Assert.Equal(instagram.PlatformAccountId, child.PlatformAccountId);
        Assert.Equal("Succeeded", child.Status);
        Assert.Equal("Published title", child.Title);
        Assert.Equal("Published content", child.Content);
        Assert.Equal("#published", child.Hashtag);
        Assert.Equal(new[] { "https://example.com/first.jpg", "https://example.com/second.jpg" }, child.ImageUrls);
        Assert.Equal(instagram.ExternalPostId, child.ExternalPostId);
        Assert.Equal(instagram.ExternalPostUrl, child.ExternalPostUrl);
        Assert.Equal(instagram.CompletedAt, child.PublishedAt);
        Assert.Equal(children.Select(x => x.PublicationId),
            (await service.GetPostCoreDetail(post.BasePostId)).Children.Select(x => x.PublicationId));
    }

    [Theory]
    [InlineData(PlatformPostStatus.Draft)]
    [InlineData(PlatformPostStatus.Generating)]
    [InlineData(PlatformPostStatus.Generated)]
    [InlineData(PlatformPostStatus.Failed)]
    [InlineData(PlatformPostStatus.Ready)]
    public async Task UnpublishedPlatformUsesSelectedContentAndImages(PlatformPostStatus status)
    {
        await using var db = CreateDb(); var service = CreateService(db);
        var post = await service.CreatePostCore(Input());
        var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.BasePostId,
            Platform = PlatformType.Facebook, Status = status, OfficialGenerationId = Guid.NewGuid() };
        db.PlatformPosts.Add(platform);
        db.GeneratedPosts.Add(new() { Id = platform.OfficialGenerationId.Value, PlatformPostId = platform.Id,
            Title = "Selected", Content = "Selected content", Hashtag = "#selected" });
        db.GeneratedPosts.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id,
            Content = "Newer but not selected", CreatedAt = DateTimeOffset.UtcNow });
        var sourceId = post.BaseImages[0].Id;
        var image = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.BasePostId,
            SourceBasePostImageId = sourceId, SourceImageUrl = post.BaseImages[0].ImageUrl, RatioCode = AspectRatioType.Square1x1,
            Status = ImageStatus.Succeeded, ImageUrl = "https://example.com/selected.jpg" };
        db.GeneratedPostImages.Add(image);
        db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id,
            SourceBasePostImageId = sourceId, RatioCode = image.RatioCode, SelectedGeneratedPostImageId = image.Id });
        db.PlatformPosts.Add(new() { Id = Guid.NewGuid(), BasePostId = post.BasePostId, Platform = PlatformType.Zalo, IsDeleted = true });
        await db.SaveChangesAsync();
        var item = Assert.IsType<Response.PostCoreSummary>(Assert.Single((await service.GetPostCoreList(new())).Value.Items));
        var child = Assert.Single(item.Children);
        Assert.Equal(platform.Id, child.PlatformPostId); Assert.Equal(status.ToString(), child.Status);
        Assert.Null(child.PublicationId); Assert.Null(child.PlatformAccountId); Assert.Null(child.PublishedAt);
        Assert.Equal("Selected content", child.Content);
        Assert.Equal(new[] { image.ImageUrl }, child.ImageUrls);
        Assert.Equal(child.PlatformPostId, Assert.Single((await service.GetPostCoreDetail(post.BasePostId)).Children).PlatformPostId);
    }

    [Fact]
    public async Task RetriesUseLatestPublicationAndDraftWithoutSelectionHasNullContent()
    {
        await using var db = CreateDb(); var service = CreateService(db);
        var post = await service.CreatePostCore(Input());
        var older = Publication(post.BasePostId, PlatformType.Facebook, "Failed", 1);
        var latest = Publication(post.BasePostId, PlatformType.Facebook, "Queued", 2);
        latest.PlatformPostId = older.PlatformPostId;
        older.CreatedAt = DateTimeOffset.UtcNow.AddDays(-1); latest.CreatedAt = DateTimeOffset.UtcNow;
        db.PostPublications.AddRange(older, latest);
        db.PlatformPosts.Add(new() { Id = older.PlatformPostId, BasePostId = post.BasePostId,
            Platform = PlatformType.Facebook, Status = PlatformPostStatus.Publishing });
        await db.SaveChangesAsync();
        var item = Assert.IsType<Response.PostCoreSummary>(Assert.Single((await service.GetPostCoreList(new())).Value.Items));
        var child = Assert.Single(item.Children);
        Assert.Equal(latest.Id, child.PublicationId); Assert.Equal("Queued", child.Status);
        Assert.Null(child.PublishedAt); Assert.Null(child.Content); Assert.Empty(child.ImageUrls);
    }

    [Fact]
    public async Task ChildrenStayWithTheirParentAndDoNotChangePagination()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var older = await service.CreatePostCore(Input());
        var newer = await service.CreatePostCore(Input());
        (await db.BasePosts.SingleAsync(x => x.Id == older.BasePostId)).CreatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var oldPublication = Publication(older.BasePostId, PlatformType.Facebook, "Succeeded", 1);
        var newPublication = Publication(newer.BasePostId, PlatformType.Instagram, "Succeeded", 2);
        db.PostPublications.AddRange(oldPublication, newPublication);
        await db.SaveChangesAsync();
        var first = await service.GetPostCoreList(new() { PageIndex = 1, PageSize = 1 });
        Assert.True(first.Value.HasNextPage);
        var second = await service.GetPostCoreList(new() { PageIndex = 2, PageSize = 1 });
        Assert.Equal(2, second.Value.TotalCount);
        Assert.False(second.Value.HasNextPage);
        var item = Assert.IsType<Response.PostCoreSummary>(Assert.Single(second.Value.Items));
        Assert.Equal(older.BasePostId, item.BasePostId);
        Assert.Equal(oldPublication.Id, Assert.Single(item.Children).PublicationId);
        Assert.Empty((await service.GetPostCoreList(new() { PageIndex = 3, PageSize = 1 })).Value.Items);
    }

    private static PostPublication Publication(Guid basePostId, PlatformType platform, string status, int day)
    {
        var job = new PostPublication
        {
            Id = Guid.NewGuid(), BasePostId = basePostId, PlatformPostId = Guid.NewGuid(),
            PlatformAccountId = Guid.NewGuid(), Status = status,
            ExternalPostId = "external-post", ExternalPostUrl = "https://example.com/published",
            CompletedAt = new DateTimeOffset(2026, 9, day, 0, 0, 0, TimeSpan.Zero)
        };
        job.Snapshot = System.Text.Json.JsonSerializer.Serialize(new HoaTheoMua.Service.PostPublishing.PublicationSnapshot(
            basePostId, job.PlatformPostId, job.PlatformAccountId, "external-account", null, platform, "v25.0", "1",
            Guid.NewGuid(), "Published title", "Published content", "#published", "Published content #published",
            [new(Guid.NewGuid(), "https://example.com/second.jpg", 2), new(Guid.NewGuid(), "https://example.com/first.jpg", 1)]));
        return job;
    }

    [Fact]
    public async Task ListsSharedPostsAndFiltersDeletionKeywordAndPlatform()
    {
        await using var db = CreateDb();
        var user = Guid.NewGuid(); var service = CreateService(db);
        var own = await service.CreatePostCore(Input());
        var deleted = await service.CreatePostCore(Input());
        await service.DeletePostCore(deleted.BasePostId);
        await CreateService(db).CreatePostCore(Input());
        Assert.Equal(2, (await service.GetPostCoreList(new())).Value.TotalCount);
        db.PlatformPosts.Add(new PlatformPost { Id = Guid.NewGuid(), BasePostId = own.BasePostId, UserId = user, Platform = PlatformType.Facebook, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var result = await service.GetPostCoreList(new() { PageIndex = 0, PageSize = 999, Platform = PlatformType.Facebook, Keyword = "MÙA" });
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(100, result.Value.PageSize);
        Assert.Equal(own.BasePostId, Assert.IsType<Response.PostCoreSummary>(Assert.Single(result.Value.Items)).BasePostId);
        Assert.Empty((await service.GetPostCoreList(new() { Platform = PlatformType.Instagram })).Value.Items);
        var farPage = await service.GetPostCoreList(new() { PageIndex = int.MaxValue, PageSize = 100 });
        Assert.Empty(farPage.Value.Items);
        Assert.False(farPage.Value.HasNextPage);
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
