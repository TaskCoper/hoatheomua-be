using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
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

public class GetNewsTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NewsId1 =
        Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly Guid NewsId2 =
        Guid.Parse("77777777-7777-7777-7777-777777777777");

    private static readonly Guid NewsId3 =
        Guid.Parse("88888888-8888-8888-8888-888888888888");

    private static readonly Guid CategoryId =
        Guid.Parse("44444444-4444-4444-4444-444444444441");

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
        SetPropertyIfExists(category, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-5));
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
        SetPropertyIfExists(detail, "NewsId", newsId ?? NewsId1);
        SetPropertyIfExists(detail, "CategoryNewsId", categoryId ?? CategoryId);
        SetPropertyIfExists(detail, "CategoryNews", category ?? CreateCategory());
        SetPropertyIfExists(detail, "IsDeleted", isDeleted);
        SetPropertyIfExists(detail, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-4));
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
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        List<CategoryNewsDetailEntity>? categoryNewsDetails = null)
    {
        var newsId = id ?? NewsId1;

        var news = Activator.CreateInstance<NewsEntity>()!;

        SetPropertyIfExists(news, "Id", newsId);
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
                CreateCategoryNewsDetail(newsId: newsId)
            });

        return news;
    }

    // TC01
    [Fact]
    public async Task GetNews_Success()
    {
        var news = CreateNews(
            id: NewsId1,
            title: "Công Nghệ Mới",
            description: "Bài viết về công nghệ mới...",
            status: NewsStatus.Published,
            isDeleted: false);

        var deletedNews = CreateNews(
            id: NewsId2,
            title: "Deleted News",
            isDeleted: true);

        var newsList = new List<NewsEntity>
        {
            news,
            deletedNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: null,
            status: null);

        var items = GetItems(result);

        Assert.Single(items);

        var item = items[0];

        Assert.Equal(NewsId1, GetGuid(item, "Id"));
        Assert.Equal("Công Nghệ Mới", GetString(item, "Title"));
        Assert.Equal("Bài viết về công nghệ mới...", GetString(item, "Description"));
        Assert.Equal(150, GetInt(item, "ViewCount"));
        Assert.Equal("https://example.com/image.png", GetString(item, "PictureUrl"));
        Assert.Equal("cong-nghe-moi", GetString(item, "slug"));
        Assert.Equal("Published", GetString(item, "Status"));

        var categories = GetList(GetProperty(item, "Categories"));
        Assert.Single(categories);
        Assert.Equal(CategoryId, GetGuid(categories[0], "CategoryId"));
        Assert.Equal("Technology", GetString(categories[0], "CategoryName"));

        var user = GetProperty(item, "User");
        Assert.NotNull(user);
        Assert.Equal(UserId, GetGuid(user!, "UserId"));
        Assert.Equal("admin@example.com", GetString(user!, "Email"));
        Assert.Equal("https://example.com/avatar.png", GetString(user!, "UserAvatar"));
        Assert.Equal("Admin User", GetString(user!, "UserFullName"));

        Assert.Equal(1, GetPaginationInt(result, "PageIndex"));
        Assert.Equal(10, GetPaginationInt(result, "PageSize"));
        Assert.Equal(1, GetPaginationInt(result, "TotalCount"));
    }

    // TC02
    [Fact]
    public async Task GetNews_FilterByKeyword()
    {
        var matchedNews = CreateNews(
            id: NewsId1,
            title: "Technology AI News",
            description: "Bài viết về AI",
            slug: "technology-ai-news");

        var notMatchedNews = CreateNews(
            id: NewsId2,
            title: "Flower News",
            description: "Bài viết về hoa",
            slug: "flower-news");

        var newsList = new List<NewsEntity>
        {
            matchedNews,
            notMatchedNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: "technology",
            status: null);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(NewsId1, GetGuid(items[0], "Id"));
        Assert.Equal("Technology AI News", GetString(items[0], "Title"));
        Assert.Equal(1, GetPaginationInt(result, "TotalCount"));
    }

    // TC03
    [Fact]
    public async Task GetNews_FilterByStatusPublished()
    {
        var publishedNews = CreateNews(
            id: NewsId1,
            title: "Published News",
            slug: "published-news",
            status: NewsStatus.Published);

        var draftNews = CreateNews(
            id: NewsId2,
            title: "Draft News",
            slug: "draft-news",
            status: NewsStatus.Draft);

        var newsList = new List<NewsEntity>
        {
            publishedNews,
            draftNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: null,
            status: NewsStatus.Published);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(NewsId1, GetGuid(items[0], "Id"));
        Assert.Equal("Published", GetString(items[0], "Status"));
        Assert.Equal(1, GetPaginationInt(result, "TotalCount"));
    }

    // TC04
    [Fact]
    public async Task GetNews_InvalidPageIndexAndPageSize_NormalizeToDefault()
    {
        var news = CreateNews(
            id: NewsId1,
            title: "Công Nghệ Mới",
            slug: "cong-nghe-moi");

        var newsList = new List<NewsEntity>
        {
            news
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 0,
            pageSize: 0,
            keyword: null,
            status: null);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(1, GetPaginationInt(result, "PageIndex"));
        Assert.Equal(10, GetPaginationInt(result, "PageSize"));
        Assert.Equal(1, GetPaginationInt(result, "TotalCount"));
    }

    // TC05
    [Fact]
    public async Task GetNews_NoData_ReturnEmptyList()
    {
        var deletedNews = CreateNews(
            id: NewsId1,
            title: "Deleted News",
            slug: "deleted-news",
            isDeleted: true);

        var newsList = new List<NewsEntity>
        {
            deletedNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: null,
            status: null);

        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetPaginationInt(result, "TotalCount"));
    }

    // TC06
    [Fact]
    public async Task GetNews_FilterByStatusDraft()
    {
        var publishedNews = CreateNews(
            id: NewsId1,
            title: "Published News",
            slug: "published-news",
            status: NewsStatus.Published);

        var draftNews = CreateNews(
            id: NewsId2,
            title: "Draft News",
            slug: "draft-news",
            status: NewsStatus.Draft);

        var newsList = new List<NewsEntity>
        {
            publishedNews,
            draftNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: null,
            status: NewsStatus.Draft);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(NewsId2, GetGuid(items[0], "Id"));
        Assert.Equal("Draft", GetString(items[0], "Status"));
        Assert.Equal(1, GetPaginationInt(result, "TotalCount"));
    }

    // TC07
    [Fact]
    public async Task GetNews_FilterNoResult_ReturnEmptyList()
    {
        var news = CreateNews(
            id: NewsId1,
            title: "Technology News",
            description: "Bài viết về công nghệ",
            slug: "technology-news",
            status: NewsStatus.Published);

        var newsList = new List<NewsEntity>
        {
            news
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: "flower",
            status: NewsStatus.Draft);

        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetPaginationInt(result, "TotalCount"));
    }

    // TC08
    [Fact]
    public async Task GetNews_KeywordInDescription_ReturnMatchedNews()
    {
        var matchedNews = CreateNews(
            id: NewsId1,
            title: "Tin tức hôm nay",
            description: "Đây là nội dung về technology mới nhất",
            slug: "tin-tuc-hom-nay");

        var notMatchedNews = CreateNews(
            id: NewsId2,
            title: "Tin khác",
            description: "Không liên quan",
            slug: "tin-khac");

        var newsList = new List<NewsEntity>
        {
            matchedNews,
            notMatchedNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: "technology",
            status: null);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(NewsId1, GetGuid(items[0], "Id"));
        Assert.Equal("Tin tức hôm nay", GetString(items[0], "Title"));
        Assert.Contains("technology", GetString(items[0], "Description")!);
    }

    // TC09
    [Fact]
    public async Task GetNews_OrderByCreatedAtDescending()
    {
        var oldestNews = CreateNews(
            id: NewsId1,
            title: "Oldest News",
            slug: "oldest-news",
            createdAt: DateTimeOffset.Parse("2026-06-20T08:00:00Z"));

        var middleNews = CreateNews(
            id: NewsId2,
            title: "Middle News",
            slug: "middle-news",
            createdAt: DateTimeOffset.Parse("2026-06-22T08:00:00Z"));

        var newestNews = CreateNews(
            id: NewsId3,
            title: "Newest News",
            slug: "newest-news",
            createdAt: DateTimeOffset.Parse("2026-06-23T08:00:00Z"));

        var newsList = new List<NewsEntity>
        {
            oldestNews,
            newestNews,
            middleNews
        };

        var service = CreateService(newsList, out _);

        var result = await service.GetNewsAsync(
            pageIndex: 1,
            pageSize: 10,
            keyword: null,
            status: null);

        var items = GetItems(result);

        Assert.Equal(3, items.Count);

        Assert.Equal(NewsId3, GetGuid(items[0], "Id"));
        Assert.Equal("Newest News", GetString(items[0], "Title"));

        Assert.Equal(NewsId2, GetGuid(items[1], "Id"));
        Assert.Equal("Middle News", GetString(items[1], "Title"));

        Assert.Equal(NewsId1, GetGuid(items[2], "Id"));
        Assert.Equal("Oldest News", GetString(items[2], "Title"));
    }

    private static object? GetPaginationValue(object result)
    {
        return GetProperty(result, "Value") ?? result;
    }

    private static List<object> GetItems(object result)
    {
        var paginationValue = GetPaginationValue(result);

        var items = GetProperty(paginationValue, "Items");

        if (items is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().ToList();
        }

        throw new Exception("Không đọc được Items từ BasePaginationResponse");
    }

    private static int GetPaginationInt(object result, string propertyName)
    {
        var paginationValue = GetPaginationValue(result);
        return GetInt(paginationValue, propertyName);
    }

    private static List<object> GetList(object? value)
    {
        if (value is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().ToList();
        }

        return new List<object>();
    }

    private static object? GetProperty(object? obj, string propertyName)
    {
        if (obj == null)
        {
            return null;
        }

        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        return property?.GetValue(obj);
    }

    private static string? GetString(object obj, string propertyName)
    {
        return GetProperty(obj, propertyName)?.ToString();
    }

    private static Guid GetGuid(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is Guid guid)
        {
            return guid;
        }

        return Guid.Parse(value!.ToString()!);
    }

    private static int GetInt(object obj, string propertyName)
    {
        // Fail loudly if the property is missing/renamed instead of masking it as 0
        // (Convert.ToInt32(null) == 0), which would let a broken pagination assertion pass.
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null)
        {
            throw new Xunit.Sdk.XunitException(
                $"Property '{propertyName}' not found on {obj.GetType().Name}");
        }

        var value = property.GetValue(obj);

        if (value is int intValue)
        {
            return intValue;
        }

        return Convert.ToInt32(value);
    }

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    // Async-queryable shims (with EF operator cleaning) moved to TestSupport/MockAsyncQueryable.cs
}