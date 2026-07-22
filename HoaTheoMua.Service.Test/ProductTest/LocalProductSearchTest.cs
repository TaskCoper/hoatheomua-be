using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using CategoryProductEntity = HoaTheoMua.Repository.Entity.CategoryProduct;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

public class LocalProductSearchTest
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

        var myConfiguration = new Dictionary<string, string>
        {
            {"NhanhVnOption:AppId", "12345"},
            {"NhanhVnOption:BusinessId", "67890"},
            {"NhanhVnOption:Token", "test_token"},
            {"NhanhVnOption:BaseUrl", "https://nhanh.vn"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        return new ProductService(dbContext, httpContextMock.Object, httpClient, configuration);
    }

    [Fact]
    public async Task GetLocalProducts_PriceRangeFilter_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 50, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 100, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 150, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            PriceFrom = 80,
            PriceTo = 160
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        Assert.NotNull(result);
        var items = result.Value.Items.Cast<ProductEntity>().ToList();
        Assert.Equal(2, items.Count);
        Assert.Contains(items, x => x.ProductName == "P2");
        Assert.Contains(items, x => x.ProductName == "P3");
    }

    [Theory]
    [InlineData("+price", new[] { "P1", "P2", "P3" })]
    [InlineData("-price", new[] { "P3", "P2", "P1" })]
    [InlineData("+name", new[] { "P1", "P2", "P3" })]
    [InlineData("-name", new[] { "P3", "P2", "P1" })]
    public async Task GetLocalProducts_SortBySingleField_Success(string sortOption, string[] expectedOrder)
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 50, IsDeleted = false, CreatedAt = DateTimeOffset.UtcNow };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 100, IsDeleted = false, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(1) };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 150, IsDeleted = false, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(2) };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            SortBy = new List<string> { sortOption }
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        var items = result.Value.Items.Cast<ProductEntity>().ToList();
        Assert.Equal(expectedOrder.Length, items.Count);
        for (int i = 0; i < expectedOrder.Length; i++)
        {
            Assert.Equal(expectedOrder[i], items[i].ProductName);
        }
    }

    [Fact]
    public async Task GetLocalProducts_SortByMultipleFields_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // p1 and p2 have same price, so second order by Name will settle
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B_Product", Price = 100, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A_Product", Price = 100, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C_Product", Price = 200, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            SortBy = new List<string> { "+price", "+name" }
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        var items = result.Value.Items.Cast<ProductEntity>().ToList();
        Assert.Equal(3, items.Count);
        Assert.Equal("A_Product", items[0].ProductName); // price 100, sorted alphabetically A first
        Assert.Equal("B_Product", items[1].ProductName); // price 100, sorted alphabetically B second
        Assert.Equal("C_Product", items[2].ProductName); // price 200 last
    }

    [Fact]
    public async Task GetLocalProductDetail_WithCategories_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Bông Hồng",
            IsDeleted = false
        };

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Hoa Tươi",
            Type = CategoryProductType.Fix,
            IsActive = true
        };

        var junction = new CategoryProductDetail
        {
            Id = Guid.NewGuid(),
            CategoryProductId = category.Id,
            ProductId = product.Id
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.CategoryProductDetails.AddAsync(junction);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetLocalProductDetail(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bông Hồng", result.ProductName);
        Assert.Single(result.Categories);
        Assert.Equal("Hoa Tươi", result.Categories[0].name);
    }

    [Fact]
    public async Task GetLocalProductDetail_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(Guid.NewGuid()));
    }
}
