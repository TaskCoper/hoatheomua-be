using System.Security.Claims;
using HoaTheoMua.Repository;
using FlowerReferenceEntity = HoaTheoMua.Repository.Entity.FlowerReference;
using HoaTheoMua.Service.FlowerReference;
using HoaTheoMua.Service.FlowerReference.Validate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FlowerReferenceService = HoaTheoMua.Service.FlowerReference.Service;

namespace HoaTheoMua.Service.Test.FlowerReferenceTest;

public class GetFlowerReferenceTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("invalid")]
    public void GetFlowerReferenceValidator_InvalidType_IsInvalid(string type)
    {
        var result = new GetFlowerReferenceValidator().Validate(new Request.GetFlowerReferenceRequest { type = type });

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(Request.GetFlowerReferenceRequest.type));
    }

    [Fact]
    public async Task GetMockup_AdminDefault_ReturnsBothStatusesExcludesDeletedAndPaginates()
    {
        await using var db = CreateDbContext();
        var mockups = Enumerable.Range(1, 12)
            .Select(i => Mockup($"mockup-{i:000}", i % 2 == 1, i))
            .Append(Mockup("mockup-deleted-new", true, 0, true))
            .Append(Mockup("mockup-deleted-old", false, 13, true))
            .ToList();
        await db.FlowerReferences.AddRangeAsync(mockups);
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetFlowerReference(0, 0, null, null);
        var items = result.Value.Items.Cast<Response.FlowerReferenceResponse>().ToList();

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
        await db.FlowerReferences.AddRangeAsync(
            Enumerable.Range(1, 6).Select(i => Mockup($"active-{i:000}", true, i))
                .Append(Mockup("inactive", false, 7))
                .Append(Mockup("deleted-active", true, 0, true)));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetFlowerReference(1, 5, true, null);
        var items = result.Value.Items.Cast<Response.FlowerReferenceResponse>().ToList();

        Assert.Equal(6, result.Value.TotalCount);
        Assert.Equal(5, items.Count);
        Assert.All(items, x => Assert.True(x.IsActive));
        Assert.Equal(Enumerable.Range(1, 5).Select(i => $"active-{i:000}"), items.Select(x => x.Name));
    }

    [Fact]
    public async Task GetMockup_AdminFilterInactive_WhenNoActiveRecords_ReturnsEmptyPage()
    {
        await using var db = CreateDbContext();
        await db.FlowerReferences.AddRangeAsync(
            Mockup("active", true, 1),
            Mockup("deleted-inactive", false, 0, true));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetFlowerReference(1, 10, false, null);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Fact]
    public async Task GetMockup_AdminSecondPage_UsesDescendingCreatedAtOrder()
    {
        await using var db = CreateDbContext();
        await db.FlowerReferences.AddRangeAsync(Enumerable.Range(1, 7).Select(i => Mockup($"mockup-{i:000}", true, i)));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetFlowerReference(2, 5, null, null);
        var items = result.Value.Items.Cast<Response.FlowerReferenceResponse>().ToList();

        Assert.Equal(7, result.Value.TotalCount);
        Assert.Equal(["mockup-006", "mockup-007"], items.Select(x => x.Name));
        Assert.True(result.Value.HasPreviousPage);
        Assert.False(result.Value.HasNextPage);
    }

    [Fact]
    public async Task GetMockup_Customer_IgnoresStatusFilterAndOnlyReturnsActiveSearchMatches()
    {
        await using var db = CreateDbContext();
        await db.FlowerReferences.AddRangeAsync(
            Mockup("Mockup active", true, 1),
            Mockup("Mockup inactive", false, 2),
            Mockup("Mockup deleted active", true, 0, true),
            Mockup("Other active", true, 3));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: false).GetFlowerReference(1, 10, false, null, "MOCKUP");
        var item = Assert.Single(result.Value.Items.Cast<Response.FlowerReferenceResponse>());

        Assert.Equal("Mockup active", item.Name);
        Assert.True(item.IsActive);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetFlowerReference_TypeFilter_ReturnsOnlyMatchingType()
    {
        await using var db = CreateDbContext();
        await db.FlowerReferences.AddRangeAsync(
            Mockup("flower", true, 1),
            Mockup("bundle", true, 2, type: HoaTheoMua.Repository.Enum.FlowerReferenceType.Bundle));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetFlowerReference(1, 10, null, "bundle");
        var item = Assert.Single(result.Value.Items.Cast<Response.FlowerReferenceResponse>());

        Assert.Equal("bundle", item.Name);
        Assert.Equal("bundle", item.Type);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static FlowerReferenceService CreateService(AppDbContext dbContext, bool isAdmin)
    {
        var identity = new ClaimsIdentity("test");
        if (isAdmin)
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

        return new FlowerReferenceService(
            dbContext,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } });
    }

    private static FlowerReferenceEntity Mockup(string name, bool isActive, int sequence, bool isDeleted = false, HoaTheoMua.Repository.Enum.FlowerReferenceType type = HoaTheoMua.Repository.Enum.FlowerReferenceType.Flower) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        ImageUrl = "https://example.test/mockup.jpg",
        IsActive = isActive,
        IsDeleted = isDeleted,
        Type = type,
        CreatedAt = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero).AddHours(-sequence)
    };
}
