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

        var result = await service.DeleteNewAsync(NewsId.ToString());
        
        //Config response
        Assert.Equal(NewsId, result.Id);
        Assert.Equal(news.slug, result.Slug);
        Assert.True(news.IsDeleted);
        Assert.NotNull(news.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteNew_BySlug_Success()
    {
        var news = CreateNews(
            id: NewsId,
            slug: "cong-nghe-moi",
            isDeleted: false);

        var service = CreateService(
            new List<NewsEntity> { news },
            out var dbContextMock);

        var result = await service.DeleteNewAsync("cong-nghe-moi");

        Assert.Equal(NewsId, result.Id);
        Assert.Equal("cong-nghe-moi", result.Slug);
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
            () => service.DeleteNewAsync(NotFoundNewsId.ToString()));

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
            () => service.DeleteNewAsync(NewsId.ToString()));

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
            () => service.DeleteNewAsync(Guid.Empty.ToString()));

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
            () => service.DeleteNewAsync(NewsId.ToString()));

        Assert.Equal("NEW_NOT_FOUND", ex.Message);
        Assert.True(softDeletedNews.IsDeleted);
        Assert.NotNull(softDeletedNews.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    // Async-queryable shims moved to TestSupport/MockAsyncQueryable.cs
}
