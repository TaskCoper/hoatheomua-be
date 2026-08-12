using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
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

    [Fact]
    public async Task UT_005_01_StandaloneCombo_CalculatesCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId1 = Guid.NewGuid();
        var matId2 = Guid.NewGuid();

        var combo = new ProductEntity
        {
            Id = comboId,
            ProductType = "Combo",
            ProductName = "Combo Standalone",
            IsDeleted = false
        };

        var mat1 = new ProductEntity
        {
            Id = matId1,
            ProductType = "Sản Phẩm",
            ProductName = "Material 1",
            Stock = 10,
            IsDeleted = false
        };

        var mat2 = new ProductEntity
        {
            Id = matId2,
            ProductType = "Sản Phẩm",
            ProductName = "Material 2",
            Stock = 15,
            IsDeleted = false
        };

        var spec1 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId1,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        var spec2 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId2,
            Quantity = 3,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { comboId });

        Assert.True(result.ContainsKey(comboId));
        // mat1: 10 / 2 = 5
        // mat2: 15 / 3 = 5
        // min = 5
        Assert.Equal(5, result[comboId].SellableQuantity);
        Assert.Equal("Còn hàng", result[comboId].StockStatus);
    }

    [Fact]
    public async Task UT_005_02_ParentProductWithComboVariants_CalculatesMinOfVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parentId = Guid.NewGuid();
        var parentNhanhId = "parent-nhanh-id";

        var parent = new ProductEntity
        {
            Id = parentId,
            NhanhProductId = parentNhanhId,
            ProductType = "Sản Phẩm",
            ProductName = "Parent Combo",
            IsDeleted = false
        };

        var variant1Id = Guid.NewGuid();
        var variant1 = new ProductEntity
        {
            Id = variant1Id,
            ParentProductId = parentNhanhId,
            ProductType = "Combo",
            ProductName = "Variant 1",
            IsDeleted = false
        };

        var variant2Id = Guid.NewGuid();
        var variant2 = new ProductEntity
        {
            Id = variant2Id,
            ParentProductId = parentNhanhId,
            ProductType = "Combo",
            ProductName = "Variant 2",
            IsDeleted = false
        };

        var matId = Guid.NewGuid();
        var mat = new ProductEntity
        {
            Id = matId,
            ProductType = "Sản Phẩm",
            ProductName = "Material",
            Stock = 12,
            IsDeleted = false
        };

        // Variant 1 needs 2 -> available = 12 / 2 = 6
        var spec1 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = variant1Id,
            MatId = matId,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        // Variant 2 needs 3 -> available = 12 / 3 = 4
        var spec2 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = variant2Id,
            MatId = matId,
            Quantity = 3,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { parentId });

        Assert.True(result.ContainsKey(parentId));
        // Parent quantity is min(Variant 1 qty (6), Variant 2 qty (4)) = 4
        Assert.Equal(4, result[parentId].SellableQuantity);
        Assert.Equal("Còn hàng", result[parentId].StockStatus);
    }

    [Fact]
    public async Task UT_005_03_RegularProduct_ReturnsStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var normalId = Guid.NewGuid();
        var normal = new ProductEntity
        {
            Id = normalId,
            ProductType = "Sản Phẩm",
            ProductName = "Regular Product",
            Stock = 15,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(normal);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { normalId });

        Assert.True(result.ContainsKey(normalId));
        Assert.Equal(15, result[normalId].SellableQuantity);
        Assert.Equal("Còn hàng", result[normalId].StockStatus);
    }

    [Fact]
    public async Task UT_005_04_NoCoreSpecification_ReturnsZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId = Guid.NewGuid();

        var combo = new ProductEntity
        {
            Id = comboId,
            ProductType = "Combo",
            ProductName = "Combo with No Core",
            IsDeleted = false
        };

        var mat = new ProductEntity
        {
            Id = matId,
            ProductType = "Sản Phẩm",
            ProductName = "Material",
            Stock = 10,
            IsDeleted = false
        };

        var spec = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId,
            Quantity = 2,
            IsCore = false, // Not core!
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { comboId });

        Assert.True(result.ContainsKey(comboId));
        Assert.Equal(0, result[comboId].SellableQuantity);
        Assert.Equal("Hết hàng", result[comboId].StockStatus);
    }

    [Fact]
    public async Task UT_005_05_OutofStock_ReturnsZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId = Guid.NewGuid();

        var combo = new ProductEntity
        {
            Id = comboId,
            ProductType = "Combo",
            ProductName = "Combo Out of Stock",
            IsDeleted = false
        };

        var mat = new ProductEntity
        {
            Id = matId,
            ProductType = "Sản Phẩm",
            ProductName = "Material",
            Stock = 0,
            IsDeleted = false
        };

        var spec = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { comboId });

        Assert.True(result.ContainsKey(comboId));
        Assert.Equal(0, result[comboId].SellableQuantity);
        Assert.Equal("Hết hàng", result[comboId].StockStatus);
    }

    [Fact]
    public async Task UT_005_06_QuantityZeroOrNegative_IsIgnoredToAvoidDivideByZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId1 = Guid.NewGuid();
        var matId2 = Guid.NewGuid();

        var combo = new ProductEntity
        {
            Id = comboId,
            ProductType = "Combo",
            ProductName = "Combo Zero Qty",
            IsDeleted = false
        };

        var mat1 = new ProductEntity
        {
            Id = matId1,
            ProductType = "Sản Phẩm",
            ProductName = "Material 1",
            Stock = 10,
            IsDeleted = false
        };

        var mat2 = new ProductEntity
        {
            Id = matId2,
            ProductType = "Sản Phẩm",
            ProductName = "Material 2",
            Stock = 15,
            IsDeleted = false
        };

        var spec1 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId1,
            Quantity = 0, // 0 quantity
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        var spec2 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId2,
            Quantity = 3, // Valid core
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { comboId });

        Assert.True(result.ContainsKey(comboId));
        // spec1 (Quantity 0) is ignored. Only spec2 is evaluated.
        // mat2: 15 / 3 = 5
        Assert.Equal(5, result[comboId].SellableQuantity);
        Assert.Equal("Còn hàng", result[comboId].StockStatus);
    }

    [Fact]
    public async Task UT_005_07_InactiveOrDeletedSpecifications_AreFiltered()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var matId1 = Guid.NewGuid();
        var matId2 = Guid.NewGuid();

        var combo = new ProductEntity
        {
            Id = comboId,
            ProductType = "Combo",
            ProductName = "Combo Specification Filter Test",
            IsDeleted = false
        };

        var mat1 = new ProductEntity
        {
            Id = matId1,
            ProductType = "Sản Phẩm",
            ProductName = "Material 1",
            Stock = 10,
            IsDeleted = false
        };

        var mat2 = new ProductEntity
        {
            Id = matId2,
            ProductType = "Sản Phẩm",
            ProductName = "Material 2",
            Stock = 15,
            IsDeleted = false
        };

        var spec1 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId1,
            Quantity = 2,
            IsCore = true,
            IsActive = false, // Inactive!
            IsDeleted = false
        };

        var spec2 = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = matId2,
            Quantity = 3,
            IsCore = true,
            IsActive = true,
            IsDeleted = true // Deleted!
        };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { comboId });

        Assert.True(result.ContainsKey(comboId));
        // No active and non-deleted core specs -> 0 quantity
        Assert.Equal(0, result[comboId].SellableQuantity);
        Assert.Equal("Hết hàng", result[comboId].StockStatus);
    }

    [Fact]
    public async Task UT_005_08_EmptyOrNullInput_ReturnsEmptyDictionary()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var resultNull = await service.CalculateAvailableQty(null!);
        var resultEmpty = await service.CalculateAvailableQty(new List<Guid>());

        Assert.Empty(resultNull);
        Assert.Empty(resultEmpty);
    }

    [Fact]
    public async Task UT_005_09_StockStatusLogic_BasedOnQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo1Id = Guid.NewGuid();
        var combo2Id = Guid.NewGuid();
        var matId = Guid.NewGuid();

        var combo1 = new ProductEntity { Id = combo1Id, ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false };
        var combo2 = new ProductEntity { Id = combo2Id, ProductType = "Combo", ProductName = "Combo 2", IsDeleted = false };
        var mat = new ProductEntity { Id = matId, ProductType = "Sản Phẩm", ProductName = "Material", Stock = 1, IsDeleted = false };

        // Combo 1 requires 1 -> available = 1 / 1 = 1 > 0 -> "Còn hàng"
        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo1Id, MatId = matId, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };
        // Combo 2 requires 2 -> available = 1 / 2 = 0 -> "Hết hàng"
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo2Id, MatId = matId, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo1, combo2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo1Id, combo2Id });

        Assert.Equal("Còn hàng", result[combo1Id].StockStatus);
        Assert.Equal(1, result[combo1Id].SellableQuantity);

        Assert.Equal("Hết hàng", result[combo2Id].StockStatus);
        Assert.Equal(0, result[combo2Id].SellableQuantity);
    }

    [Fact]
    public async Task UT_005_10_MaterialNotFoundInDb_IsTreatedAsZeroStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var comboId = Guid.NewGuid();
        var nonExistentMatId = Guid.NewGuid();

        var combo = new ProductEntity
        {
            Id = comboId,
            ProductType = "Combo",
            ProductName = "Combo missing Material",
            IsDeleted = false
        };

        var spec = new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = nonExistentMatId, // Material does not exist in db
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(combo);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { comboId });

        Assert.True(result.ContainsKey(comboId));
        // Missing material -> stock treated as 0 -> available = 0
        Assert.Equal(0, result[comboId].SellableQuantity);
        Assert.Equal("Hết hàng", result[comboId].StockStatus);
    }
}