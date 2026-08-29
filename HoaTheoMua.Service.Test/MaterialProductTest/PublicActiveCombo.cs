using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.MaterialProductTest;

public class PublicActiveCombo
{
    private static readonly Guid ComboId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task PublicActiveCombo_InactiveProduct_PublishesOnlyRequestedProduct()
    {
        await using var db = CreateDbContext();
        db.Products.AddRange(
            Product(ComboId, "COMBO-1", false),
            Product(Guid.NewGuid(), "CHILD-1", false, "COMBO-1"),
            Product(Guid.NewGuid(), "CHILD-2", true, "COMBO-1"),
            Product(Guid.NewGuid(), "DELETED", false, "COMBO-1", true),
            Product(Guid.NewGuid(), "OTHER", false, "OTHER-COMBO"));
        await db.SaveChangesAsync();

        var result = await CreateService(db).PublicActiveCombo(ComboId);

        Assert.Equal("Mở bán thành công.", result);
        Assert.True((await db.Products.FindAsync(ComboId))!.IsActive);
        Assert.False((await db.Products.SingleAsync(x => x.NhanhProductId == "CHILD-1")).IsActive);
        Assert.True((await db.Products.SingleAsync(x => x.NhanhProductId == "CHILD-2")).IsActive);
        Assert.False((await db.Products.IgnoreQueryFilters().SingleAsync(x => x.NhanhProductId == "DELETED")).IsActive);
        Assert.False((await db.Products.SingleAsync(x => x.ParentProductId == "OTHER-COMBO")).IsActive);
    }

    [Fact]
    public async Task PublicActiveCombo_AlreadyActive_IsIdempotent()
    {
        await using var db = CreateDbContext();
        var combo = Product(ComboId, "COMBO-1", true);
        combo.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var child = Product(Guid.NewGuid(), "CHILD-1", false, "COMBO-1");
        db.Products.AddRange(combo, child);
        await db.SaveChangesAsync();
        var oldUpdatedAt = combo.UpdatedAt;

        var result = await CreateService(db).PublicActiveCombo(ComboId);

        Assert.Equal("Sản phẩm đã được mở bán trước đó.", result);
        Assert.False((await db.Products.FindAsync(child.Id))!.IsActive);
        Assert.Equal(oldUpdatedAt, (await db.Products.FindAsync(ComboId))!.UpdatedAt);
    }

    [Fact]
    public async Task PublicActiveCombo_ComboNotFound_ThrowsNotFound()
    {
        await using var db = CreateDbContext();
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(db).PublicActiveCombo(ComboId));
        Assert.Equal("Không tìm thấy sản phẩm", exception.Message);
    }

    [Fact]
    public async Task PublicActiveCombo_DeletedParent_ThrowsNotFound()
    {
        await using var db = CreateDbContext();
        db.Products.Add(Product(ComboId, "COMBO-1", false, isDeleted: true));
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(db).PublicActiveCombo(ComboId));
    }

    [Fact]
    public async Task PublicActiveCombo_UnsyncedProduct_PublishesSuccessfully()
    {
        await using var db = CreateDbContext();
        db.Products.Add(Product(ComboId, null, false));
        await db.SaveChangesAsync();
        var result = await CreateService(db).PublicActiveCombo(ComboId);

        Assert.Equal("Mở bán thành công.", result);
        Assert.True((await db.Products.FindAsync(ComboId))!.IsActive);
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
