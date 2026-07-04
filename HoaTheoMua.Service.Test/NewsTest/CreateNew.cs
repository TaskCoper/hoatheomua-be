using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using NewsEntity = HoaTheoMua.Repository.Entity.News;
using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryNewsDetailEntity = HoaTheoMua.Repository.Entity.CategoryNewsDetail;
using NewsService = HoaTheoMua.Service.New.Service;
using NewsRequest = HoaTheoMua.Service.New.CreateNewRequest;

namespace HoaTheoMua.Service.Test.NewTest;

public class CreateNewTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NewsId =
        Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly Guid CategoryId1 =
        Guid.Parse("44444444-4444-4444-4444-444444444441");

    private static readonly Guid CategoryId2 =
        Guid.Parse("44444444-4444-4444-4444-444444444442");

    private static readonly Guid InvalidCategoryId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static NewsService CreateService(
        List<NewsEntity> newsList,
        List<CategoryNewsEntity> categoryNewsList,
        List<CategoryNewsDetailEntity> categoryNewsDetails,
        out Mock<AppDbContext> dbContextMock,
        bool throwUnauthorized = false)
    {
        var newsDbSetMock = BuildMockDbSet(newsList);
        var categoryDbSetMock = BuildMockDbSet(categoryNewsList);
        var categoryDetailDbSetMock = BuildMockDbSet(categoryNewsDetails);

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

        if (throwUnauthorized)
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Throws(new UnauthorizedAccessException("Unauthorized"));
        }
        else
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Returns(UserId);
        }

        currentUserMock
            .SetupGet(x => x.FullName)
            .Returns("Admin User");

        currentUserMock
            .SetupGet(x => x.Email)
            .Returns("admin@example.com");

        currentUserMock
            .SetupGet(x => x.Avatar)
            .Returns("https://example.com/avatar.png");

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
            .Setup(x => x.Add(It.IsAny<T>()))
            .Callback<T>(entity =>
            {
                sourceList.Add(entity);
            })
            .Returns<T>(_ => null!);

        dbSetMock
            .Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) =>
            {
                sourceList.Add(entity);
            })
            .Returns<T, CancellationToken>((_, _) =>
                new ValueTask<EntityEntry<T>>((EntityEntry<T>)null!));

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

        dbSetMock
            .Setup(x => x.Update(It.IsAny<T>()))
            .Returns<T>(_ => null!);

        return dbSetMock;
    }

    private static NewsRequest CreateValidRequest(
        string title = "Công Nghệ Mới",
        string description = "Bài viết về công nghệ mới...",
        string? pictureUrl = "https://example.com/image.png",
        NewsStatus status = NewsStatus.Published,
        List<Guid>? categoryIds = null)
    {
        return new NewsRequest
        {
            Title = title,
            Description = description,
            PictureUrl = pictureUrl,
            Status = status,
            CategoryIds = categoryIds ?? new List<Guid> { CategoryId1 }
        };
    }

    private static CategoryNewsEntity CreateCategory(
        Guid? id = null,
        string title = "Công nghệ",
        bool isDeleted = false)
    {
        var category = Activator.CreateInstance<CategoryNewsEntity>()!;

        SetPropertyIfExists(category, "Id", id ?? CategoryId1);
        SetPropertyIfExists(category, "Title", title);
        SetPropertyIfExists(category, "Slug", "cong-nghe");
        SetPropertyIfExists(category, "IsDeleted", isDeleted);
        SetPropertyIfExists(category, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-1));
        SetPropertyIfExists(category, "UpdatedAt", null);

        return category;
    }

    private static NewsEntity CreateNews(
        Guid? id = null,
        string title = "Công Nghệ Mới",
        string description = "Old description",
        string slug = "cong-nghe-moi",
        NewsStatus status = NewsStatus.Published,
        bool isDeleted = false)
    {
        var news = Activator.CreateInstance<NewsEntity>()!;

        SetPropertyIfExists(news, "Id", id ?? NewsId);
        SetPropertyIfExists(news, "Title", title);
        SetPropertyIfExists(news, "Description", description);
        SetPropertyIfExists(news, "CoverImage", "https://example.com/old-image.png");
        SetPropertyIfExists(news, "Status", status);
        SetPropertyIfExists(news, "slug", slug);
        SetPropertyIfExists(news, "UserId", UserId);
        SetPropertyIfExists(news, "AuthorName", "Admin User");
        SetPropertyIfExists(news, "AuthorEmail", "admin@example.com");
        SetPropertyIfExists(news, "AuthorAvatar", "https://example.com/avatar.png");
        SetPropertyIfExists(news, "ViewCount", 0);
        SetPropertyIfExists(news, "IsDeleted", isDeleted);
        SetPropertyIfExists(news, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-2));
        SetPropertyIfExists(news, "UpdatedAt", null);

        return news;
    }

    // TC01
    [Fact]
    public async Task CreateNew_Success()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
        };
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest();

        var result = await service.CreateNewAsync(request);

        var createdNews = Assert.Single(newsList);
        var createdCategoryDetail = Assert.Single(categoryNewsDetails);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Công Nghệ Mới", result.Title);
        Assert.Equal("Bài viết về công nghệ mới...", result.Description);
        Assert.Equal(0, result.ViewCount);
        Assert.Equal("https://example.com/image.png", result.PictureUrl);
        Assert.Equal("cong-nghe-moi", result.slug);
        Assert.Equal("Published", result.Status);
        Assert.Equal(UserId, result.UserId);
        Assert.Single(result.CategoryIds);
        Assert.Contains(CategoryId1, result.CategoryIds);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.Null(result.UpdatedAt);

        Assert.Equal("Công Nghệ Mới", createdNews.Title);
        Assert.Equal("Bài viết về công nghệ mới...", createdNews.Description);
        Assert.Equal("https://example.com/image.png", createdNews.CoverImage);
        Assert.Equal("cong-nghe-moi", createdNews.slug);
        Assert.Equal(NewsStatus.Published, createdNews.Status);
        Assert.Equal(UserId, createdNews.UserId);
        Assert.Equal("Admin User", createdNews.AuthorName);
        Assert.Equal("admin@example.com", createdNews.AuthorEmail);
        Assert.Equal("https://example.com/avatar.png", createdNews.AuthorAvatar);
        Assert.Equal(0, createdNews.ViewCount);
        Assert.False(createdNews.IsDeleted);

        Assert.Equal(CategoryId1, createdCategoryDetail.CategoryNewsId);
        Assert.Equal(createdNews.Id, createdCategoryDetail.NewsId);
        Assert.False(createdCategoryDetail.IsDeleted);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task CreateNew_CategoryIdsEmpty_ThrowBadRequest()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
        };
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            categoryIds: new List<Guid>());

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateNewAsync(request));

        Assert.Equal("CATEGORY_IDS_REQUIRED", ex.Message);
        Assert.Empty(newsList);
        Assert.Empty(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task CreateNew_CategoryNotFound_ThrowBadRequest()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>();
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            categoryIds: new List<Guid> { InvalidCategoryId });

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateNewAsync(request));

        Assert.Equal("CATEGORY_NEWS_INVALID", ex.Message);
        Assert.Empty(newsList);
        Assert.Empty(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task CreateNew_NoUserIdInToken_ThrowUnauthorized()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
        };
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock,
            throwUnauthorized: true);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateNewAsync(request));

        Assert.Equal("Unauthorized", ex.Message);
        Assert.Empty(newsList);
        Assert.Empty(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC05
    [Fact]
    public async Task CreateNew_DuplicateSlug_AddSuffix()
    {
        var existingNews = CreateNews(
            title: "Công Nghệ Mới",
            slug: "cong-nghe-moi");

        var newsList = new List<NewsEntity>
        {
            existingNews
        };
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
        };
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            title: "Công Nghệ Mới");

        var result = await service.CreateNewAsync(request);

        var createdNews = newsList.Single(x => x.Id != existingNews.Id);

        Assert.Equal("cong-nghe-moi-2", result.slug);
        Assert.Equal("cong-nghe-moi-2", createdNews.slug);
        Assert.Equal(2, newsList.Count);
        Assert.Single(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC06
    [Fact]
    public async Task CreateNew_TitleEmpty_CurrentServiceStillCreatesWithGeneratedSlug()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
        };
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            title: "");

        var result = await service.CreateNewAsync(request);

        var createdNews = Assert.Single(newsList);

        Assert.Equal("", result.Title);
        Assert.False(string.IsNullOrWhiteSpace(result.slug));
        Assert.Equal(result.slug, createdNews.slug);
        Assert.Single(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC07
    [Fact]
    public async Task CreateNew_CategoryIdsContainGuidEmpty_IgnoreGuidEmptyAndCreateSuccess()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
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
                CategoryId1,
                Guid.Empty
            });

        var result = await service.CreateNewAsync(request);

        Assert.Single(result.CategoryIds);
        Assert.Contains(CategoryId1, result.CategoryIds);
        Assert.DoesNotContain(Guid.Empty, result.CategoryIds);
        Assert.Single(categoryNewsDetails);
        Assert.Equal(CategoryId1, categoryNewsDetails[0].CategoryNewsId);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC08
    [Fact]
    public async Task CreateNew_CategoryIdsDuplicated_DeduplicateAndCreateSuccess()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1, title: "Công nghệ"),
            CreateCategory(id: CategoryId2, title: "Tin tức")
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
                CategoryId1,
                CategoryId1,
                CategoryId2
            });

        var result = await service.CreateNewAsync(request);

        Assert.Equal(2, result.CategoryIds.Count);
        Assert.Contains(CategoryId1, result.CategoryIds);
        Assert.Contains(CategoryId2, result.CategoryIds);

        Assert.Equal(2, categoryNewsDetails.Count);
        Assert.Contains(categoryNewsDetails, x => x.CategoryNewsId == CategoryId1);
        Assert.Contains(categoryNewsDetails, x => x.CategoryNewsId == CategoryId2);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC09
    [Fact]
    public async Task CreateNew_StatusDraft_Success()
    {
        var newsList = new List<NewsEntity>();
        var categoryNewsList = new List<CategoryNewsEntity>
        {
            CreateCategory(id: CategoryId1)
        };
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var service = CreateService(
            newsList,
            categoryNewsList,
            categoryNewsDetails,
            out var dbContextMock);

        var request = CreateValidRequest(
            status: NewsStatus.Draft);

        var result = await service.CreateNewAsync(request);

        var createdNews = Assert.Single(newsList);

        Assert.Equal("Draft", result.Status);
        Assert.Equal(NewsStatus.Draft, createdNews.Status);
        Assert.Equal(0, result.ViewCount);
        Assert.Single(categoryNewsDetails);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static void SetPropertyIfExists(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null || !property.CanWrite)
        {
            return;
        }

        if (value == null)
        {
            property.SetValue(obj, null);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (targetType.IsEnum)
        {
            var enumValue = Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
            property.SetValue(obj, enumValue);
            return;
        }

        if (targetType == typeof(Guid))
        {
            property.SetValue(obj, Guid.Parse(value.ToString()!));
            return;
        }

        if (targetType == typeof(DateTimeOffset))
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                property.SetValue(obj, dateTimeOffset);
            }
            else
            {
                property.SetValue(obj, DateTimeOffset.Parse(value.ToString()!));
            }

            return;
        }

        if (targetType.IsAssignableFrom(value.GetType()))
        {
            property.SetValue(obj, value);
            return;
        }

        property.SetValue(obj, Convert.ChangeType(value, targetType));
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];

            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method =>
                    method.Name == nameof(IQueryProvider.Execute) &&
                    method.IsGenericMethod &&
                    method.GetParameters().Length == 1)
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new object[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private class TestAsyncEnumerable<T> :
        EnumerableQuery<T>,
        IAsyncEnumerable<T>,
        IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider =>
            new TestAsyncQueryProvider<T>(this);
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}