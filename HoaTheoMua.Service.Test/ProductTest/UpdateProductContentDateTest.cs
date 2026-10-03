using HoaTheoMua.Repository;
using HoaTheoMua.Service.Product;
using HoaTheoMua.Service.Test.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

// Admin sửa sản phẩm ở web mình: chỉ đổi ngày nội dung (NhanhUpdatedAt) khi khách thấy trang khác đi.
public class UpdateProductContentDateTest
{
    private static readonly DateTimeOffset OldDate = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    private static AppDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private static ProductService CreateService(AppDbContext dbContext) => new(
        dbContext,
        new Mock<IHttpContextAccessor>().Object,
        new HttpClient(),
        new ConfigurationBuilder().Build(),
        MapperFactory.Create());

    private static async Task<(ProductEntity Parent, ProductEntity Child)> SeedParentAndChild(AppDbContext db)
    {
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(), NhanhProductId = "P1", NhanhUpdatedAt = OldDate,
            ProductName = "Bó hoa", Price = 100m, Stock = 5
        };
        var child = new ProductEntity
        {
            Id = Guid.NewGuid(), NhanhProductId = "C1", ParentProductId = "P1", NhanhUpdatedAt = OldDate,
            ProductName = "Bó hoa M", Price = 100m, Stock = 5
        };
        db.Products.AddRange(parent, child);
        await db.SaveChangesAsync();
        return (parent, child);
    }

    // ---- UpdateProduct (PATCH) ----

    [Fact]
    public async Task UpdateProduct_VisibleFieldChanged_MarksDateOfProductAndParent()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);
        var before = DateTimeOffset.UtcNow;

        await CreateService(db).UpdateProduct(child.Id, new Request.UpdateProductRequest { Price = 120m });

        Assert.True(child.NhanhUpdatedAt >= before);
        Assert.True(parent.NhanhUpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateProduct_OnlyStockChanged_DoesNotMarkDate()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);

        await CreateService(db).UpdateProduct(child.Id, new Request.UpdateProductRequest { Stock = 99, ImportPrice = 5m });

        Assert.Equal(99m, child.Stock);
        Assert.Equal(OldDate, child.NhanhUpdatedAt);
        Assert.Equal(OldDate, parent.NhanhUpdatedAt);
    }

    [Fact]
    public async Task UpdateProduct_SameValueSentAgain_DoesNotMarkDate()
    {
        await using var db = CreateDbContext();
        var (_, child) = await SeedParentAndChild(db);

        await CreateService(db).UpdateProduct(child.Id, new Request.UpdateProductRequest { Price = 100m, ProductName = "Bó hoa M" });

        Assert.Equal(OldDate, child.NhanhUpdatedAt);
    }

    [Fact]
    public async Task UpdateProduct_StillWritesUpdatedAtForAudit()
    {
        await using var db = CreateDbContext();
        var (_, child) = await SeedParentAndChild(db);

        await CreateService(db).UpdateProduct(child.Id, new Request.UpdateProductRequest { Stock = 1 });

        Assert.NotNull(child.UpdatedAt);
    }

    // ---- UpdateProductV2 (PUT) ----

    [Fact]
    public async Task UpdateProductV2_DescriptionProvided_MarksDateOfProductAndParent()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);
        var before = DateTimeOffset.UtcNow;

        await CreateService(db).UpdateProductV2(child.Id, new Request.UpdateProductRequestV2 { Description = "Mô tả mới" });

        Assert.True(child.NhanhUpdatedAt >= before);
        Assert.True(parent.NhanhUpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateProductV2_DescriptionNull_DoesNotMarkDate()
    {
        await using var db = CreateDbContext();
        var (_, child) = await SeedParentAndChild(db);

        await CreateService(db).UpdateProductV2(child.Id, new Request.UpdateProductRequestV2 { Description = null });

        Assert.Equal(OldDate, child.NhanhUpdatedAt);
    }

    // ---- UpdateLabelProductNhanhVn ----

    [Fact]
    public async Task UpdateLabel_LabelChanged_MarksDateOfProductAndParent()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);
        child.ProductLabel = "CŨ";
        await db.SaveChangesAsync();
        var before = DateTimeOffset.UtcNow;

        await CreateService(db).UpdateLabelProductNhanhVn(new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "MỚI", ProductIds = new List<string> { child.Id.ToString() } }
        });

        Assert.Equal("MỚI", child.ProductLabel);
        Assert.True(child.NhanhUpdatedAt >= before);
        Assert.True(parent.NhanhUpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateLabel_SameLabel_DoesNotMarkDate()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);
        child.ProductLabel = "GIỮ NGUYÊN";
        await db.SaveChangesAsync();

        await CreateService(db).UpdateLabelProductNhanhVn(new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "GIỮ NGUYÊN", ProductIds = new List<string> { child.Id.ToString() } }
        });

        Assert.Equal(OldDate, child.NhanhUpdatedAt);
        Assert.Equal(OldDate, parent.NhanhUpdatedAt);
    }
}
