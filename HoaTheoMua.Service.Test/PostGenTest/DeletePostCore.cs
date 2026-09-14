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

public class DeletePostCore
{
    [Fact]
    public async Task SoftDeletesPostSourcesAndPlatformsAndPreservesVersions()
    {
        await using var db = CreateDb(); var user = Guid.NewGuid(); var service = CreateService(db); var post = await service.CreatePostCore(Input());
        var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.BasePostId, Platform = PlatformType.Facebook, UserId = user };
        db.PlatformPosts.Add(platform);
        db.GeneratedPosts.Add(new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Content = "history" });
        await db.SaveChangesAsync();
        var result = await CreateService(db).DeletePostCore(post.BasePostId);
        Assert.True(result.IsDeleted);
        Assert.True((await db.BasePosts.SingleAsync()).IsDeleted);
        Assert.All(db.BasePostImages, x => Assert.True(x.IsDeleted));
        Assert.True(platform.IsDeleted);
        Assert.Single(db.GeneratedPosts);
        Assert.Empty((await service.GetPostCoreList(new())).Value.Items);
    }
    [Fact]
    public async Task GeneratingPostRejectsUpdateAndDelete()
    {
        await using var db = CreateDb(); var user = Guid.NewGuid(); var service = CreateService(db); var post = await service.CreatePostCore(Input());
        db.PlatformPosts.Add(new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.BasePostId, UserId = user, Status = PlatformPostStatus.Generating });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => service.DeletePostCore(post.BasePostId));
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdatePostCore(post.BasePostId, new() { Title = "x", Content = "y", Images = Input().Images }));
        Assert.False((await db.BasePosts.SingleAsync()).IsDeleted);
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
