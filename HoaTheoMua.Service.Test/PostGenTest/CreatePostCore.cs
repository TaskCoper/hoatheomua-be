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

public class CreatePostCore
{
    [Fact]
    public async Task CreatesSharedPostAndOrdersThreeImagesWithoutHistoryWrites()
    {
        await using var db = CreateDb();
        var result = await CreateService(db).CreatePostCore(Input());
        Assert.Equal(result.BasePostId, (await db.BasePosts.SingleAsync()).Id);
        Assert.Equal(new[] { 1, 2, 3 }, result.BaseImages.Select(x => x.SortOrder));
        Assert.Empty(result.Images);
        Assert.Empty(result.Platforms);
        Assert.Empty(db.PostHistories);
        Assert.Empty(db.ClientHistories);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void RejectsImageCountOutsideLimits(int count)
    {
        var input = Input();
        input.Images = Enumerable.Range(1, count).Select(x => new Request.ImageInput { ImageUrl = "https://example.com/a.jpg", SortOrder = x }).ToList();
        Assert.False(new CreatePostCoreValidator().Validate(input).IsValid);
    }
    [Theory]
    [InlineData("file:///tmp/image.jpg")]
    [InlineData("ftp://example.com/image.jpg")]
    [InlineData("/image.jpg")]
    [InlineData("")]
    public void RejectsInvalidUrl(string url)
    {
        var input = Input(); input.Images![0].ImageUrl = url;
        Assert.False(new CreatePostCoreValidator().Validate(input).IsValid);
    }
    [Fact]
    public void RejectsDuplicateOrderNullItemsAndExcessiveText()
    {
        var input = Input(); input.Images![0].SortOrder = 1;
        Assert.False(new CreatePostCoreValidator().Validate(input).IsValid);
        input = Input(); input.Images![0] = null!;
        Assert.False(new CreatePostCoreValidator().Validate(input).IsValid);
        input = Input(); input.Title = new string('a', 501); input.Content = new string('b', 20001); input.Hashtag = new string('c', 1001);
        var errors = new CreatePostCoreValidator().Validate(input).Errors;
        Assert.Contains(errors, x => x.PropertyName == "Title");
        Assert.Contains(errors, x => x.PropertyName == "Content");
        Assert.Contains(errors, x => x.PropertyName == "Hashtag");
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
