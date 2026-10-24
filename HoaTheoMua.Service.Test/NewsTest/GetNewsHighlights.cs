using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using NewsService = HoaTheoMua.Service.New.Service;

namespace HoaTheoMua.Service.Test.NewsTest;

public class GetNewsHighlightsTest
{
    [Fact]
    public async Task GetNewsHighlights_CurrentWeek_PicksHighestViewAsWeeklyFocus()
    {
        await using var db = CreateDbContext();
        var startOfWeek = GetStartOfCurrentWeek();

        await db.NewsList.AddRangeAsync(
            News("A", 100, startOfWeek.AddDays(1)),
            News("B", 500, startOfWeek.AddDays(2)),
            News("C", 300, startOfWeek.AddDays(3)));

        await db.SaveChangesAsync();

        var result = await CreateService(db).GetNewsHighlightsAsync();

        Assert.NotNull(result.WeeklyFocus);
        Assert.Equal("B", result.WeeklyFocus!.Title);
        Assert.Equal(["C", "A"], result.FeaturedNews.Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task GetNewsHighlights_SameViewCount_PicksNewestAsWeeklyFocus()
    {
        await using var db = CreateDbContext();
        var startOfWeek = GetStartOfCurrentWeek();

        await db.NewsList.AddRangeAsync(
            News("Older", 500, startOfWeek.AddDays(1)),
            News("Newer", 500, startOfWeek.AddDays(4)));

        await db.SaveChangesAsync();

        var result = await CreateService(db).GetNewsHighlightsAsync();

        Assert.Equal("Newer", result.WeeklyFocus!.Title);
        Assert.Single(result.FeaturedNews);
        Assert.Equal("Older", result.FeaturedNews[0].Title);
    }

    [Fact]
    public async Task GetNewsHighlights_NotEnoughCurrentWeekNews_FillsFeaturedWithOlderPublishedNews()
    {
        await using var db = CreateDbContext();
        var startOfWeek = GetStartOfCurrentWeek();

        await db.NewsList.AddRangeAsync(
            News("Week Focus", 500, startOfWeek.AddDays(1)),
            News("Week Featured", 200, startOfWeek.AddDays(2)),
            News("Old 1", 1000, startOfWeek.AddDays(-10)),
            News("Old 2", 900, startOfWeek.AddDays(-20)),
            News("Old 3", 800, startOfWeek.AddDays(-30)));

        await db.SaveChangesAsync();

        var result = await CreateService(db).GetNewsHighlightsAsync();

        Assert.Equal("Week Focus", result.WeeklyFocus!.Title);
        Assert.Equal(
            ["Week Featured", "Old 1", "Old 2", "Old 3"],
            result.FeaturedNews.Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task GetNewsHighlights_ExcludesDraftAndDeletedNews()
    {
        await using var db = CreateDbContext();
        var startOfWeek = GetStartOfCurrentWeek();

        await db.NewsList.AddRangeAsync(
            News("Published", 100, startOfWeek.AddDays(1)),
            News("Draft", 10000, startOfWeek.AddDays(2), status: NewsStatus.Draft),
            News("Deleted", 20000, startOfWeek.AddDays(3), isDeleted: true));

        await db.SaveChangesAsync();

        var result = await CreateService(db).GetNewsHighlightsAsync();

        Assert.Equal("Published", result.WeeklyFocus!.Title);
        Assert.Empty(result.FeaturedNews);
    }

    [Fact]
    public async Task GetNewsHighlights_NoCurrentWeekNews_FallsBackToHighestViewPublishedNews()
    {
        await using var db = CreateDbContext();
        var startOfWeek = GetStartOfCurrentWeek();

        await db.NewsList.AddRangeAsync(
            News("Old Low", 100, startOfWeek.AddDays(-20)),
            News("Old High", 1000, startOfWeek.AddDays(-10)),
            News("Old Mid", 500, startOfWeek.AddDays(-15)));

        await db.SaveChangesAsync();

        var result = await CreateService(db).GetNewsHighlightsAsync();

        Assert.Equal("Old High", result.WeeklyFocus!.Title);
        Assert.Equal(["Old Mid", "Old Low"], result.FeaturedNews.Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task GetNewsHighlights_FeaturedCountTwo_ReturnsOnlyTwoFeaturedNews()
    {
        await using var db = CreateDbContext();
        var startOfWeek = GetStartOfCurrentWeek();

        await db.NewsList.AddRangeAsync(
            News("Focus", 1000, startOfWeek.AddDays(1)),
            News("Featured 1", 900, startOfWeek.AddDays(2)),
            News("Featured 2", 800, startOfWeek.AddDays(3)),
            News("Featured 3", 700, startOfWeek.AddDays(4)));

        await db.SaveChangesAsync();

        var result = await CreateService(db).GetNewsHighlightsAsync(2);

        Assert.Equal("Focus", result.WeeklyFocus!.Title);
        Assert.Equal(
            ["Featured 1", "Featured 2"],
            result.FeaturedNews.Select(x => x.Title).ToList());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static NewsService CreateService(AppDbContext db)
    {
        var currentUser = new Mock<ICurrentUserService>();
        return new NewsService(db, currentUser.Object);
    }

    private static DateTimeOffset GetStartOfCurrentWeek()
    {
        var now = DateTimeOffset.UtcNow;
        var daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
        return new DateTimeOffset(now.Date, now.Offset).AddDays(-daysSinceMonday);
    }

    private static News News(
        string title,
        int viewCount,
        DateTimeOffset createdAt,
        NewsStatus status = NewsStatus.Published,
        bool isDeleted = false)
    {
        return new News
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = $"{title} description",
            ViewCount = viewCount,
            CoverImage = $"https://example.com/{title}.jpg",
            Status = status,
            slug = title.ToLowerInvariant().Replace(" ", "-"),
            UserId = Guid.NewGuid(),
            AuthorName = "Admin",
            AuthorEmail = "admin@example.com",
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            CategoryNewsDetail = new List<CategoryNewsDetail>()
        };
    }
}
