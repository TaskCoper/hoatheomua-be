using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using HoaTheoMua.Service.Test.TestSupport;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

public class GetLocalProductIntegrationTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static ProductService CreateService(AppDbContext dbContext)
    {
        var httpContextMock = new Mock<IHttpContextAccessor>();
        var httpClient = new HttpClient();

        var myConfiguration = new Dictionary<string, string?>
        {
            {"NhanhVnOption:AppId", "12345"},
            {"NhanhVnOption:BusinessId", "67890"},
            {"NhanhVnOption:Token", "test_token"},
            {"NhanhVnOption:BaseUrl", "https://nhanh.vn"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        return new ProductService(dbContext, httpContextMock.Object, httpClient, configuration, MapperFactory.Create());
    }

    // --- UT-004: GetLocalProductsV2 Integration Tests ---

    [Fact]
    public async Task UT_004_01_GetLocalProductsV2_ComboNoVariant_HasSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = comboId, ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat 1", Stock = 100, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = comboId, MatId = matId, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { pageIndex = 1, pageSize = 10 });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        var combo = items.FirstOrDefault(i => i.Id == comboId);
        Assert.NotNull(combo);
        Assert.Equal(100, combo.SellableQuantity);
        Assert.Equal("Còn hàng", combo.StockStatus);
    }

    [Fact]
    public async Task UT_004_02_GetLocalProductsV2_ParentComboWithVariants_CalculatesMin()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parentId = Guid.NewGuid();
        var parentNhanhId = "combo-parent-001";
        var variant1Id = Guid.NewGuid();
        var variant2Id = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = parentId, NhanhProductId = parentNhanhId, ProductType = "Sản Phẩm", ProductName = "Parent Combo", IsDeleted = false },
            new ProductEntity { Id = variant1Id, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 1", IsDeleted = false },
            new ProductEntity { Id = variant2Id, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 2", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat", Stock = 100, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddRangeAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1Id, MatId = matId, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false }, // 100/2 = 50
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2Id, MatId = matId, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false }  // 100/5 = 20
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { pageIndex = 1, pageSize = 10 });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        var parentProduct = items.FirstOrDefault(i => i.Id == parentId);
        Assert.NotNull(parentProduct);
        Assert.Equal(20, parentProduct.SellableQuantity);
        Assert.Equal("Còn hàng", parentProduct.StockStatus);
    }

    [Fact]
    public async Task UT_004_03_GetLocalProductsV2_RegularProduct_ReturnsStockAsSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var productId = Guid.NewGuid();
        await dbContext.Products.AddAsync(
            new ProductEntity { Id = productId, ProductType = "Sản Phẩm", ProductName = "Regular", Stock = 15, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { pageIndex = 1, pageSize = 10 });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        var product = items.FirstOrDefault(i => i.Id == productId);
        Assert.NotNull(product);
        Assert.Equal(15, product.SellableQuantity);
        Assert.Equal("Còn hàng", product.StockStatus);
    }

    [Fact]
    public async Task UT_004_04_GetLocalProductsV2_MixedProducts_IntegratesAllCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1Id = Guid.NewGuid();
        var comboId = Guid.NewGuid();
        var matId = Guid.NewGuid();
        var p3Id = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = p1Id, ProductType = "Sản Phẩm", ProductName = "Product 1", Stock = 10, IsDeleted = false },
            new ProductEntity { Id = comboId, ProductType = "Combo", ProductName = "Combo 2", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat", Stock = 60, IsDeleted = false },
            new ProductEntity { Id = p3Id, ProductType = "Sản Phẩm", ProductName = "Product 3", Stock = 0, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = comboId, MatId = matId, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false } // 60/2 = 30
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { pageIndex = 1, pageSize = 10 });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        var p1 = items.FirstOrDefault(i => i.Id == p1Id);
        var combo = items.FirstOrDefault(i => i.Id == comboId);
        var p3 = items.FirstOrDefault(i => i.Id == p3Id);

        Assert.NotNull(p1);
        Assert.Equal(10, p1.SellableQuantity);
        Assert.Equal("Còn hàng", p1.StockStatus);

        Assert.NotNull(combo);
        Assert.Equal(30, combo.SellableQuantity);
        Assert.Equal("Còn hàng", combo.StockStatus);

        Assert.NotNull(p3);
        Assert.Equal(0, p3.SellableQuantity);
        Assert.Equal("Hết hàng", p3.StockStatus);
    }

    [Fact]
    public async Task UT_004_05_GetLocalProductsV2_EmptyList_ReturnsNoErrors()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { pageIndex = 1, pageSize = 10 });
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task UT_004_06_GetLocalProductsV2_ComboNoFormula_ReturnsZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        await dbContext.Products.AddAsync(
            new ProductEntity { Id = comboId, ProductType = "Combo", ProductName = "Combo No Spec", IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { pageIndex = 1, pageSize = 10 });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        var combo = items.FirstOrDefault(i => i.Id == comboId);
        Assert.NotNull(combo);
        Assert.Equal(0, combo.SellableQuantity);
        Assert.Equal("Hết hàng", combo.StockStatus);
    }

    // --- UT-013: GetLocalProductDetail Integration Tests ---

    [Fact]
    public async Task UT_013_01_GetLocalProductDetail_RegularProduct_ReturnsNullSellableProperties()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var productId = Guid.NewGuid();
        await dbContext.Products.AddAsync(
            new ProductEntity { Id = productId, ProductType = "Sản Phẩm", ProductName = "Regular", Stock = 15, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(productId);

        Assert.Equal(productId, result.Id);
        Assert.Null(result.SellableQuantity);
        Assert.Null(result.StockStatus);
    }

    [Fact]
    public async Task UT_013_02_GetLocalProductDetail_ComboNoVariant_HasSellableProperties()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = comboId, ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat 1", Stock = 100, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = comboId, MatId = matId, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(comboId);

        Assert.Equal(comboId, result.Id);
        Assert.Equal(100, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_03_GetLocalProductDetail_ParentComboWithVariants_HasSellableProperties()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parentId = Guid.NewGuid();
        var parentNhanhId = "combo-parent-001";
        var variant1Id = Guid.NewGuid();
        var variant2Id = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = parentId, NhanhProductId = parentNhanhId, ProductType = "Sản Phẩm", ProductName = "Parent Combo", IsDeleted = false },
            new ProductEntity { Id = variant1Id, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 1", IsDeleted = false },
            new ProductEntity { Id = variant2Id, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 2", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat", Stock = 100, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddRangeAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1Id, MatId = matId, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false }, // 50
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2Id, MatId = matId, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false }  // 20
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parentId);

        Assert.Equal(parentId, result.Id);
        Assert.Equal(20, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);

        // Verify variants (children) list is populated with correct sellable quantities too!
        Assert.Equal(2, result.Children.Count);
        var v1 = result.Children.FirstOrDefault(c => c.Id == variant1Id);
        var v2 = result.Children.FirstOrDefault(c => c.Id == variant2Id);
        Assert.NotNull(v1);
        Assert.Equal(50, v1.SellableQuantity);
        Assert.NotNull(v2);
        Assert.Equal(20, v2.SellableQuantity);
    }

    [Fact]
    public async Task UT_013_04_GetLocalProductDetail_NonExistentProduct_ThrowsNotFound()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var nonexistentId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(nonexistentId));
    }

    [Fact]
    public async Task UT_013_05_GetLocalProductDetail_ComboOutOfStock_ReturnsZeroAndStockStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = comboId, ProductType = "Combo", ProductName = "Combo Out", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat", Stock = 0, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = comboId, MatId = matId, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(comboId);

        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_06_GetLocalProductDetail_ComboNoFormula_ReturnsZeroAndStockStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        await dbContext.Products.AddAsync(
            new ProductEntity { Id = comboId, ProductType = "Combo", ProductName = "Combo No Formula", IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(comboId);

        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_07_GetLocalProductDetail_SoftDeletedProduct_ThrowsNotFound()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var deletedId = Guid.NewGuid();
        await dbContext.Products.AddAsync(
            new ProductEntity { Id = deletedId, ProductType = "Sản Phẩm", ProductName = "Deleted", IsDeleted = true }
        );
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(deletedId));
    }

    [Fact]
    public async Task UT_013_08_GetLocalProductDetail_ParentComboWithAllVariantsOutOfStock_ReturnsZeroAndStockStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parentId = Guid.NewGuid();
        var parentNhanhId = "combo-parent-002";
        var variant1Id = Guid.NewGuid();
        var variant2Id = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = parentId, NhanhProductId = parentNhanhId, ProductType = "Sản Phẩm", ProductName = "Parent Combo", IsDeleted = false },
            new ProductEntity { Id = variant1Id, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 1", IsDeleted = false },
            new ProductEntity { Id = variant2Id, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 2", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat", Stock = 0, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddRangeAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1Id, MatId = matId, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false },
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2Id, MatId = matId, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parentId);

        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_09_GetLocalProductDetail_VariantChildIdPassed_ReturnsCorrectVariantDetails()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parentId = Guid.NewGuid();
        var parentNhanhId = "combo-parent-003";
        var variantId = Guid.NewGuid();
        var matId = Guid.NewGuid();

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = parentId, NhanhProductId = parentNhanhId, ProductType = "Sản Phẩm", ProductName = "Parent Combo", IsDeleted = false },
            new ProductEntity { Id = variantId, ParentProductId = parentNhanhId, ProductType = "Combo", ProductName = "Variant 1", IsDeleted = false },
            new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Mat", Stock = 50, IsDeleted = false }
        );

        await dbContext.ComboSpecifications.AddAsync(
            new ComboSpecification { Id = Guid.NewGuid(), SubId = variantId, MatId = matId, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false }
        );
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(variantId);

        Assert.Equal(variantId, result.Id);
        Assert.Equal("Combo", result.ProductType);
        Assert.Equal(parentNhanhId, result.ParentProductId);
        Assert.Equal(50, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
    }
}