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

public class UpdatePostCore
{
    [Fact]
    public async Task ReordersRetainedImagesAndSoftDeletesRemovedSources()
    {
        await using var db = CreateDb(); var service = CreateService(db); var post = await service.CreatePostCore(Input());
        var request = new Request.UpdatePostCoreRequest { Title = "Sửa title", Content = post.Content, Images = post.BaseImages.Take(2).Reverse().Select((x, i) => new Request.ImageInput { Id = x.Id, ImageUrl = x.ImageUrl, SortOrder = i + 1 }).ToList() };
        request.Images.Add(new() { ImageUrl = "https://example.com/new.jpg", SortOrder = 3 });
        var result = await service.UpdatePostCore(post.BasePostId, request);
        Assert.Equal("Sửa title", result.Title);
        Assert.Equal(post.BaseImages[1].Id, result.BaseImages[0].Id);
        Assert.True((await db.BasePostImages.SingleAsync(x => x.Id == post.BaseImages[2].Id)).IsDeleted);
        Assert.Equal(4, await db.BasePostImages.CountAsync());
        Assert.NotNull(result.UpdatedAt);
    }
    [Fact]
    public async Task ForeignImageAndUrlMutationFailBeforeChangingPost()
    {
        await using var db = CreateDb(); var service = CreateService(db); var post = await service.CreatePostCore(Input());
        var request = new Request.UpdatePostCoreRequest { Title = "changed", Content = "changed", Images = new() { new() { Id = Guid.NewGuid(), ImageUrl = "https://example.com/new.jpg", SortOrder = 1 } } };
        var error = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePostCore(post.BasePostId, request));
        Assert.Equal("Image does not belong to this post", error.Message);
        request.Images[0].Id = post.BaseImages[0].Id;
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePostCore(post.BasePostId, request));
        Assert.Equal(post.Title, (await service.GetPostCoreDetail(post.BasePostId)).Title);
        Assert.Equal(3, await db.BasePostImages.CountAsync(x => !x.IsDeleted));
    }
    [Fact]
    public async Task AnotherServiceCanUpdateTheSharedPost()
    {
        await using var db = CreateDb(); var service = CreateService(db); var post = await service.CreatePostCore(Input());
        await CreateService(db).UpdatePostCore(post.BasePostId, new() { Title = "x", Content = "y", Images = Input().Images });
        Assert.Equal("x", (await service.GetPostCoreDetail(post.BasePostId)).Title);
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
