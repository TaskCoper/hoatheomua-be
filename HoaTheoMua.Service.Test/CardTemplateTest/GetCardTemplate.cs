using System.Security.Claims;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.CardTemplate;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CardTemplateEntity = HoaTheoMua.Repository.Entity.CardTemplate;
using CardTemplateService = HoaTheoMua.Service.CardTemplate.Service;

namespace HoaTheoMua.Service.Test.CardTemplateTest;

public class GetCardTemplate
{
    [Fact]
    public async Task UT_022_01_Customer_OnlySeesActiveNonDeletedTemplatesOfRequestedType()
    {
        await using var db = CreateDbContext();
        await db.CardTemplates.AddRangeAsync(
            Template("HM-01", CardTemplateType.Handmade, true, 1),
            Template("HM-02", CardTemplateType.Handmade, false, 2),
            Template("HM-03", CardTemplateType.Handmade, true, 3, isDeleted: true),
            Template("AI-01", CardTemplateType.Ai, true, 4));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: false).GetCardTemplate(1, 10, false, "handmade");

        var item = Assert.Single(result.Value.Items.Cast<Response.CardTemplateResponse>());
        Assert.Equal("HM-01", item.Name);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task UT_022_02_Admin_CanFilterInactiveAiTemplates()
    {
        await using var db = CreateDbContext();
        await db.CardTemplates.AddRangeAsync(
            Template("AI-01", CardTemplateType.Ai, true, 1),
            Template("AI-02", CardTemplateType.Ai, false, 2),
            Template("AI-03", CardTemplateType.Ai, false, 3, isDeleted: true),
            Template("HM-01", CardTemplateType.Handmade, false, 4));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetCardTemplate(1, 20, false, "ai");

        var item = Assert.Single(result.Value.Items.Cast<Response.CardTemplateResponse>());
        Assert.Equal("AI-02", item.Name);
        Assert.False(item.IsActive);
    }

    [Fact]
    public async Task UT_022_03_Customer_SearchesCaseInsensitively_AndGetsStableFirstPage()
    {
        await using var db = CreateDbContext();
        var baseTime = new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);
        var templates = Enumerable.Range(1, 7)
            .Select(i => Template($"HM-{i:00}", CardTemplateType.Handmade, true, i, createdAt: baseTime.AddMinutes(i)))
            .ToList();
        templates[4].CreatedAt = baseTime.AddMinutes(6); // HM-05 ties HM-06; ID is the tie-breaker.
        await db.CardTemplates.AddRangeAsync(templates);
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: false).GetCardTemplate(1, 5, null, "handmade", "  hM-  ");
        var items = result.Value.Items.Cast<Response.CardTemplateResponse>().ToList();

        Assert.Equal(7, result.Value.TotalCount);
        Assert.Equal(5, items.Count);
        Assert.Equal(["HM-07", "HM-06", "HM-05", "HM-04", "HM-03"], items.Select(x => x.Name));
    }

    [Fact]
    public async Task UT_022_04_Customer_NoMatchingTemplate_ReturnsEmptyDefaultPage()
    {
        await using var db = CreateDbContext();
        await db.CardTemplates.AddRangeAsync(
            Template("inactive", CardTemplateType.Handmade, false, 1),
            Template("deleted", CardTemplateType.Handmade, true, 2, isDeleted: true),
            Template("ai", CardTemplateType.Ai, true, 3));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: false).GetCardTemplate(0, 0, null, "handmade");

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Theory]
    [InlineData(null, "BAD_REQUEST")]
    [InlineData("other", "BAD_REQUEST")]
    public async Task UT_022_05_Customer_InvalidTemplateType_ThrowsBadRequest(string? templateType, string messageCode)
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(db, isAdmin: false).GetCardTemplate(1, 10, null, templateType));

        Assert.Equal(messageCode, exception.MessageCode);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static CardTemplateService CreateService(AppDbContext dbContext, bool isAdmin)
    {
        var identity = new ClaimsIdentity("test");
        if (isAdmin)
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

        return new CardTemplateService(
            dbContext,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } });
    }

    private static CardTemplateEntity Template(
        string name,
        CardTemplateType templateType,
        bool isActive,
        int id,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null) => new()
    {
        Id = new Guid($"00000000-0000-0000-0000-{id:D12}"),
        Name = name,
        ImageUrl = "https://example.test/template.jpg",
        TemplateType = templateType,
        IsActive = isActive,
        IsDeleted = isDeleted,
        CreatedAt = createdAt ?? new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero).AddMinutes(id)
    };
}
