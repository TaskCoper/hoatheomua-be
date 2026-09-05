using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
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

public class UpdateLabelProductNhanhVnTest
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
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new ProductService(
            dbContext,
            new Mock<IHttpContextAccessor>().Object,
            new HttpClient(),
            configuration,
            MapperFactory.Create());
    }
    //Test case: update thành công
    [Fact]
    public async Task UpdateLabelProductNhanhVn_ValidRequest_UpdatesLabelsForAllProducts()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var p1 = new ProductEntity { Id = Guid.NewGuid(), IsDeleted = false };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), IsDeleted = false };
        var p3 = new ProductEntity { Id = Guid.NewGuid(), IsDeleted = false };
        var p4 = new ProductEntity { Id = Guid.NewGuid(), IsDeleted = false };

        await dbContext.Products.AddRangeAsync(p1, p2, p3, p4);
        await dbContext.SaveChangesAsync();

        var request = new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "VẬT LIỆU", ProductIds = new List<string> { p1.Id.ToString(), p2.Id.ToString(), p2.Id.ToString() } },
            new() { Label = "PHỤ LIỆU", ProductIds = new List<string> { p1.Id.ToString(), p3.Id.ToString() } },
            new() { Label = "vật liệu", ProductIds = new List<string> { p1.Id.ToString(), p3.Id.ToString() } },
            new() { Label = "hoa chúc mừng", ProductIds = new List<string> { p4.Id.ToString() } }
        };

        var result = await service.UpdateLabelProductNhanhVn(request);

        Assert.Equal("Cập nhật nhãn cho sản phẩm thành công", result);
        Assert.Equal("VẬT LIỆU, PHỤ LIỆU", p1.ProductLabel);
        Assert.Equal("VẬT LIỆU", p2.ProductLabel);
        Assert.Equal("PHỤ LIỆU, vật liệu", p3.ProductLabel);
        Assert.Equal("hoa chúc mừng", p4.ProductLabel);
    }
    //Các test case phía xuống là Validate
    [Fact]
    public async Task UpdateLabelProductNhanhVn_EmptyRequest_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateLabelProductNhanhVn(new List<Request.UpdateLabelProductNhanhVnRequest>()));

        Assert.Equal("Products are required", exception.Message);
    }

    [Fact]
    public async Task UpdateLabelProductNhanhVn_EmptyLabel_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var request = new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "   ", ProductIds = new List<string> { Guid.NewGuid().ToString() } }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateLabelProductNhanhVn(request));

        Assert.Equal("Label is required", exception.Message);
    }

    [Fact]
    public async Task UpdateLabelProductNhanhVn_EmptyProductIds_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var request = new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "VẬT LIỆU", ProductIds = new List<string>() }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateLabelProductNhanhVn(request));

        Assert.Equal("ProductIds are required for label VẬT LIỆU", exception.Message);
    }

    [Fact]
    public async Task UpdateLabelProductNhanhVn_InvalidProductId_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var request = new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "VẬT LIỆU", ProductIds = new List<string> { "invalid-guid" } }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateLabelProductNhanhVn(request));

        Assert.Equal("ProductId is invalid: invalid-guid", exception.Message);
    }

    [Fact]
    public async Task UpdateLabelProductNhanhVn_ProductNotFound_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var request = new List<Request.UpdateLabelProductNhanhVnRequest>
        {
            new() { Label = "VẬT LIỆU", ProductIds = new List<string> { Guid.NewGuid().ToString() } }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateLabelProductNhanhVn(request));

        Assert.Equal("One or more products were not found or have been deleted", exception.Message);
    }
}
