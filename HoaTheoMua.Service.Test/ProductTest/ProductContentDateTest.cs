using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.ProductTest;

public class ProductContentDateTest
{
    private static AppDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static readonly DateTimeOffset Old = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset New = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Mark_NoDateYet_SetsDate()
    {
        var product = new ProductEntity();

        ProductContentDate.Mark(product, New);

        Assert.Equal(New, product.NhanhUpdatedAt);
    }

    [Fact]
    public void Mark_NewerDate_UpdatesDate()
    {
        var product = new ProductEntity { NhanhUpdatedAt = Old };

        ProductContentDate.Mark(product, New);

        Assert.Equal(New, product.NhanhUpdatedAt);
    }

    [Fact]
    public void Mark_OlderDate_KeepsExistingDate()
    {
        var product = new ProductEntity { NhanhUpdatedAt = New };

        ProductContentDate.Mark(product, Old);

        Assert.Equal(New, product.NhanhUpdatedAt);
    }

    [Fact]
    public async Task MarkWithParentAsync_ChildProduct_MarksChildAndParent()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "P1", NhanhUpdatedAt = Old };
        var child = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "C1", ParentProductId = "P1" };
        db.Products.AddRange(parent, child);
        await db.SaveChangesAsync();

        await ProductContentDate.MarkWithParentAsync(db, child, New);

        Assert.Equal(New, child.NhanhUpdatedAt);
        Assert.Equal(New, parent.NhanhUpdatedAt);
    }

    [Fact]
    public async Task MarkWithParentAsync_ParentProduct_OnlyMarksItself()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "P1" };
        var other = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "P2", NhanhUpdatedAt = Old };
        db.Products.AddRange(parent, other);
        await db.SaveChangesAsync();

        await ProductContentDate.MarkWithParentAsync(db, parent, New);

        Assert.Equal(New, parent.NhanhUpdatedAt);
        Assert.Equal(Old, other.NhanhUpdatedAt);
    }

    [Fact]
    public async Task MarkWithParentAsync_ParentMissingOrDeleted_DoesNotThrowAndSkipsParent()
    {
        await using var db = CreateDbContext();
        var deletedParent = new ProductEntity
        {
            Id = Guid.NewGuid(), NhanhProductId = "P1", IsDeleted = true, NhanhUpdatedAt = Old
        };
        var child = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "C1", ParentProductId = "P1" };
        var orphan = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "C2", ParentProductId = "NOPE" };
        db.Products.AddRange(deletedParent, child, orphan);
        await db.SaveChangesAsync();

        await ProductContentDate.MarkWithParentAsync(db, child, New);
        await ProductContentDate.MarkWithParentAsync(db, orphan, New);

        Assert.Equal(Old, deletedParent.NhanhUpdatedAt);
        Assert.Equal(New, child.NhanhUpdatedAt);
        Assert.Equal(New, orphan.NhanhUpdatedAt);
    }

    [Fact]
    public async Task MarkParentAsync_ChildProduct_OnlyMarksParent()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "P1" };
        var child = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "C1", ParentProductId = "P1" };
        db.Products.AddRange(parent, child);
        await db.SaveChangesAsync();

        await ProductContentDate.MarkParentAsync(db, child, New);

        Assert.Equal(New, parent.NhanhUpdatedAt);
        Assert.Null(child.NhanhUpdatedAt);
    }

    [Fact]
    public void Snapshot_StockAndPositionChange_DoesNotChangeSnapshot()
    {
        var product = new ProductEntity { ProductName = "Hoa", Price = 100m, Stock = 5, ImportPrice = 10m, WebOrder = 1 };
        var before = ProductContentDate.Snapshot(product);

        product.Stock = 99;
        product.ImportPrice = 50m;
        product.WebOrder = 7;
        product.Position = 3;
        product.IsPin = true;

        Assert.Equal(before, ProductContentDate.Snapshot(product));
    }

    [Fact]
    public void Snapshot_VisibleFieldChange_ChangesSnapshot()
    {
        var product = new ProductEntity { ProductName = "Hoa", Price = 100m };
        var before = ProductContentDate.Snapshot(product);

        product.Price = 120m;

        Assert.NotEqual(before, ProductContentDate.Snapshot(product));
    }
}
