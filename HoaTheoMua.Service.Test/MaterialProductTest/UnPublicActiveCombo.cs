using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.MaterialProductTest;

public class UnPublicActiveCombo
{
    private static readonly Guid ComboId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task UnPublicActiveCombo_ActiveCombo_UnpublishesParentAndAllActiveChildren()
    {
        await using var db = CreateDbContext();
        db.Products.AddRange(
            Product(ComboId, "COMBO-2", true),
            Product(Guid.NewGuid(), "CHILD-1", true, "COMBO-2"),
            Product(Guid.NewGuid(), "CHILD-2", false, "COMBO-2"),
            Product(Guid.NewGuid(), "DELETED", true, "COMBO-2", true),
            Product(Guid.NewGuid(), "OTHER", true, "OTHER-COMBO"));
        await db.SaveChangesAsync();

        var result = await CreateService(db).UnPublicActiveCombo(ComboId);

        Assert.Equal("Ngừng mở bán combo thành công.", result);
        Assert.False((await db.Products.FindAsync(ComboId))!.IsActive);
        Assert.All(await db.Products.Where(x => x.ParentProductId == "COMBO-2" && !x.IsDeleted).ToListAsync(),
            x => Assert.False(x.IsActive));
        Assert.True((await db.Products.IgnoreQueryFilters().SingleAsync(x => x.NhanhProductId == "DELETED")).IsActive);
        Assert.True((await db.Products.SingleAsync(x => x.ParentProductId == "OTHER-COMBO")).IsActive);
    }

    [Fact]
    public async Task UnPublicActiveCombo_AlreadyInactive_IsIdempotent()
    {
        await using var db = CreateDbContext();
        var combo = Product(ComboId, "COMBO-2", false);
        combo.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var child = Product(Guid.NewGuid(), "CHILD-1", true, "COMBO-2");
        db.Products.AddRange(combo, child);
        await db.SaveChangesAsync();
        var oldUpdatedAt = combo.UpdatedAt;

        var result = await CreateService(db).UnPublicActiveCombo(ComboId);

        Assert.Equal("Combo đang ở trạng thái chưa công khai.", result);
        Assert.True((await db.Products.FindAsync(child.Id))!.IsActive);
        Assert.Equal(oldUpdatedAt, (await db.Products.FindAsync(ComboId))!.UpdatedAt);
    }

    [Fact]
    public async Task UnPublicActiveCombo_ComboNotFound_ThrowsNotFound()
    {
        await using var db = CreateDbContext();
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(db).UnPublicActiveCombo(ComboId));
        Assert.Equal("Không tìm thấy combo", exception.Message);
    }

    [Fact]
    public async Task UnPublicActiveCombo_DeletedParent_ThrowsNotFound()
    {
        await using var db = CreateDbContext();
        db.Products.Add(Product(ComboId, "COMBO-2", true, isDeleted: true));
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(db).UnPublicActiveCombo(ComboId));
    }

    private static ProductEntity Product(Guid id, string? nhanhProductId, bool isActive,
        string? parentProductId = null, bool isDeleted = false) => new()
    {
        Id = id, NhanhProductId = nhanhProductId, ParentProductId = parentProductId,
        IsActive = isActive, IsDeleted = isDeleted, CreatedAt = DateTimeOffset.UtcNow
    };

    private static AppDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options);

    private static HoaTheoMua.Service.Material.Service CreateService(AppDbContext db) => new(db);
}
