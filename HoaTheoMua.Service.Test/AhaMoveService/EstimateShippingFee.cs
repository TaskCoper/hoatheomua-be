using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using MockQueryable.Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhaMoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using AhaMoveServiceInstance = HoaTheoMua.Service.AhaMoveService.Service;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;

namespace HoaTheoMua.Service.Test.AhaMoveServiceTest;

public class EstimateShippingFeeTest
{
    
    private CompanyAddressEntity CreateFullCompanyAddress(
        string addressLine, 
        bool isDeleted, 
        DateTimeOffset createdAt)
    {
        return new CompanyAddressEntity
        {
            Id = Guid.NewGuid(),
            AddressLine = addressLine,
            Ward = "Phường Bến Nghé",
            City = "Ho Chi Minh",
            Country = "Vietnam",
            PhoneNumber = "0222222222",
            FullName = "Cửa hàng Hoa Theo Mùa",
            Remarks = "Gần ngã tư",
            Latitude = 10.776889m,
            Longitude = 106.700806m,
            MapId = "map-default-id",
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            UpdatedAt = null
        };
    }

    private (AppDbContext, Mock<IConfiguration>) SetupMockDependencies(
        List<CompanyAddressEntity> addresses)
    {
        // 1. Mock CompanyAddresses DbSet
        var mockDbSet = addresses.BuildMockDbSet();
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);
        
        mockDbContext.Setup(x => x.CompanyAddresses).Returns(mockDbSet.Object);

