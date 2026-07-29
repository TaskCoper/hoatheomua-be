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
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

public class LocalProductSearchV2Test
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
    public async Task GetLocalProductsV2_NoPinSorting_SingleCategory_SortsOnlyByPosition()
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
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 10, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 5, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = false, Position = 8, IsDeleted = false };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p1.Id, IsPin = true, Position = 30, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p2.Id, IsPin = false, Position = 10, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p3.Id, IsPin = false, Position = 20, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { category.Id }
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        // Since we removed isPin sorting, they must sort only by CategoryProductDetail.Position (10 -> 20 -> 30) -> B -> C -> A
        Assert.Equal(new[] { "B", "C", "A" }, items.Select(x => x.ProductName));
    }

    [Fact]
    public async Task GetLocalProductsV2_NoPinSorting_ZeroCategories_SortsOnlyByProductPosition()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 100, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 20, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = false, Position = 50, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            categoryIds = null
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        // Should sort only by Product.Position (20 -> 50 -> 100) -> B -> C -> A
        Assert.Equal(new[] { "B", "C", "A" }, items.Select(x => x.ProductName));
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsPinTrue_NoCategory()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 10, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 20, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = true, Position = 30, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            isPin = true
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(2, items.Count);
        Assert.Contains("A", items.Select(x => x.ProductName));
        Assert.Contains("C", items.Select(x => x.ProductName));
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsPinFalse_NoCategory()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 10, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 20, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = true, Position = 30, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            isPin = false
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("B", items[0].ProductName);
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsPinTrue_WithCategory()
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
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = false, Position = 10, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 20, IsDeleted = false };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p1.Id, IsPin = true, Position = 10, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p2.Id, IsPin = false, Position = 20, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { category.Id },
            isPin = true
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("A", items[0].ProductName);
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsPinTrue_WithCategory_PinOnProductOnly()
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
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 10, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 20, IsDeleted = false };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p1.Id, IsPin = false, Position = 10, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p2.Id, IsPin = false, Position = 20, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { category.Id },
            isPin = true
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("A", items[0].ProductName);
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsPinFalse_WithCategory()
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
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 10, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 20, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = false, Position = 30, IsDeleted = false };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p1.Id, IsPin = false, Position = 10, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p2.Id, IsPin = true, Position = 20, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p3.Id, IsPin = false, Position = 30, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { category.Id },
            isPin = false
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("C", items[0].ProductName);
    }
}
