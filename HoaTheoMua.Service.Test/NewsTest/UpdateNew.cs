using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using HoaTheoMua.Service.Test.TestSupport;
using static HoaTheoMua.Service.Test.TestSupport.ReflectionHelper;
using NewsEntity = HoaTheoMua.Repository.Entity.News;
using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryNewsDetailEntity = HoaTheoMua.Repository.Entity.CategoryNewsDetail;
using NewsService = HoaTheoMua.Service.New.Service;
using NewsUpdateRequest = HoaTheoMua.Service.New.UpdateNewRequest;

namespace HoaTheoMua.Service.Test.NewsTest;

public class UpdateNewTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NewsId =
        Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly Guid NotFoundNewsId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly Guid CategoryId1 =
        Guid.Parse("44444444-4444-4444-4444-444444444441");

    private static readonly Guid CategoryId2 =
        Guid.Parse("44444444-4444-4444-4444-444444444442");

    private static readonly Guid CategoryId3 =
        Guid.Parse("44444444-4444-4444-4444-444444444443");

    private static readonly Guid InvalidCategoryId =
        Guid.Parse("88888888-8888-8888-8888-888888888888");

    private static NewsService CreateService(
        List<NewsEntity> newsList,
        List<CategoryNewsEntity> categoryNewsList,
        List<CategoryNewsDetailEntity> categoryNewsDetails,
        out Mock<AppDbContext> dbContextMock)
    {
        var newsDbSetMock = BuildMockDbSet(newsList);
        var categoryDbSetMock = BuildMockDbSet(categoryNewsList);
        var categoryDetailDbSetMock = BuildCategoryNewsDetailsMockDbSet(
            categoryNewsDetails,
            newsList);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        dbContextMock = new Mock<AppDbContext>(options)
        {
            CallBase = true
        };

        dbContextMock
            .Setup(x => x.NewsList)
            .Returns(newsDbSetMock.Object);

        dbContextMock
            .Setup(x => x.CategoryNewsList)
            .Returns(categoryDbSetMock.Object);

        dbContextMock
            .Setup(x => x.CategoryNewsDetails)
            .Returns(categoryDetailDbSetMock.Object);

        dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

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

        dbSetMock
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<T>>()))
            .Callback<IEnumerable<T>>(entities =>
            {
                sourceList.AddRange(entities);
            });

        dbSetMock
            .Setup(x => x.RemoveRange(It.IsAny<IEnumerable<T>>()))
            .Callback<IEnumerable<T>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    sourceList.Remove(entity);
                }
            });

        return dbSetMock;
    }

    private static Mock<DbSet<CategoryNewsDetailEntity>> BuildCategoryNewsDetailsMockDbSet(
        List<CategoryNewsDetailEntity> sourceList,
        List<NewsEntity> newsList)
    {
        var dbSetMock = BuildMockDbSet(sourceList);

        dbSetMock
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<CategoryNewsDetailEntity>>()))
            .Callback<IEnumerable<CategoryNewsDetailEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    sourceList.Add(entity);

                    var newsId = GetGuidProperty(entity, "NewsId");
                    var news = newsList.FirstOrDefault(x => GetGuidProperty(x, "Id") == newsId);

                    if (news != null)
                    {
                        var details = GetCategoryNewsDetails(news);
                        details.Add(entity);
                    }
                }
            });

        dbSetMock
            .Setup(x => x.RemoveRange(It.IsAny<IEnumerable<CategoryNewsDetailEntity>>()))
            .Callback<IEnumerable<CategoryNewsDetailEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    sourceList.Remove(entity);

                    foreach (var news in newsList)
                    {
                        var details = GetCategoryNewsDetails(news);
                        details.Remove(entity);
                    }
                }
            });

        return dbSetMock;
    }

    private static NewsUpdateRequest CreateValidRequest(
        string title = "Công Nghệ Mới - Cập nhật",
        string description = "Nội dung đã được cập nhật...",
        string? pictureUrl = "https://example.com/image-new.png",
        NewsStatus status = NewsStatus.Published,
        List<Guid>? categoryIds = null)
    {
        return new NewsUpdateRequest
        {
            Title = title,
            Description = description,
            PictureUrl = pictureUrl,
            Status = status,
            CategoryIds = categoryIds ?? new List<Guid>
            {
                CategoryId1,
                CategoryId2
            }
        };
    }

    private static CategoryNewsEntity CreateCategory(
        Guid? id = null,
        string title = "Technology",
        bool isDeleted = false)
    {
        var category = Activator.CreateInstance<CategoryNewsEntity>()!;

        SetPropertyIfExists(category, "Id", id ?? CategoryId1);
        SetPropertyIfExists(category, "Title", title);
        SetPropertyIfExists(category, "Slug", title.ToLower());
        SetPropertyIfExists(category, "IsDeleted", isDeleted);
        SetPropertyIfExists(category, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-10));
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
        SetPropertyIfExists(detail, "CategoryNewsId", categoryId ?? CategoryId1);
        SetPropertyIfExists(detail, "CategoryNews", category);
        SetPropertyIfExists(detail, "IsDeleted", isDeleted);
        SetPropertyIfExists(detail, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-5));
        SetPropertyIfExists(detail, "UpdatedAt", null);

        return detail;
    }

    private static NewsEntity CreateNews(
        Guid? id = null,
        string title = "Công Nghệ Mới",
        string description = "Bài viết về công nghệ mới...",
        string coverImage = "https://example.com/image.png",
        string slug = "cong-nghe-moi",
        NewsStatus status = NewsStatus.Draft,
        bool isDeleted = false,
        List<CategoryNewsDetailEntity>? categoryNewsDetails = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        var news = Activator.CreateInstance<NewsEntity>()!;

        SetPropertyIfExists(news, "Id", id ?? NewsId);
        SetPropertyIfExists(news, "Title", title);
        SetPropertyIfExists(news, "Description", description);
        SetPropertyIfExists(news, "CoverImage", coverImage);
        SetPropertyIfExists(news, "slug", slug);
        SetPropertyIfExists(news, "Status", status);
        SetPropertyIfExists(news, "UserId", UserId);
        SetPropertyIfExists(news, "AuthorName", "Admin User");
        SetPropertyIfExists(news, "AuthorEmail", "admin@example.com");
        SetPropertyIfExists(news, "AuthorAvatar", "https://example.com/avatar.png");
        SetPropertyIfExists(news, "ViewCount", 150);
        SetPropertyIfExists(news, "IsDeleted", isDeleted);
        SetPropertyIfExists(news, "CreatedAt", createdAt ?? DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"));
        SetPropertyIfExists(news, "UpdatedAt", updatedAt);

        SetPropertyIfExists(
            news,
            "CategoryNewsDetail",
            categoryNewsDetails ?? new List<CategoryNewsDetailEntity>());

        return news;
    }

    // TC01
    [Fact]
    public async Task UpdateNew_BySlug_Success()
    {
        var category1 = CreateCategory(CategoryId1, "Technology");
        var category2 = CreateCategory(CategoryId2, "Business");

        var detail1 = CreateCategoryNewsDetail(
            newsId: NewsId,
            categoryId: CategoryId1,
            category: category1);

        var news = CreateNews(
            id: NewsId,
            status: NewsStatus.Draft,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1,
            category2
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest();

        var result = await service.UpdateNewAsync("cong-nghe-moi", request);

        Assert.NotNull(result);
        Assert.Equal(NewsId, result!.Id);
        Assert.Equal("Công Nghệ Mới - Cập nhật", result.Title);
        Assert.Equal("Nội dung đã được cập nhật...", result.Description);
        Assert.Equal("https://example.com/image-new.png", result.PictureUrl);
        Assert.Equal("Published", result.Status);
        Assert.Equal(UserId, result.UserId);
        Assert.NotNull(result.UpdatedAt);

        Assert.Equal("Công Nghệ Mới - Cập nhật", news.Title);
        Assert.Equal("Nội dung đã được cập nhật...", news.Description);
        Assert.Equal("https://example.com/image-new.png", news.CoverImage);
        Assert.Equal(NewsStatus.Published, news.Status);
        Assert.NotNull(news.UpdatedAt);

        Assert.Equal(2, categoryNewsDetails.Count);
        Assert.Contains(categoryNewsDetails, x => x.CategoryNewsId == CategoryId1);
        Assert.Contains(categoryNewsDetails, x => x.CategoryNewsId == CategoryId2);

        Assert.Equal(2, result.CategoryIds.Count);
        Assert.Contains(CategoryId1, result.CategoryIds);
        Assert.Contains(CategoryId2, result.CategoryIds);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task UpdateNew_CategoryIdsEmpty_ThrowBadRequest()
    {
        var category1 = CreateCategory(CategoryId1);
        var detail1 = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: category1);

        var news = CreateNews(
            id: NewsId,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>());

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateNewAsync(NewsId.ToString(), request));

        Assert.Equal("CATEGORY_IDS_REQUIRED", ex.Message);
        Assert.Single(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task UpdateNew_CategoryNotFound_ThrowBadRequest()
    {
        var category1 = CreateCategory(CategoryId1);
        var detail1 = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: category1);

        var news = CreateNews(
            id: NewsId,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>
            {
                InvalidCategoryId
            });

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateNewAsync(NewsId.ToString(), request));

        Assert.Equal("CATEGORY_NEWS_INVALID", ex.Message);
        Assert.Single(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task UpdateNew_NotFound_ThrowNotFound()
    {
        var category1 = CreateCategory(CategoryId1);

        var newsList = new List<NewsEntity>();

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>
            {
                CategoryId1
            });

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateNewAsync(NotFoundNewsId.ToString(), request));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC06
    [Fact]
    public async Task UpdateNew_TitleChanged_CurrentServiceKeepsOldSlug()
    {
        var category1 = CreateCategory(CategoryId1);
        var detail1 = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: category1);

        var news = CreateNews(
            id: NewsId,
            title: "Title Cũ",
            slug: "title-cu",
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out _);

        var request = CreateValidRequest(
            title: "Title Mới",
            categoryIds: new List<Guid>
            {
                CategoryId1
            });

        var result = await service.UpdateNewAsync(NewsId.ToString(), request);

        Assert.NotNull(result);
        Assert.Equal("Title Mới", result!.Title);
        Assert.Equal("title-cu", result.slug);
        Assert.Equal("title-cu", news.slug);
    }

    // TC07
    [Fact]
    public async Task UpdateNew_CategoryIdsChanged_RemoveOldAndAddNew()
    {
        var categoryA = CreateCategory(CategoryId1, "A");
        var categoryB = CreateCategory(CategoryId2, "B");
        var categoryC = CreateCategory(CategoryId3, "C");

        var detailA = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: categoryA);
        var detailB = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId2, category: categoryB);

        var news = CreateNews(
            id: NewsId,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detailA,
                detailB
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            categoryA,
            categoryB,
            categoryC
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detailA,
            detailB
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out _);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>
            {
                CategoryId2,
                CategoryId3
            });

        var result = await service.UpdateNewAsync(NewsId.ToString(), request);

        Assert.NotNull(result);

        Assert.DoesNotContain(categoryNewsDetails, x => x.CategoryNewsId == CategoryId1);
        Assert.Contains(categoryNewsDetails, x => x.CategoryNewsId == CategoryId2);
        Assert.Contains(categoryNewsDetails, x => x.CategoryNewsId == CategoryId3);

        Assert.Equal(2, result!.CategoryIds.Count);
        Assert.DoesNotContain(CategoryId1, result.CategoryIds);
        Assert.Contains(CategoryId2, result.CategoryIds);
        Assert.Contains(CategoryId3, result.CategoryIds);
    }

    // TC08
    [Fact]
    public async Task UpdateNew_CategoryIdsSame_NotAddOrRemoveCategoryDetails()
    {
        var categoryA = CreateCategory(CategoryId1, "A");
        var categoryB = CreateCategory(CategoryId2, "B");

        var detailA = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: categoryA);
        var detailB = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId2, category: categoryB);

        var news = CreateNews(
            id: NewsId,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detailA,
                detailB
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            categoryA,
            categoryB
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detailA,
            detailB
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out _);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>
            {
                CategoryId1,
                CategoryId2
            });

        var result = await service.UpdateNewAsync(NewsId.ToString(), request);

        Assert.NotNull(result);
        Assert.Equal(2, categoryNewsDetails.Count);
        Assert.Contains(detailA, categoryNewsDetails);
        Assert.Contains(detailB, categoryNewsDetails);

        Assert.Equal(2, result!.CategoryIds.Count);
        Assert.Contains(CategoryId1, result.CategoryIds);
        Assert.Contains(CategoryId2, result.CategoryIds);
    }

    // TC09
    [Fact]
    public async Task UpdateNew_StatusDraftToPublished_Success()
    {
        var category1 = CreateCategory(CategoryId1);
        var detail1 = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: category1);

        var news = CreateNews(
            id: NewsId,
            title: "Công Nghệ Mới",
            description: "Bài viết về công nghệ mới...",
            coverImage: "https://example.com/image.png",
            status: NewsStatus.Draft,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out _);

        var request = CreateValidRequest(
            title: "Công Nghệ Mới",
            description: "Bài viết về công nghệ mới...",
            pictureUrl: "https://example.com/image.png",
            status: NewsStatus.Published,
            categoryIds: new List<Guid>
            {
                CategoryId1
            });

        var result = await service.UpdateNewAsync(NewsId.ToString(), request);

        Assert.NotNull(result);
        Assert.Equal("Published", result!.Status);
        Assert.Equal(NewsStatus.Published, news.Status);
        Assert.Equal("Công Nghệ Mới", news.Title);
        Assert.Equal("Bài viết về công nghệ mới...", news.Description);
        Assert.Equal("https://example.com/image.png", news.CoverImage);
    }

    // TC10
    [Fact]
    public async Task UpdateNew_SoftDeletedNews_ThrowNotFound()
    {
        var category1 = CreateCategory(CategoryId1);
        var detail1 = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: category1);

        var deletedNews = CreateNews(
            id: NewsId,
            isDeleted: true,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            deletedNews
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>
            {
                CategoryId1
            });

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateNewAsync(NewsId.ToString(), request));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
        Assert.True(deletedNews.IsDeleted);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC11
    [Fact]
    public async Task UpdateNew_CategoryIdsContainGuidEmpty_IgnoreGuidEmptyAndUpdateSuccess()
    {
        var category1 = CreateCategory(CategoryId1);
        var detail1 = CreateCategoryNewsDetail(newsId: NewsId, categoryId: CategoryId1, category: category1);

        var news = CreateNews(
            id: NewsId,
            categoryNewsDetails: new List<CategoryNewsDetailEntity>
            {
                detail1
            });

        var newsList = new List<NewsEntity>
        {
            news
        };

        var categoryNewsList = new List<CategoryNewsEntity>
        {
            category1
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            detail1
        };

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out _);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>
            {
                Guid.Empty,
                CategoryId1
            });

        var result = await service.UpdateNewAsync(NewsId.ToString(), request);

        Assert.NotNull(result);
        Assert.Single(result!.CategoryIds);
        Assert.Contains(CategoryId1, result.CategoryIds);
        Assert.DoesNotContain(Guid.Empty, result.CategoryIds);
    }

    private static ICollection<CategoryNewsDetailEntity> GetCategoryNewsDetails(NewsEntity news)
    {
        var value = GetProperty(news, "CategoryNewsDetail");

        if (value is ICollection<CategoryNewsDetailEntity> collection)
        {
            return collection;
        }

        throw new Exception("Không đọc được CategoryNewsDetail từ News entity");
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

    private static Guid GetGuidProperty(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is Guid guid)
        {
            return guid;
        }

        return Guid.Parse(value!.ToString()!);
    }

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    // Async-queryable shims (with EF operator cleaning) moved to TestSupport/MockAsyncQueryable.cs
}