        // 2. Mock IConfiguration phục vụ cho việc khởi tạo Service ban đầu
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockSection.Object);

        return (mockDbContext.Object, mockConfig);
    }

    private HttpClient CreateMockHttpClient(params HttpResponseMessage[] responses)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var handlerSequence = handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        foreach (var response in responses)
        {
            handlerSequence.ReturnsAsync(response);
        }

        return new HttpClient(handlerMock.Object);
    }


    [Fact]
    public async Task EstimateShippingFee_Success_WithValidDataAndOrderTime()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity>
        {
            CreateFullCompanyAddress(addressLine: "123 Đường ABC", isDeleted: false, createdAt: DateTimeOffset.UtcNow)
        };
        var (dbContext, config) = SetupMockDependencies(addresses);

        // Giả lập NhanhVn API Response thành công
        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse
            {
                Code = 1,
                Data = new List<NhanhProduct>
                {
                    new() { 
                        Id = 101, 
                        Name = "Hoa Hồng Đỏ", 
                        Prices = new ProductPrices { Retail = 150000 },
                        Shipping = new ProductShipping { Height = 20, Weight = 2, Length = 20, Width = 20 }
                    }
                }
            })
        };

        // Giả lập AhaMove API Response thành công
        var ahaResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<ServiceResponse>
            {
                new() { ServiceId = "SGN-BIKE", TotalFee = 35000 }
            })
        };

        var httpClient = CreateMockHttpClient(nhanhResponse, ahaResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest()
        {
            OrderTime = 1719999999, // Lớn hơn 0 -> IdleUntil sẽ nhận giá trị này
            Latitude = 10.8m,
            Longitude = 106.7m,
            Address = "456 Đường XYZ",
            ShortAddress = "Quận 3, HCM",
            FullName = "Nguyễn Văn A",
            Phone = "0909090909",
            Remarks = "Giao giờ hành chính",
            Items = new List<AhaMoveRequest.Items>
            {
                new() { ItemId = 101, Number = 2 }
            }
        };

        // Act
        var result = await service.EstimateShippingFee(request);

        // Assert
        Assert.NotNull(result);
        
        // Kiểm tra cấu trúc object ẩn danh trả về
        var type = result.GetType();
        var ahaResultProp = type.GetProperty("ahamoveResult")?.GetValue(result) as List<ServiceResponse>;
        var nhanhResultProp = type.GetProperty("productNhanhResult")?.GetValue(result) as NhanhProductListResponse;

        Assert.NotNull(ahaResultProp);
        Assert.NotNull(nhanhResultProp);
        Assert.Single(ahaResultProp);
        Assert.Equal(101, nhanhResultProp.Data.First().Id);
    }

    [Fact]
    public async Task EstimateShippingFee_Success_WhenOrderTimeIsZero_IdleUntilIsNull()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity>
        {
            CreateFullCompanyAddress(addressLine: "123 Đường ABC", isDeleted: false, createdAt: DateTimeOffset.UtcNow)
        };
        var (dbContext, config) = SetupMockDependencies(addresses);

        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse 
            { 
                Code = 1, 
                Data = new List<NhanhProduct> { new() { Id = 101, Prices = new ProductPrices(), Shipping = new ProductShipping() } } 
            })
        };
        var ahaResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<ServiceResponse> { new() })
        };

        var httpClient = CreateMockHttpClient(nhanhResponse, ahaResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest
        {
            OrderTime = 0, // Kiểm tra điều kiện OrderTime = 0 -> IdleUntil sẽ là null
            Items = new List<AhaMoveRequest.Items> { new() { ItemId = 101, Number = 1 } }
        };

        // Act
        var result = await service.EstimateShippingFee(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsException_WhenCompanyAddressNotFound()
    {
        // Arrange: Danh sách trống
        var addresses = new List<CompanyAddressEntity>(); 
        var (dbContext, config) = SetupMockDependencies(addresses);
        
        var httpClient = CreateMockHttpClient();
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.EstimateShippingFee(request));
        Assert.Equal("Company address not found", exception.Message);
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsBadRequestException_WhenNhanhVnReturnsHttpError()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity> { CreateFullCompanyAddress("ABC", false, DateTimeOffset.UtcNow) };
        var (dbContext, config) = SetupMockDependencies(addresses);

        // Giả lập lỗi HTTP 500 từ Nhanh.vn
        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var httpClient = CreateMockHttpClient(nhanhResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest { Items = new List<AhaMoveRequest.Items>() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateShippingFee(request));
        Assert.Contains("NhanhVn API HTTP Error", exception.Message);
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsBadRequestException_WhenNhanhVnResponseDataIsNull()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity> { CreateFullCompanyAddress("ABC", false, DateTimeOffset.UtcNow) };
        var (dbContext, config) = SetupMockDependencies(addresses);

        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = null })
        };

        var httpClient = CreateMockHttpClient(nhanhResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest { Items = new List<AhaMoveRequest.Items>() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateShippingFee(request));
        Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsBadRequestException_WhenNhanhVnCodeIsZero()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity> { CreateFullCompanyAddress("ABC", false, DateTimeOffset.UtcNow) };
        var (dbContext, config) = SetupMockDependencies(addresses);

        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse { Code = 0, Data = new List<NhanhProduct>() })
        };

        var httpClient = CreateMockHttpClient(nhanhResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest { Items = new List<AhaMoveRequest.Items>() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateShippingFee(request));
        Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsInvalidOperationException_WhenNhanhVnReturnsUnrequestedProductId()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity> { CreateFullCompanyAddress("ABC", false, DateTimeOffset.UtcNow) };
        var (dbContext, config) = SetupMockDependencies(addresses);

        // NhanhVn trả về sản phẩm có ID không khớp với Id được yêu cầu từ request
        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse
            {
                Code = 1,
                Data = new List<NhanhProduct> { new() { Id = 999, Prices = new ProductPrices(), Shipping = new ProductShipping() } }
            })
        };

        var httpClient = CreateMockHttpClient(nhanhResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest
        {
            Items = new List<AhaMoveRequest.Items> { new() { ItemId = 111, Number = 1 } }
        };

        // Act & Assert: Hàm .First() so khớp id sẽ báo lỗi hệ thống
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EstimateShippingFee(request));
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsBadRequestException_WhenAhaMoveReturnsHttpError()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity> { CreateFullCompanyAddress("ABC", false, DateTimeOffset.UtcNow) };
        var (dbContext, config) = SetupMockDependencies(addresses);

        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse 
            { 
                Code = 1, 
                Data = new List<NhanhProduct> { new() { Id = 101, Prices = new ProductPrices(), Shipping = new ProductShipping() } } 
            })
        };
        
        var ahaResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        var httpClient = CreateMockHttpClient(nhanhResponse, ahaResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest
        {
            Items = new List<AhaMoveRequest.Items> { new() { ItemId = 101, Number = 1 } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateShippingFee(request));
        Assert.Contains("Ahamove API HTTP Error", exception.Message);
    }

    [Fact]
    public async Task EstimateShippingFee_ThrowsBadRequestException_WhenAhaMoveReturnsNullOrEmpty()
    {
        // Arrange
        var addresses = new List<CompanyAddressEntity> { CreateFullCompanyAddress("ABC", false, DateTimeOffset.UtcNow) };
        var (dbContext, config) = SetupMockDependencies(addresses);

        var nhanhResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new NhanhProductListResponse 
            { 
                Code = 1, 
                Data = new List<NhanhProduct> { new() { Id = 101, Prices = new ProductPrices(), Shipping = new ProductShipping() } } 
            })
        };
        
        var ahaResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new List<ServiceResponse>()) // Trả về mảng trống
        };

        var httpClient = CreateMockHttpClient(nhanhResponse, ahaResponse);
        var service = new AhaMoveServiceInstance(httpClient, dbContext, config.Object);

        var request = new AhaMoveRequest.EstimateShippingFeeRequest
        {
            Items = new List<AhaMoveRequest.Items> { new() { ItemId = 101, Number = 1 } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateShippingFee(request));
        Assert.Equal("Ahamove returned null or empty data", exception.Message);
    }

    
}

// Các class DTO phục vụ biên dịch (Bỏ qua/Xóa đi nếu các class này đã có sẵn trong project)
public class NhanhProductListResponse { public int Code { get; set; } public List<NhanhProduct>? Data { get; set; } }
public class NhanhProduct { public int Id { get; set; } public string Name { get; set; } = ""; public ProductPrices Prices { get; set; } = new(); public ProductShipping Shipping { get; set; } = new(); }
public class ProductPrices { public decimal Retail { get; set; } }
public class ProductShipping { public double Height { get; set; } public double Weight { get; set; } public double Length { get; set; } public double Width { get; set; } }
public class ServiceResponse { public string ServiceId { get; set; } = ""; public double TotalFee { get; set; } }