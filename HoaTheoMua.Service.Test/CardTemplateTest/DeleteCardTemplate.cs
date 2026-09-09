using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CardTemplateEntity = HoaTheoMua.Repository.Entity.CardTemplate;
using CardTemplateService = HoaTheoMua.Service.CardTemplate.Service;

namespace HoaTheoMua.Service.Test.CardTemplateTest;

public class DeleteCardTemplate
{
    [Fact]
    public async Task UT_026_01_ExistingTemplate_SoftDeletesWithoutChangingOtherFields()
    {
        await using var db = CreateDbContext();
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 9, 9, 8, 0, 0, TimeSpan.Zero);
        var previousUpdatedAt = createdAt.AddHours(1);
        await db.CardTemplates.AddAsync(Template(id, false, createdAt, previousUpdatedAt));
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteCardTemplate(id);
        var persisted = await db.CardTemplates.SingleAsync(x => x.Id == id);

        Assert.Equal("Đã xóa mẫu thiệp thành công.", result);
        Assert.True(persisted.IsDeleted);
        Assert.Equal("Template", persisted.Name);
        Assert.Equal("https://storage.example.com/template.png", persisted.ImageUrl);
        Assert.Equal(CardTemplateType.Handmade, persisted.TemplateType);
        Assert.Equal(createdAt, persisted.CreatedAt);
        Assert.NotNull(persisted.UpdatedAt);
        Assert.True(persisted.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public async Task UT_026_05_MissingTemplate_ThrowsNotFound()
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(db).DeleteCardTemplate(Guid.NewGuid()));

        Assert.Equal("CARD_TEMPLATE_NOT_FOUND", exception.Message);
    }

    [Fact]
    public async Task UT_026_06_AlreadyDeletedTemplate_ThrowsBadRequestWithoutChangingTimestamp()
    {
        await using var db = CreateDbContext();
        var id = Guid.NewGuid();
        var previousUpdatedAt = new DateTimeOffset(2026, 9, 9, 9, 0, 0, TimeSpan.Zero);
        await db.CardTemplates.AddAsync(Template(id, true, previousUpdatedAt.AddHours(-1), previousUpdatedAt));
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(db).DeleteCardTemplate(id));
        var persisted = await db.CardTemplates.SingleAsync(x => x.Id == id);

        Assert.Equal("CARD_TEMPLATE_ALREADY_DELETED", exception.Message);
        Assert.True(persisted.IsDeleted);
        Assert.Equal(previousUpdatedAt, persisted.UpdatedAt);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static CardTemplateService CreateService(AppDbContext dbContext) => new(
        dbContext,
        new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

    private static CardTemplateEntity Template(Guid id, bool isDeleted, DateTimeOffset createdAt, DateTimeOffset? updatedAt) => new()
    {
        Id = id,
        Name = "Template",
        Description = "Mô tả",
        ImageUrl = "https://storage.example.com/template.png",
        TemplateType = CardTemplateType.Handmade,
        IsActive = true,
        IsDeleted = isDeleted,
        CreatedAt = createdAt,
        UpdatedAt = updatedAt
    };
}
