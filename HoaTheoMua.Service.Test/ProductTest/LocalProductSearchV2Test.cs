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
using HoaTheoMua.Service.Test.TestSupport;
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

        return new ProductService(dbContext, httpContextMock.Object, httpClient, configuration, MapperFactory.Create());
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

    #region TDD - US002: Lọc sản phẩm là Vật Liệu (isMaterial)

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsMaterialTrue_ReturnsOnlyMaterials()
    {
        // 1. Arrange: Chuẩn bị DB giả lập
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa hồng", ProductLabel = "thành phần, hoa tươi", Position = 1, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy gói", ProductLabel = "phụ kiện, Thành PHẦN", Position = 2, IsDeleted = false }; // Test case-insensitive
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = "combo", Position = 3, IsDeleted = false };
        var p4 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Không label", ProductLabel = null, Position = 4, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3, p4);
        await dbContext.SaveChangesAsync();

        // 2. Act: Gọi hàm lấy danh sách với cờ isMaterial = true
        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            isMaterial = true
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        // 3. Assert: Kiểm tra chỉ 2 vật liệu được trả về và không phân biệt chữ hoa/thường
        Assert.Equal(2, items.Count);
        Assert.Contains("Hoa hồng", items.Select(x => x.ProductName));
        Assert.Contains("Giấy gói", items.Select(x => x.ProductName));
        Assert.DoesNotContain("Combo 1", items.Select(x => x.ProductName));
        Assert.DoesNotContain("Không label", items.Select(x => x.ProductName));
        
        // Assert phân trang
        Assert.Equal(2, result.Value.TotalItem);
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsMaterialFalseOrNull_ReturnsAllProducts()
    {
        // 1. Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa hồng", ProductLabel = "thành phần, hoa tươi", Position = 1, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = "combo", Position = 2, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.SaveChangesAsync();

        // 2. Act: Gọi hàm với isMaterial = false
        var resultFalse = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            isMaterial = false
        });
        
        // 3. Act: Gọi hàm với isMaterial = null
        var resultNull = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            isMaterial = null
        });

        // 4. Assert: Phải trả về đủ 2 sản phẩm
        Assert.Equal(2, resultFalse.Value.Items.Count);
        Assert.Equal(2, resultNull.Value.Items.Count);
    }

    [Fact]
    public async Task GetLocalProductsV2_FilterByIsMaterialTrue_WithCategory()
    {
        // 1. Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Vật liệu",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Lá bạch đàn", ProductLabel = "thành phần", Position = 1, IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy báo", ProductLabel = "thành phần", Position = 2, IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Băng keo", ProductLabel = "phụ liệu", Position = 3, IsDeleted = false };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        
        // Chỉ Lá bạch đàn và Băng keo nằm trong category này
        await dbContext.CategoryProductDetails.AddRangeAsync(
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p1.Id, IsPin = false, Position = 10, IsDeleted = false },
            new CategoryProductDetail { Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = p3.Id, IsPin = false, Position = 20, IsDeleted = false });
            
        await dbContext.SaveChangesAsync();

        // 2. Act: Truy vấn category "Vật liệu" VÀ isMaterial = true
        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            categoryIds = new List<Guid> { category.Id },
            isMaterial = true
        });

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        
        // 3. Assert: Phải chỉ trả về p1 (Lá bạch đàn), vì nó vừa thuộc category vừa là thành phần.
        // p2 là thành phần nhưng KHÔNG thuộc category -> Loại
        // p3 thuộc category nhưng KHÔNG là thành phần -> Loại
        Assert.Single(items);
        Assert.Equal("Lá bạch đàn", items[0].ProductName);
    }
    #endregion
}
