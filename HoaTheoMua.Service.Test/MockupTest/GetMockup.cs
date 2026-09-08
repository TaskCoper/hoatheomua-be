using System.Security.Claims;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Mockup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MockupEntity = HoaTheoMua.Repository.Entity.Mockup;
using MockupService = HoaTheoMua.Service.Mockup.Service;

namespace HoaTheoMua.Service.Test.MockupTest;

public class GetMockupTest
{
    [Fact]
    public async Task GetMockup_AdminDefault_ReturnsBothStatusesExcludesDeletedAndPaginates()
    {
        await using var db = CreateDbContext();
        var mockups = Enumerable.Range(1, 12)
            .Select(i => Mockup($"mockup-{i:000}", i % 2 == 1, i))
            .Append(Mockup("mockup-deleted-new", true, 0, true))
            .Append(Mockup("mockup-deleted-old", false, 13, true))
            .ToList();
        await db.Mockups.AddRangeAsync(mockups);
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetMockup(0, 0, null);
        var items = result.Value.Items.Cast<Response.MockupResponse>().ToList();

        Assert.Equal(12, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(Enumerable.Range(1, 10).Select(i => $"mockup-{i:000}"), items.Select(x => x.Name));
        Assert.Contains(items, x => x.IsActive);
        Assert.Contains(items, x => !x.IsActive);
    }

    [Fact]
    public async Task GetMockup_AdminFilterActive_ReturnsOnlyActiveBeforePagination()
    {
        await using var db = CreateDbContext();
        await db.Mockups.AddRangeAsync(
            Enumerable.Range(1, 6).Select(i => Mockup($"active-{i:000}", true, i))
                .Append(Mockup("inactive", false, 7))
                .Append(Mockup("deleted-active", true, 0, true)));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetMockup(1, 5, true);
        var items = result.Value.Items.Cast<Response.MockupResponse>().ToList();

        Assert.Equal(6, result.Value.TotalCount);
        Assert.Equal(5, items.Count);
        Assert.All(items, x => Assert.True(x.IsActive));
        Assert.Equal(Enumerable.Range(1, 5).Select(i => $"active-{i:000}"), items.Select(x => x.Name));
    }

    [Fact]
    public async Task GetMockup_AdminFilterInactive_WhenNoActiveRecords_ReturnsEmptyPage()
    {
        await using var db = CreateDbContext();
        await db.Mockups.AddRangeAsync(
            Mockup("active", true, 1),
            Mockup("deleted-inactive", false, 0, true));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetMockup(1, 10, false);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Fact]
    public async Task GetMockup_AdminSecondPage_UsesDescendingCreatedAtOrder()
    {
        await using var db = CreateDbContext();
        await db.Mockups.AddRangeAsync(Enumerable.Range(1, 7).Select(i => Mockup($"mockup-{i:000}", true, i)));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetMockup(2, 5, null);
        var items = result.Value.Items.Cast<Response.MockupResponse>().ToList();

        Assert.Equal(7, result.Value.TotalCount);
        Assert.Equal(["mockup-006", "mockup-007"], items.Select(x => x.Name));
        Assert.True(result.Value.HasPreviousPage);
        Assert.False(result.Value.HasNextPage);
    }

    [Fact]
    public async Task GetMockup_Customer_IgnoresStatusFilterAndOnlyReturnsActiveSearchMatches()
    {
        await using var db = CreateDbContext();
        await db.Mockups.AddRangeAsync(
            Mockup("Mockup active", true, 1),
            Mockup("Mockup inactive", false, 2),
            Mockup("Mockup deleted active", true, 0, true),
            Mockup("Other active", true, 3));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: false).GetMockup(1, 10, false, "MOCKUP");
        var item = Assert.Single(result.Value.Items.Cast<Response.MockupResponse>());

        Assert.Equal("Mockup active", item.Name);
        Assert.True(item.IsActive);
        Assert.Equal(1, result.Value.TotalCount);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static MockupService CreateService(AppDbContext dbContext, bool isAdmin)
    {
        var identity = new ClaimsIdentity("test");
        if (isAdmin)
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

        return new MockupService(
            dbContext,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } });
    }

    private static MockupEntity Mockup(string name, bool isActive, int sequence, bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        ImageUrl = "https://example.test/mockup.jpg",
        IsActive = isActive,
        IsDeleted = isDeleted,
        CreatedAt = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero).AddHours(-sequence)
    };
}
