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
using HoaTheoMua.Service.Test.TestSupport;
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

        return new ProductService(dbContext, httpContextMock.Object, httpClient, configuration, MapperFactory.Create());
    }

    [Fact]
    public async Task GetLocalProducts_PriceRangeFilter_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 50, IsActive = true, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 100, IsActive = true, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 150, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            priceFrom = 80,
            priceTo = 160
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        Assert.NotNull(result);
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(2, items.Count);
        Assert.Contains(items, x => x.ProductName == "P2");
        Assert.Contains(items, x => x.ProductName == "P3");
    }

    [Fact]
    public async Task GetLocalProducts_InactiveProduct_ExcludesItFromResults()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        dbContext.Products.AddRange(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                ProductName = "Active product",
                IsActive = true,
                IsDeleted = false
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                ProductName = "Inactive product",
                IsActive = false,
                IsDeleted = false
            });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProducts(new Request.LocalProductQueryRequest());

        var item = Assert.Single(result.Value.Items.Cast<Response.ProductDetailResponse>());
        Assert.Equal("Active product", item.ProductName);
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

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 50, IsActive = true, IsDeleted = false, CreatedAt = DateTimeOffset.UtcNow };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 100, IsActive = true, IsDeleted = false, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(1) };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 150, IsActive = true, IsDeleted = false, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(2) };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            sortBy = new List<string> { sortOption }
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
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
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B_Product", Price = 100, IsActive = true, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A_Product", Price = 100, IsActive = true, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C_Product", Price = 200, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            sortBy = new List<string> { "+price", "+name" }
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(3, items.Count);
        Assert.Equal("A_Product", items[0].ProductName); // price 100, sorted alphabetically A first
        Assert.Equal("B_Product", items[1].ProductName); // price 100, sorted alphabetically B second
        Assert.Equal("C_Product", items[2].ProductName); // price 200 last
    }

    [Fact]
    public async Task GetLocalProductDetail_ReturnsProductFields_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Bông Hồng",
            OtherImages = new List<string> { "rose-1.jpg", "rose-2.jpg" },
            NhanhCategoryId = 101,
            NhanhBrandId = 202,
            NhanhTypeId = 303,
            NhanhUpdatedAt = new DateTimeOffset(2026, 8, 3, 10, 30, 0, TimeSpan.Zero),
            IsDeleted = false
        };

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Hoa Tươi",
            Name_En = "Fresh Flowers",
            Description = "Hoa tươi",
            Description_En = "Fresh Flowers",
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
        Assert.Equal(new[] { "rose-1.jpg", "rose-2.jpg" }, result.OtherImages);
        Assert.Equal(101, result.NhanhCategoryId);
        Assert.Equal(202, result.NhanhBrandId);
        Assert.Equal(303, result.NhanhTypeId);
        Assert.Equal(product.NhanhUpdatedAt, result.NhanhUpdatedAt);
        Assert.Empty(result.Children);
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

    [Fact]
    public async Task GetLocalProductDetail_ReturnsCategoriesWithParentAndChildren()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Product",
            IsDeleted = false
        };
        var parentCategory = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Parent category",
            Name_En = "Parent category",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        var linkedCategory = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Linked category",
            Name_En = "Linked category",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false,
            ParentCategoryProductId = parentCategory.Id
        };
        var childCategory = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Child category",
            Name_En = "Child category",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false,
            ParentCategoryProductId = linkedCategory.Id
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.CategoryProducts.AddRangeAsync(parentCategory, linkedCategory, childCategory);
        await dbContext.CategoryProductDetails.AddAsync(new CategoryProductDetail
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            CategoryProductId = linkedCategory.Id,
            IsPin = true,
            IsDeleted = false
        });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetLocalProductDetail(product.Id);

        // Assert
        var category = Assert.Single(result.Categories);
        Assert.Equal(linkedCategory.Id, category.categoryId);
        Assert.True(category.isPinned);
        Assert.Equal(parentCategory.Id, category.Parent?.categoryId);
        Assert.Empty(category.Parent!.children!);
        var child = Assert.Single(category.children!);
        Assert.Equal(childCategory.Id, child.categoryId);
    }

    [Fact]
    public async Task GetLocalProductDetail_WithChildren_ReturnsAllActiveChildren()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "1000",
            ProductName = "Parent",
            IsDeleted = false
        };
        var firstChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "1001",
            ParentProductId = "1000",
            ProductName = "First child",
            Position = 1,
            IsDeleted = false
        };
        var secondChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "1002",
            ParentProductId = "1000",
            ProductName = "Second child",
            Position = 2,
            IsDeleted = false
        };
        var deletedChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "1003",
            ParentProductId = "1000",
            ProductName = "Deleted child",
            Position = 3,
            IsDeleted = true
        };
        var unrelatedProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "2000",
            ParentProductId = "9999",
            ProductName = "Unrelated product",
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(
            parent,
            secondChild,
            deletedChild,
            unrelatedProduct,
            firstChild);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetLocalProductDetail(parent.Id);

        // Assert
        Assert.Equal(parent.Id, result.Id);
        Assert.Equal(2, result.Children.Count);
        Assert.Contains(result.Children, child => child.Id == firstChild.Id);
        Assert.Contains(result.Children, child => child.Id == secondChild.Id);
        Assert.Null(result.Parent);

        var childResult = await service.GetLocalProductDetail(firstChild.Id);

        Assert.NotNull(childResult.Parent);
        Assert.Equal(parent.Id, childResult.Parent.Id);
        Assert.Equal(parent.NhanhProductId, childResult.Parent.NhanhProductId);
    }

    [Fact]
    public async Task GetLocalProducts_CategoryIdsFilter_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var c1 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Cate 1", Name_En = "Cate 1", Description = "Cate 1", Description_En = "Cate 1", Type = CategoryProductType.Fix, IsActive = true, IsDeleted = false };
        var c2 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Cate 2", Name_En = "Cate 2", Description = "Cate 2", Description_En = "Cate 2", Type = CategoryProductType.Fix, IsActive = true, IsDeleted = false };

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", IsActive = true, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", IsActive = true, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", IsActive = true, IsDeleted = false };

        var j1 = new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c1.Id, ProductId = p1.Id, IsDeleted = false };
        var j2 = new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c2.Id, ProductId = p2.Id, IsDeleted = false };

        await dbContext.CategoryProducts.AddRangeAsync(c1, c2);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.CategoryProductDetails.AddRangeAsync(j1, j2);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { c1.Id }
        };

        // Act
        var result = await service.GetLocalProducts(request);

        // Assert
        Assert.NotNull(result);
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("P1", items[0].ProductName);
    }

    [Fact]
    public async Task GetLocalProducts_OneCategory_SortsByCategoryPositionFirst()
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
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = true, Position = 1, IsActive = true, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = false, Position = 3, IsActive = true, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = false, Position = 2, IsActive = true, IsDeleted = false };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p1.Id, IsPin = false, Position = 1, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p2.Id, IsPin = true, Position = 2, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p3.Id, IsPin = true, Position = 1, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProducts(new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { category.Id },
            sortBy = new List<string> { "+name" }
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(new[] { "A", "C", "B" }, items.Select(x => x.ProductName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetLocalProducts_ZeroOrMultipleCategories_SortsByProductPositionFirst(
        int categoryFilterMode)
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var c1 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "C1",
            Name_En = "C1",
            Description = "C1",
            Description_En = "C1",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        var c2 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "C2",
            Name_En = "C2",
            Description = "C2",
            Description_En = "C2",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "A", IsPin = false, Position = 1, IsActive = true, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "B", IsPin = true, Position = 2, IsActive = true, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "C", IsPin = true, Position = 1, IsActive = true, IsDeleted = false };

        await dbContext.CategoryProducts.AddRangeAsync(c1, c2);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c1.Id, ProductId = p1.Id, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c1.Id, ProductId = p2.Id, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c2.Id, ProductId = p3.Id, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var categoryIds = categoryFilterMode switch
        {
            0 => null,
            1 => new List<Guid>(),
            _ => new List<Guid> { c1.Id, c2.Id }
        };

        var result = await service.GetLocalProducts(new Request.LocalProductQueryRequest
        {
            categoryIds = categoryIds,
            sortBy = new List<string> { "+name" }
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(new[] { "A", "C", "B" }, items.Select(x => x.ProductName));
    }

    [Fact]
    public async Task GetMaxPrice_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 50, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 100, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 80, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetMaxPrice();

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public async Task GetMaxPrice_WithCategoryId_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var c1 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "C1", Name_En = "C1", Description = "C1", Description_En = "C1", Type = CategoryProductType.Fix, IsActive = true, IsDeleted = false };
        var c2 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "C2", Name_En = "C2", Description = "C2", Description_En = "C2", Type = CategoryProductType.Fix, IsActive = true, IsDeleted = false };

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 50, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 100, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 80, IsDeleted = false };

        var j1 = new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c1.Id, ProductId = p1.Id, IsDeleted = false };
        var j2 = new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c1.Id, ProductId = p3.Id, IsDeleted = false };
        var j3 = new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = c2.Id, ProductId = p2.Id, IsDeleted = false };

        await dbContext.CategoryProducts.AddRangeAsync(c1, c2);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.CategoryProductDetails.AddRangeAsync(j1, j2, j3);
        await dbContext.SaveChangesAsync();

        // Act
        var resultForC1 = await service.GetMaxPrice(new List<Guid> { c1.Id });
        var resultForC2 = await service.GetMaxPrice(new List<Guid> { c2.Id });
        var resultForBoth = await service.GetMaxPrice(new List<Guid> { c1.Id, c2.Id });

        // Assert
        Assert.Equal(80, resultForC1);
        Assert.Equal(100, resultForC2);
        Assert.Equal(100, resultForBoth);
    }

    [Fact]
    public async Task GetMaxPrice_NoProducts_ReturnsZero()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetMaxPrice();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetLocalProducts_ProductTypeFilter_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", ProductLabel = "Sản phẩm", IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", ProductLabel = "thành phần", IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", ProductLabel = "THÀNH PHẦN", IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            productType = ProductType.Material
        };

        // Act
        var result = await service.GetLocalProductsV2(request);

        // Assert
        Assert.NotNull(result);
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(2, items.Count);
        Assert.Contains(items, x => x.ProductName == "P2");
        Assert.Contains(items, x => x.ProductName == "P3");
    }
}
