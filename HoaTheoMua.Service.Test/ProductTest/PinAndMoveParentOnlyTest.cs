using System;
using System.Collections.Generic;
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

public class PinAndMoveParentOnlyTest
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

        return new ProductService(dbContext, httpContextMock.Object, httpClient, configuration);
    }

    [Fact]
    public async Task MoveProduct_ChildProductWithoutCategory_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Child",
            ParentProductId = "parent-1",
            IsPin = true,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(product.Id,
            new Request.MoveProductRequest { Position = 1 }));
    }

    [Fact]
    public async Task MoveProduct_ChildProductInCategory_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Category",
            Name_En = "Category",
            Description = "Category",
            Description_En = "Category",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Child",
            ParentProductId = "parent-1",
            IsDeleted = false
        };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddAsync(product);
        await dbContext.CategoryProductDetails.AddAsync(new CategoryProductDetail
        {
            Id = Guid.NewGuid(),
            CategoryProductId = category.Id,
            ProductId = product.Id,
            IsPin = true,
            IsDeleted = false
        });
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(product.Id,
            new Request.MoveProductRequest { Position = 1, CategoryId = category.Id }));
    }

    [Fact]
    public async Task PinProduct_ChildProduct_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Child",
            ParentProductId = "parent-1",
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.PinProduct(
            new Request.PinProductRequest { ProductId = product.Id, IsPin = true }));

        Assert.Equal("Product not found or is not a parent product.", exception.Message);
    }

    // Bảo vệ case: ParentProductId là chuỗi rỗng "" (legacy data từ Nhanh.vn) được coi là parent
    [Fact]
    public async Task PinProduct_EmptyStringParentProductId_TreatedAsParent()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Root with empty string parent",
            ParentProductId = "",
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        // Should NOT throw — empty string ParentProductId is treated as parent
        var result = await service.PinProduct(
            new Request.PinProductRequest { ProductId = product.Id, IsPin = true });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task MoveProduct_EmptyStringParentProductId_TreatedAsParent()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Root with empty string parent",
            ParentProductId = "",
            IsPin = true,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        // Should NOT throw
        var result = await service.MoveProduct(product.Id,
            new Request.MoveProductRequest { Position = 1 });

        Assert.NotNull(result);
    }
}
