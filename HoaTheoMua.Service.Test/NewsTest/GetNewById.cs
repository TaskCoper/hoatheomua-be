using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using HoaTheoMua.Service.Test.TestSupport;
using static HoaTheoMua.Service.Test.TestSupport.ReflectionHelper;
using NewsEntity = HoaTheoMua.Repository.Entity.News;
using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryNewsDetailEntity = HoaTheoMua.Repository.Entity.CategoryNewsDetail;
using NewsService = HoaTheoMua.Service.New.Service;

namespace HoaTheoMua.Service.Test.NewsTest;

public class GetNewByIdTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NewsId =
        Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly Guid CategoryId =
        Guid.Parse("44444444-4444-4444-4444-444444444441");

    private static readonly Guid NotFoundNewsId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static NewsService CreateService(
        List<NewsEntity> newsList,
        out Mock<AppDbContext> dbContextMock)
    {
        var newsDbSetMock = BuildMockDbSet(newsList);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.NewsList)
            .Returns(newsDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(UserId);

        return new NewsService(dbContextMock.Object, currentUserMock.Object);
    }

    private static Mock<DbSet<T>> BuildMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();

        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(sourceList.GetEnumerator()));

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.Expression)
            .Returns(queryable.Expression);

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.ElementType)
            .Returns(queryable.ElementType);

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => sourceList.GetEnumerator());

        return dbSetMock;
    }

    private static CategoryNewsEntity CreateCategory(
        Guid? id = null,
        string title = "Technology",
        bool isDeleted = false)
    {
        var category = Activator.CreateInstance<CategoryNewsEntity>()!;

        SetPropertyIfExists(category, "Id", id ?? CategoryId);
        SetPropertyIfExists(category, "Title", title);
        SetPropertyIfExists(category, "Slug", "technology");
        SetPropertyIfExists(category, "IsDeleted", isDeleted);
        SetPropertyIfExists(category, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-3));
        SetPropertyIfExists(category, "UpdatedAt", null);

        return category;
    }

    private static CategoryNewsDetailEntity CreateCategoryNewsDetail(
        Guid? id = null,
        Guid? newsId = null,
        Guid? categoryId = null,
        CategoryNewsEntity? category = null,
        bool isDeleted = false)
    {
        var detail = Activator.CreateInstance<CategoryNewsDetailEntity>()!;

        SetPropertyIfExists(detail, "Id", id ?? Guid.NewGuid());
        SetPropertyIfExists(detail, "NewsId", newsId ?? NewsId);
        SetPropertyIfExists(detail, "CategoryNewsId", categoryId ?? CategoryId);
        SetPropertyIfExists(detail, "CategoryNews", category ?? CreateCategory());
        SetPropertyIfExists(detail, "IsDeleted", isDeleted);
        SetPropertyIfExists(detail, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-2));
        SetPropertyIfExists(detail, "UpdatedAt", null);

        return detail;
    }

    private static NewsEntity CreateNews(
        Guid? id = null,
        string title = "Công Nghệ Mới",
        string description = "Bài viết về công nghệ mới...",
        int viewCount = 150,
        string coverImage = "https://example.com/image.png",
        string slug = "cong-nghe-moi",
        NewsStatus status = NewsStatus.Published,
        bool isDeleted = false,
        List<CategoryNewsDetailEntity>? categoryNewsDetails = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        var news = Activator.CreateInstance<NewsEntity>()!;

        SetPropertyIfExists(news, "Id", id ?? NewsId);
        SetPropertyIfExists(news, "Title", title);
        SetPropertyIfExists(news, "Description", description);
        SetPropertyIfExists(news, "ViewCount", viewCount);
        SetPropertyIfExists(news, "CoverImage", coverImage);
        SetPropertyIfExists(news, "slug", slug);
        SetPropertyIfExists(news, "Status", status);
        SetPropertyIfExists(news, "UserId", UserId);
        SetPropertyIfExists(news, "AuthorName", "Admin User");
        SetPropertyIfExists(news, "AuthorEmail", "admin@example.com");
        SetPropertyIfExists(news, "AuthorAvatar", "https://example.com/avatar.png");
        SetPropertyIfExists(news, "IsDeleted", isDeleted);
        SetPropertyIfExists(news, "CreatedAt", createdAt ?? DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"));
        SetPropertyIfExists(news, "UpdatedAt", updatedAt);

        SetPropertyIfExists(
            news,
            "CategoryNewsDetail",
            categoryNewsDetails ?? new List<CategoryNewsDetailEntity>
            {
                CreateCategoryNewsDetail()
            });

        return news;
    }

    // TC01
    [Fact]
    public async Task GetNewById_ByGuid_Success()
    {
        var category = CreateCategory(
            id: CategoryId,
            title: "Technology");

        var detail = CreateCategoryNewsDetail(
            newsId: NewsId,
            categoryId: CategoryId,
            category: category);

        var news = CreateNews(
            id: NewsId,
            slug: "cong-nghe-moi",
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewByIdAsync(NewsId.ToString());

        Assert.NotNull(result);
        Assert.Equal(NewsId, result!.Id);
        Assert.Equal("Công Nghệ Mới", result.Title);
        Assert.Equal("Bài viết về công nghệ mới...", result.Description);
        Assert.Equal(150, result.ViewCount);
        Assert.Equal("https://example.com/image.png", result.PictureUrl);
        Assert.Equal("cong-nghe-moi", result.slug);
        Assert.Equal("Published", result.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"), result.CreatedAt);
        Assert.Null(result.UpdatedAt);

        Assert.NotNull(result.Categories);
        Assert.Single(result.Categories);
        Assert.Equal(CategoryId, result.Categories[0].CategoryId);
        Assert.Equal("Technology", result.Categories[0].CategoryName);

        Assert.NotNull(result.User);
        Assert.Equal(UserId, result.User.UserId);
        Assert.Equal("admin@example.com", result.User.Email);
        Assert.Equal("https://example.com/avatar.png", result.User.UserAvatar);
        Assert.Equal("Admin User", result.User.UserFullName);
    }

    // TC02
    [Fact]
    public async Task GetNewById_BySlug_Success()
    {
        var category = CreateCategory(
            id: CategoryId,
            title: "Technology");

        var detail = CreateCategoryNewsDetail(
            newsId: NewsId,
            categoryId: CategoryId,
            category: category);

        var news = CreateNews(
            id: NewsId,
            slug: "cong-nghe-moi",
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewByIdAsync("cong-nghe-moi");

        Assert.NotNull(result);
        Assert.Equal(NewsId, result!.Id);
        Assert.Equal("Công Nghệ Mới", result.Title);
        Assert.Equal("cong-nghe-moi", result.slug);
        Assert.Equal("Published", result.Status);

        Assert.Single(result.Categories);
        Assert.Equal(CategoryId, result.Categories[0].CategoryId);
        Assert.Equal("Technology", result.Categories[0].CategoryName);

        Assert.Equal(UserId, result.User.UserId);
        Assert.Equal("admin@example.com", result.User.Email);
        Assert.Equal("Admin User", result.User.UserFullName);
    }

    // TC03
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Cong-Nghe-Moi")]
    [InlineData("cong_nghe_moi")]
    [InlineData("cong--nghe-moi")]
    [InlineData("cong-nghe-moi-")]
    [InlineData("công-nghệ-mới")]
    public async Task GetNewById_InvalidKey_ThrowBadRequest(string key)
    {
        var newsList = new List<NewsEntity>
        {
            CreateNews()
        };

        var service = CreateService(newsList, out _);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetNewByIdAsync(key));

        Assert.Equal("NEW_LOOKUP_KEY_INVALID", ex.Message);
    }

    // TC03
    [Fact]
    public async Task GetNewById_KeyTooLong_ThrowBadRequest()
    {
        var newsList = new List<NewsEntity>
        {
            CreateNews()
        };

        var service = CreateService(newsList, out _);

        var key = new string('a', 101);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetNewByIdAsync(key));

        Assert.Equal("NEW_LOOKUP_KEY_INVALID", ex.Message);
    }

    // TC04
    [Fact]
    public async Task GetNewById_DeletedNewsByGuid_ThrowNotFound()
    {
        var deletedNews = CreateNews(
            id: NewsId,
            slug: "cong-nghe-moi",
            isDeleted: true);

        var newsList = new List<NewsEntity>
        {
            deletedNews
        };

        var service = CreateService(newsList, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetNewByIdAsync(NewsId.ToString()));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
    }

    // TC05
    [Fact]
    public async Task GetNewById_GuidNotFound_ThrowNotFound()
    {
        var newsList = new List<NewsEntity>
        {
            CreateNews(id: NewsId)
        };

        var service = CreateService(newsList, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetNewByIdAsync(NotFoundNewsId.ToString()));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
    }

    // TC06
    [Fact]
    public async Task GetNewById_SlugNotFound_ThrowNotFound()
    {
        var newsList = new List<NewsEntity>
        {
            CreateNews(slug: "cong-nghe-moi")
        };

        var service = CreateService(newsList, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetNewByIdAsync("tin-cong-nghe-khac"));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
    }

    // TC07
    [Fact]
    public async Task GetNewById_DeletedNewsBySlug_ThrowNotFound()
    {
        var deletedNews = CreateNews(
            id: NewsId,
            slug: "cong-nghe-moi",
            isDeleted: true);

        var newsList = new List<NewsEntity>
        {
            deletedNews
        };

        var service = CreateService(newsList, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetNewByIdAsync("cong-nghe-moi"));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
    }

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    // Async-queryable shims moved to TestSupport/MockAsyncQueryable.cs
}