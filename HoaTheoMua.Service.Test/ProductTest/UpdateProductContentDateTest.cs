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

// Admin sửa mô tả sản phẩm ở web mình thì ngày nội dung (NhanhUpdatedAt) của sản phẩm và sản phẩm cha đổi.
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
    public async Task UpdateProduct_WritesUpdatedAtForAudit()
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
    public async Task UpdateProductV2_ParentProduct_OnlyMarksItself()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);
        var before = DateTimeOffset.UtcNow;

        await CreateService(db).UpdateProductV2(parent.Id, new Request.UpdateProductRequestV2 { Description = "Mô tả mới" });

        Assert.True(parent.NhanhUpdatedAt >= before);
        Assert.Equal(OldDate, child.NhanhUpdatedAt);
    }

    [Fact]
    public async Task UpdateProductV2_DescriptionNull_DoesNotMarkDate()
    {
        await using var db = CreateDbContext();
        var (parent, child) = await SeedParentAndChild(db);

        await CreateService(db).UpdateProductV2(child.Id, new Request.UpdateProductRequestV2 { Description = null });

        Assert.Equal(OldDate, child.NhanhUpdatedAt);
        Assert.Equal(OldDate, parent.NhanhUpdatedAt);
    }
}
