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

public class GetProductWithListTests
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
    public async Task GetProductWithList_ProductIdsMatch_ReturnsMappedProducts()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var firstProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Rose Bouquet",
            CoverImage = "rose.jpg",
            ProductCode = "ROSE-001",
            Price = 120000,
            IsDeleted = false
        };
        var secondProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Tulip Basket",
            CoverImage = "tulip.jpg",
            ProductCode = "TULIP-001",
            Price = 180000,
            IsDeleted = false
        };
        var productNotRequested = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Not Requested",
            CoverImage = "other.jpg",
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(firstProduct, secondProduct, productNotRequested);
        await dbContext.SaveChangesAsync();

        var request = new Request.GetProductWithList
        {
            ProductIds = new List<Guid> { firstProduct.Id, secondProduct.Id }
            //nhận vào 2 id hợp lệ
        };

        // Act
        var result = await service.GetProductWithList(request);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x =>
            x.Id == firstProduct.Id &&
            x.ProductName == firstProduct.ProductName &&
            x.CoverImage == firstProduct.CoverImage);
        Assert.Contains(result, x =>
            x.Id == secondProduct.Id &&
            x.ProductName == secondProduct.ProductName &&
            x.CoverImage == secondProduct.CoverImage);
        Assert.DoesNotContain(result, x => x.Id == productNotRequested.Id);
    }

    [Fact]
    public async Task GetProductWithList_ProductIsDeleted_DoesNotReturnDeletedProduct()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var activeProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Active Product",
            CoverImage = "active.jpg",
            IsDeleted = false
        };
        var deletedProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Deleted Product",
            CoverImage = "deleted.jpg",
            IsDeleted = true
        };

        await dbContext.Products.AddRangeAsync(activeProduct, deletedProduct);
        await dbContext.SaveChangesAsync();

        var request = new Request.GetProductWithList
        {
            ProductIds = new List<Guid> { activeProduct.Id, deletedProduct.Id } 
            //nhận vào 1 id của thằng bình thường và 1 thằng bị xóa
        };

        // Act
        var result = await service.GetProductWithList(request);

        // Assert
        var product = Assert.Single(result);
        Assert.Equal(activeProduct.Id, product.Id);
        Assert.Equal(activeProduct.ProductName, product.ProductName);
        Assert.Equal(activeProduct.CoverImage, product.CoverImage);
    }

    [Fact]
    public async Task GetProductWithList_EmptyProductIds_ReturnsEmptyList()
    {
        // Arrange -> chuẩn bị data
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        await dbContext.Products.AddAsync(new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Existing Product",
            CoverImage = "existing.jpg",
            IsDeleted = false
        });
        await dbContext.SaveChangesAsync();

        var request = new Request.GetProductWithList //nhận vào list rỗng
        {
            ProductIds = new List<Guid>()
        };

        // Act
        var result = await service.GetProductWithList(request);

        // Assert
        Assert.Empty(result);
    }
}
