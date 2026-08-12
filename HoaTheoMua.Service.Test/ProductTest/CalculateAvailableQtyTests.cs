using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using HoaTheoMua.Service.Product;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using HoaTheoMua.Service.Test.TestSupport;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

public class CalculateAvailableQtyTests
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

    #region UT-005: CalculateAvailableQty Method Tests

    [Fact]
    public async Task UT_005_01_StandaloneCombo_CalculatesCorrectlyBasedOnMinCoreComponentRatio()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 99, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 3, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(33, result[combo.Id].SellableQuantity);
        Assert.Equal("Còn hàng", result[combo.Id].StockStatus);
    }

    [Fact]
    public async Task UT_005_02_ComboParent_CalculatesMinOfChildVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };

        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false }; // Qty = 50
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false }; // Qty = 20

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { parent.Id });

        Assert.True(result.ContainsKey(parent.Id));
        Assert.Equal(20, result[parent.Id].SellableQuantity);
    }

    [Fact]
    public async Task UT_005_03_NormalProduct_RetrievesStockDirectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 15, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { product.Id });

        Assert.True(result.ContainsKey(product.Id));
        Assert.Equal(15, result[product.Id].SellableQuantity);
        Assert.Equal("Còn hàng", result[product.Id].StockStatus);
    }

    [Fact]
    public async Task UT_005_04_ComboWithNoCoreComponents_ReturnsZeroQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = false, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Equal("Hết hàng", result[combo.Id].StockStatus);
    }

    [Fact]
    public async Task UT_005_05_ComboWithOutOfStockCoreComponent_ReturnsZeroQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 0, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 50, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Equal("Hết hàng", result[combo.Id].StockStatus);
    }

    [Fact]
    public async Task UT_005_06_RequiredQuantityZeroOrNegative_AvoidsDivideByZeroAndSkips()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 50, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 0, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(25, result[combo.Id].SellableQuantity);
    }

    [Fact]
    public async Task UT_005_07_SoftDeletedOrInactiveSpecifications_AreFilteredOut()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 50, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = true }; // deleted
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(50, result[combo.Id].SellableQuantity);
    }

    [Fact]
    public async Task UT_005_08_EmptyInputList_ReturnsEmptyDictionary()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var result = await service.CalculateAvailableQty(new List<Guid>());

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UT_005_09_StockStatusDeterminism_CorrectlyMapsStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var combo2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 1, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo1.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo2.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false }; // requires 2 but stock is 1 -> qty = 0

        await dbContext.Products.AddRangeAsync(combo1, combo2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo1.Id, combo2.Id });

        Assert.Equal("Còn hàng", result[combo1.Id].StockStatus);
        Assert.Equal(1, result[combo1.Id].SellableQuantity);

        Assert.Equal("Hết hàng", result[combo2.Id].StockStatus);
        Assert.Equal(0, result[combo2.Id].SellableQuantity);
    }

    [Fact]
    public async Task UT_005_10_ComponentMissingFromInventory_TreatedAsZeroStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        // matId does not refer to any existing product in database
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = Guid.NewGuid(), Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddAsync(combo);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Equal("Hết hàng", result[combo.Id].StockStatus);
    }

    #endregion

    #region UT-004: GetLocalProductsV2 Integration Tests

    [Fact]
    public async Task UT_004_01_GetLocalProductsV2_StandaloneCombo_HasSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        Assert.Equal(100, mappedCombo.SellableQuantity);
        Assert.Equal("Còn hàng", mappedCombo.StockStatus);
    }

    [Fact]
    public async Task UT_004_02_GetLocalProductsV2_ComboParent_HasSellableQuantityAsMinOfVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", ProductName = "Parent", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedParent = items.FirstOrDefault(i => i.Id == parent.Id);

        Assert.NotNull(mappedParent);
        Assert.Equal(20, mappedParent.SellableQuantity);
        Assert.Equal("Còn hàng", mappedParent.StockStatus);
    }

    [Fact]
    public async Task UT_004_03_GetLocalProductsV2_NormalProduct_HasSellableQuantityAsStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", ProductName = "Normal Product", Stock = 15, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedProduct = items.FirstOrDefault(i => i.Id == product.Id);

        Assert.NotNull(mappedProduct);
        Assert.Equal(15, mappedProduct.SellableQuantity);
        Assert.Equal("Còn hàng", mappedProduct.StockStatus);
    }

    [Fact]
    public async Task UT_004_04_GetLocalProductsV2_MixedProducts()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", ProductName = "Normal 1", Stock = 10, IsDeleted = false };
        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false };
        var product2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", ProductName = "Normal 2", Stock = 0, IsDeleted = false };

        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 30, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(product1, combo, product2, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var p1 = items.FirstOrDefault(i => i.Id == product1.Id);
        var c = items.FirstOrDefault(i => i.Id == combo.Id);
        var p2 = items.FirstOrDefault(i => i.Id == product2.Id);

        Assert.NotNull(p1);
        Assert.Equal(10, p1.SellableQuantity);
        Assert.Equal("Còn hàng", p1.StockStatus);

        Assert.NotNull(c);
        Assert.Equal(30, c.SellableQuantity);
        Assert.Equal("Còn hàng", c.StockStatus);

        Assert.NotNull(p2);
        Assert.Equal(0, p2.SellableQuantity);
        Assert.Equal("Hết hàng", p2.StockStatus);
    }

    [Fact]
    public async Task UT_004_05_GetLocalProductsV2_EmptyDatabase_ReturnsEmpty()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task UT_004_06_GetLocalProductsV2_ComboWithoutSpecs_ReturnsZeroQty()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "No Specs", IsDeleted = false };
        await dbContext.Products.AddAsync(combo);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        Assert.Equal(0, mappedCombo.SellableQuantity);
        Assert.Equal("Hết hàng", mappedCombo.StockStatus);
    }

    #endregion

    #region UT-013: GetLocalProductDetail Integration Tests

    [Fact]
    public async Task UT_013_01_GetLocalProductDetail_NormalProduct_HasNullSellableQuantityAndStockStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 15, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(product.Id);

        Assert.Equal(product.Id, result.Id);
        Assert.Null(result.SellableQuantity);
        Assert.Null(result.StockStatus);
    }

    [Fact]
    public async Task UT_013_02_GetLocalProductDetail_StandaloneCombo_HasSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(100, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_03_GetLocalProductDetail_ComboParent_HasSellableQuantityAsMinOfVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parent.Id);

        Assert.Equal(parent.Id, result.Id);
        Assert.Equal(20, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_04_GetLocalProductDetail_NonExistentProduct_ThrowsNotFoundException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var nonExistentId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(nonExistentId));
        Assert.Equal("PRODUCT_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UT_013_05_GetLocalProductDetail_ComboOutOfStock_HasZeroQtyAndStockStatusOutOfStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 0, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_06_GetLocalProductDetail_ComboWithoutFormula_HasZeroQtyAndStockStatusOutOfStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        await dbContext.Products.AddAsync(combo);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_07_GetLocalProductDetail_SoftDeletedProduct_ThrowsNotFoundException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", IsDeleted = true };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(product.Id));
        Assert.Equal("PRODUCT_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UT_013_08_GetLocalProductDetail_ComboParentWithAllVariantsOutOfStock_HasZeroQtyAndStockStatusOutOfStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 0, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parent.Id);

        Assert.Equal(parent.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
    }

    [Fact]
    public async Task UT_013_09_GetLocalProductDetail_ChildVariantComboDirectQuery_ReturnsIndividualQty()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản Phẩm", Stock = 50, IsDeleted = false };

        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(variant.Id);

        Assert.Equal(variant.Id, result.Id);
        Assert.Equal(50, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
    }

    #endregion
}
