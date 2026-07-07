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
using NewsEntity = HoaTheoMua.Repository.Entity.News;
using NewsService = HoaTheoMua.Service.New.Service;

namespace HoaTheoMua.Service.Test.NewsTest;

public class DeleteNewTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NewsId =
        Guid.Parse("66666666-6666-6666-6666-666666666666");

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
            .Setup(x => x.Update(It.IsAny<T>()))
            .Returns<T>(_ => null!);

        return dbSetMock;
    }

    private static NewsEntity CreateNews(
        Guid? id = null,
        string title = "Công Nghệ Mới",
        string description = "Bài viết về công nghệ mới...",
        string coverImage = "https://example.com/image.png",
        string slug = "cong-nghe-moi",
        NewsStatus status = NewsStatus.Published,
        bool isDeleted = false,
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
        SetPropertyIfExists(news, "ViewCount", 0);
        SetPropertyIfExists(news, "IsDeleted", isDeleted);
        SetPropertyIfExists(news, "CreatedAt", createdAt ?? DateTimeOffset.UtcNow.AddDays(-1));
        SetPropertyIfExists(news, "UpdatedAt", updatedAt);

        return news;
    }

    // TC01
    [Fact]
    public async Task DeleteNew_Success()
    {
        var news = CreateNews(
            id: NewsId,
            isDeleted: false);

        var newsList = new List<NewsEntity>
        {
            news
        };

        var service = CreateService(newsList, out var dbContextMock);

        var result = await service.DeleteNewAsync(NewsId);

        Assert.True(result);
        Assert.True(news.IsDeleted);
        Assert.NotNull(news.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task DeleteNew_NotFound_ThrowNotFound()
    {
        var newsList = new List<NewsEntity>();

        var service = CreateService(newsList, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteNewAsync(NotFoundNewsId));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task DeleteNew_AlreadySoftDeleted_ThrowNotFound()
    {
        var deletedNews = CreateNews(
            id: NewsId,
            isDeleted: true);

        var newsList = new List<NewsEntity>
        {
            deletedNews
        };

        var service = CreateService(newsList, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteNewAsync(NewsId));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
        Assert.True(deletedNews.IsDeleted);
        Assert.Null(deletedNews.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task DeleteNew_EmptyGuid_CurrentServiceThrowNotFound()
    {
        var news = CreateNews(
            id: NewsId,
            isDeleted: false);

        var newsList = new List<NewsEntity>
        {
            news
        };

        var service = CreateService(newsList, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteNewAsync(Guid.Empty));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
        Assert.False(news.IsDeleted);
        Assert.Null(news.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC05
    [Fact]
    public async Task DeleteNew_SoftDeletedNewsBefore_ThrowNotFound()
    {
        var softDeletedNews = CreateNews(
            id: NewsId,
            title: "News đã bị xóa",
            isDeleted: true,
            updatedAt: DateTimeOffset.UtcNow.AddDays(-1));

        var newsList = new List<NewsEntity>
        {
            softDeletedNews
        };

        var service = CreateService(newsList, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteNewAsync(NewsId));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
        Assert.True(softDeletedNews.IsDeleted);
        Assert.NotNull(softDeletedNews.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
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