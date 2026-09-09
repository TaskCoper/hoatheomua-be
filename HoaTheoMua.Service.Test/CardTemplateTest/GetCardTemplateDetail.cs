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

public class GetCardTemplateDetail
{
    [Fact]
    public async Task UT_023_01_Customer_ActiveTemplate_ReturnsFullDetail()
    {
        await using var db = CreateDbContext();
        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440023");
        var createdAt = new DateTimeOffset(2026, 9, 9, 8, 0, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddHours(1);
        await db.CardTemplates.AddAsync(Template(id, "Thiệp sinh nhật viết tay", CardTemplateType.Handmade, true,
            description: "Mẫu có sẵn", imageUrl: "https://storage.example.com/hm.png", createdAt: createdAt, updatedAt: updatedAt));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: false).GetCardTemplateDetail(id);

        Assert.Equal(id, result.Id);
        Assert.Equal("Thiệp sinh nhật viết tay", result.Name);
        Assert.Equal("Mẫu có sẵn", result.Description);
        Assert.Equal("https://storage.example.com/hm.png", result.ImageUrl);
        Assert.Equal("handmade", result.TemplateType);
        Assert.True(result.IsActive);
        Assert.False(result.IsDeleted);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.Equal(updatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task UT_023_02_Admin_InactiveTemplate_ReturnsDetail()
    {
        await using var db = CreateDbContext();
        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440024");
        await db.CardTemplates.AddAsync(Template(id, "AI template", CardTemplateType.Ai, false));
        await db.SaveChangesAsync();

        var result = await CreateService(db, isAdmin: true).GetCardTemplateDetail(id);

        Assert.Equal(id, result.Id);
        Assert.Equal("ai", result.TemplateType);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UT_023_06_ExistingGuidWithoutTemplate_ThrowsNotFound()
    {
        await using var db = CreateDbContext();
        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440099");

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(db, isAdmin: false).GetCardTemplateDetail(id));

        Assert.Equal("CARD_TEMPLATE_NOT_FOUND", exception.Message);
    }

    [Fact]
    public async Task UT_023_08_Customer_InactiveTemplate_ThrowsConflict()
    {
        await using var db = CreateDbContext();
        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440024");
        await db.CardTemplates.AddAsync(Template(id, "Inactive", CardTemplateType.Ai, false));
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService(db, isAdmin: false).GetCardTemplateDetail(id));

        Assert.Equal("CARD_TEMPLATE_INACTIVE", exception.Message);
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
        Guid id,
        string name,
        CardTemplateType templateType,
        bool isActive,
        bool isDeleted = false,
        string? description = null,
        string imageUrl = "https://example.test/template.jpg",
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        ImageUrl = imageUrl,
        TemplateType = templateType,
        IsActive = isActive,
        IsDeleted = isDeleted,
        CreatedAt = createdAt ?? new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero),
        UpdatedAt = updatedAt
    };
}
