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
