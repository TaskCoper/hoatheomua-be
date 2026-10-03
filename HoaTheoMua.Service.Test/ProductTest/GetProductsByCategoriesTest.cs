using HoaTheoMua.Repository;
using CategoryProductEntity = HoaTheoMua.Repository.Entity.CategoryProduct;
using CategoryProductDetailEntity = HoaTheoMua.Repository.Entity.CategoryProductDetail;
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

public class GetProductsByCategoriesTest
{
    private static readonly DateTimeOffset CreatedDate = new(2025, 1, 5, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedDate = new(2026, 9, 28, 0, 0, 0, TimeSpan.Zero);

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

    private static CategoryProductEntity Category() => new()
    {
        Id = Guid.NewGuid(), Name = "Danh mục", Name_En = "Category", CreatedAt = DateTimeOffset.UtcNow
    };

    private static ProductEntity Product(string name, bool isActive = true, bool isDeleted = false, string? parentProductId = null)
        => new()
        {
            Id = Guid.NewGuid(), ProductName = name, IsActive = isActive, IsDeleted = isDeleted,
            NhanhProductId = name, ParentProductId = parentProductId,
            NhanhCreatedAt = CreatedDate, NhanhUpdatedAt = UpdatedDate
        };

    private static CategoryProductDetailEntity Link(CategoryProductEntity category, ProductEntity product, bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = product.Id,
        IsDeleted = isDeleted, CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task GetProductsByCategories_ReturnsIdNameAndNhanhDates()
    {
        await using var db = CreateDbContext();
        var category = Category();
        var product = Product("Bó hồng đỏ");
        db.CategoryProducts.Add(category);
        db.Products.Add(product);
        db.CategoryProductDetails.Add(Link(category, product));
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetProductsByCategories(new List<Guid> { category.Id });

        var item = Assert.Single(result);
        Assert.Equal(product.Id, item.Id);
        Assert.Equal("Bó hồng đỏ", item.Name);
        Assert.Equal(CreatedDate, item.NhanhCreatedAt);
        Assert.Equal(UpdatedDate, item.NhanhUpdatedAt);
    }

    [Fact]
    public async Task GetProductsByCategories_SkipsDeletedInactiveChildAndDeletedLink()
    {
        await using var db = CreateDbContext();
        var category = Category();
        var ok = Product("OK");
        var deleted = Product("DELETED", isDeleted: true);
        var inactive = Product("INACTIVE", isActive: false);
        var child = Product("CHILD", parentProductId: "OK");
        var removedLink = Product("REMOVED-LINK");
        db.CategoryProducts.Add(category);
        db.Products.AddRange(ok, deleted, inactive, child, removedLink);
        db.CategoryProductDetails.AddRange(
            Link(category, ok), Link(category, deleted), Link(category, inactive),
            Link(category, child), Link(category, removedLink, isDeleted: true));
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetProductsByCategories(new List<Guid> { category.Id });

        Assert.Equal(new[] { "OK" }, result.Select(x => x.Name));
    }

    [Fact]
    public async Task GetProductsByCategories_ProductInManyCategories_ReturnedOnce()
    {
        await using var db = CreateDbContext();
        var c1 = Category();
        var c2 = Category();
        var product = Product("A");
        db.CategoryProducts.AddRange(c1, c2);
        db.Products.Add(product);
        db.CategoryProductDetails.AddRange(Link(c1, product), Link(c2, product));
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetProductsByCategories(new List<Guid> { c1.Id, c2.Id });

        Assert.Single(result);
    }

    [Fact]
    public async Task GetProductsByCategories_OtherCategory_NotReturned()
    {
        await using var db = CreateDbContext();
        var c1 = Category();
        var c2 = Category();
        var product = Product("A");
        db.CategoryProducts.AddRange(c1, c2);
        db.Products.Add(product);
        db.CategoryProductDetails.Add(Link(c2, product));
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetProductsByCategories(new List<Guid> { c1.Id });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductsByCategories_NullOrEmptyIds_ReturnsEmptyList()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        Assert.Empty(await service.GetProductsByCategories(null));
        Assert.Empty(await service.GetProductsByCategories(new List<Guid>()));
        Assert.Empty(await service.GetProductsByCategories(new List<Guid> { Guid.Empty }));
    }
}
