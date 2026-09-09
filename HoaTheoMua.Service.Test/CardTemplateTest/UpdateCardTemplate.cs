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

public class UpdateCardTemplate
{
    [Fact]
    public async Task UpdateCardTemplate_ProvidedMutableFields_UpdatesAndPreservesOthers()
    {
        await using var db = CreateDbContext();
        var id = Guid.Parse("550e8400-e29b-41d4-a716-446655440025");
        var createdAt = new DateTimeOffset(2026, 9, 9, 8, 0, 0, TimeSpan.Zero);
        var previousUpdatedAt = createdAt.AddHours(1);
        await db.CardTemplates.AddAsync(Template(id, "Tên cũ", "Mô tả cũ", true, createdAt, previousUpdatedAt));
        await db.SaveChangesAsync();

        var result = await CreateService(db).UpdateCardTemplate(id, new Request.UpdateCardTemplateRequest
        {
            Name = "Tên mới",
            IsActive = false
        });

        Assert.Equal("Tên mới", result.Name);
        Assert.Equal("Mô tả cũ", result.Description);
        Assert.Equal("https://storage.example.com/old.png", result.ImageUrl);
        Assert.False(result.IsActive);
        Assert.Equal("handmade", result.TemplateType);
        Assert.False(result.IsDeleted);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public async Task UpdateCardTemplate_EmptyRequest_ThrowsBadRequestBeforeDatabaseLookup()
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(db).UpdateCardTemplate(Guid.NewGuid(), new Request.UpdateCardTemplateRequest()));

        Assert.Equal("CARD_TEMPLATE_UPDATE_REQUIRED", exception.Message);
    }

    [Fact]
    public async Task UpdateCardTemplate_MissingTemplate_ThrowsNotFound()
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(db).UpdateCardTemplate(Guid.NewGuid(), new Request.UpdateCardTemplateRequest { Name = "Tên mới" }));

        Assert.Equal("TEMPLATE_NOT_FOUND", exception.Message);
    }

    [Fact]
    public async Task UpdateCardTemplate_DeletedTemplate_ThrowsNotFoundAndDoesNotChangeIt()
    {
        await using var db = CreateDbContext();
        var id = Guid.NewGuid();
        var previousUpdatedAt = new DateTimeOffset(2026, 9, 9, 9, 0, 0, TimeSpan.Zero);
        await db.CardTemplates.AddAsync(Template(id, "Tên cũ", "Mô tả cũ", true, previousUpdatedAt.AddHours(-1), previousUpdatedAt, isDeleted: true));
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(db).UpdateCardTemplate(id, new Request.UpdateCardTemplateRequest { Name = "Tên mới" }));
        var persisted = await db.CardTemplates.SingleAsync(x => x.Id == id);

        Assert.Equal("TEMPLATE_NOT_FOUND", exception.Message);
        Assert.Equal("Tên cũ", persisted.Name);
        Assert.Equal(previousUpdatedAt, persisted.UpdatedAt);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static CardTemplateService CreateService(AppDbContext dbContext) => new(
        dbContext,
        new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

    private static CardTemplateEntity Template(
        Guid id,
        string name,
        string? description,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        bool isDeleted = false) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        ImageUrl = "https://storage.example.com/old.png",
        TemplateType = CardTemplateType.Handmade,
        IsActive = isActive,
        IsDeleted = isDeleted,
        CreatedAt = createdAt,
        UpdatedAt = updatedAt
    };
}
