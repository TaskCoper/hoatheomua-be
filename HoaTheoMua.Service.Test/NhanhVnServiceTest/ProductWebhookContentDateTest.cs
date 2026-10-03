using HoaTheoMua.Repository;
using HoaTheoMua.Service.NhanhVnService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using NhanhVnServiceImplementation = HoaTheoMua.Service.NhanhVnService.Service;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using RevalidateService = HoaTheoMua.Service.Revalidate.IService;

namespace HoaTheoMua.Service.Test.NhanhVnServiceTest;

// Webhook Product ghi NhanhUpdatedAt (ngày nội dung sản phẩm đổi) để sitemap lastmod dùng.
public class ProductWebhookContentDateTest
{
    private static readonly DateTimeOffset NhanhTime = new(2026, 9, 28, 2, 15, 0, TimeSpan.Zero);

    private static AppDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static NhanhVnServiceImplementation CreateService(AppDbContext dbContext) => new(
        new HttpClient(),
        dbContext,
        new ConfigurationBuilder().Build(),
        Mock.Of<RevalidateService>());

    private static NhanhProductAddWebhookRequest Request(long id, long? parentId = null, long? updatedAt = null) => new()
    {
        Data = new NhanhProductDetailDto { Id = id, ParentId = parentId, Name = "Sản phẩm", UpdatedAt = updatedAt }
    };

    // ---- Add ----

    [Fact]
    public async Task Add_NhanhSendsUpdatedAt_UsesNhanhDate()
    {
        await using var db = CreateDbContext();

        await CreateService(db).WebhookNhanhVnProductAdd(Request(100, updatedAt: NhanhTime.ToUnixTimeSeconds()));

        var product = await db.Products.SingleAsync(x => x.NhanhProductId == "100");
        Assert.Equal(NhanhTime, product.NhanhUpdatedAt);
    }

    [Fact]
    public async Task Add_NhanhDoesNotSendUpdatedAt_LeavesDateNull()
    {
        await using var db = CreateDbContext();

        await CreateService(db).WebhookNhanhVnProductAdd(Request(100));

        var product = await db.Products.SingleAsync(x => x.NhanhProductId == "100");
        Assert.Null(product.NhanhUpdatedAt);
    }

    [Fact]
    public async Task Add_ChildProduct_AlsoMarksParentWithCurrentTime()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "1", NhanhUpdatedAt = NhanhTime.AddDays(-30) };
        db.Products.Add(parent);
        await db.SaveChangesAsync();
        var before = DateTimeOffset.UtcNow;

        await CreateService(db).WebhookNhanhVnProductAdd(Request(2, parentId: 1, updatedAt: NhanhTime.ToUnixTimeSeconds()));

        var child = await db.Products.SingleAsync(x => x.NhanhProductId == "2");
        Assert.Equal(NhanhTime, child.NhanhUpdatedAt);
        Assert.True(parent.NhanhUpdatedAt >= before);
    }

    // ---- Update ----

    [Fact]
    public async Task Update_NhanhSendsUpdatedAt_UsesNhanhDate()
    {
        await using var db = CreateDbContext();
        var product = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "100", NhanhUpdatedAt = NhanhTime.AddDays(-5) };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        await CreateService(db).WebhookNhanhVnProductUpdate(Request(100, updatedAt: NhanhTime.ToUnixTimeSeconds()));

        Assert.Equal(NhanhTime, product.NhanhUpdatedAt);
    }

    [Fact]
    public async Task Update_NhanhDoesNotSendUpdatedAt_KeepsExistingDate()
    {
        await using var db = CreateDbContext();
        var product = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "100", NhanhUpdatedAt = NhanhTime };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        await CreateService(db).WebhookNhanhVnProductUpdate(Request(100));

        Assert.Equal(NhanhTime, product.NhanhUpdatedAt);
    }

    // [Fact]
    // public async Task Update_ChildProduct_AlsoMarksParentWithCurrentTime()
    // {
    //     await using var db = CreateDbContext();
    //     var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "1", NhanhUpdatedAt = NhanhTime.AddDays(-30) };
    //     var child = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "2", ParentProductId = "1" };
    //     db.Products.AddRange(parent, child);
    //     await db.SaveChangesAsync();
    //     await CreateService(db).WebhookNhanhVnProductUpdate(Request(2, parentId: 1, updatedAt: NhanhTime.ToUnixTimeSeconds()));
    //
    //     Assert.Equal(NhanhTime, child.NhanhUpdatedAt);
    //     Assert.Equal(NhanhTime.AddDays(-30), parent.NhanhUpdatedAt);
    // }

    // ---- Delete ----

    [Fact]
    public async Task Delete_ChildProduct_MarksParentWithCurrentTime()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "1", NhanhUpdatedAt = NhanhTime };
        var child = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "2", ParentProductId = "1" };
        db.Products.AddRange(parent, child);
        await db.SaveChangesAsync();
        var before = DateTimeOffset.UtcNow;

        await CreateService(db).WebhookNhanhVnProductDelete(
            new NhanhProductDeleteWebhookRequest { Data = new List<string> { "2" } });

        Assert.True(child.IsDeleted);
        Assert.True(parent.NhanhUpdatedAt >= before);
        Assert.False(parent.IsDeleted);
    }

    [Fact]
    public async Task Delete_ParentProduct_DoesNotTouchOtherProducts()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "1", NhanhUpdatedAt = NhanhTime };
        var other = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "9", NhanhUpdatedAt = NhanhTime };
        db.Products.AddRange(parent, other);
        await db.SaveChangesAsync();

        await CreateService(db).WebhookNhanhVnProductDelete(
            new NhanhProductDeleteWebhookRequest { Data = new List<string> { "1" } });

        Assert.True(parent.IsDeleted);
        Assert.False(other.IsDeleted);
        Assert.Equal(NhanhTime, other.NhanhUpdatedAt);
    }

    // ---- Inventory ----

    [Fact]
    public async Task Inventory_StockChange_DoesNotTouchNhanhUpdatedAt()
    {
        await using var db = CreateDbContext();
        var parent = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "1", NhanhUpdatedAt = NhanhTime };
        var child = new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "2", ParentProductId = "1", NhanhUpdatedAt = NhanhTime };
        db.Products.AddRange(parent, child);
        await db.SaveChangesAsync();

        await CreateService(db).WebhookNhanhVnInventory(new NhanhInventoryChangeWebhookRequest
        {
            Data = new List<NhanhInventoryItem> { new() { Id = 2, Remain = 10, Available = 10 } }
        });

        Assert.Equal(NhanhTime, child.NhanhUpdatedAt);
        Assert.Equal(NhanhTime, parent.NhanhUpdatedAt);
    }
}
