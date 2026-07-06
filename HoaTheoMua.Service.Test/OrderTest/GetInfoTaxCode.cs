using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Xunit;
using OrderServiceImpl = HoaTheoMua.Service.Order.Service;
using OrderResponse = HoaTheoMua.Service.Order.Response;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using ProductService = HoaTheoMua.Service.Product.IService;

namespace HoaTheoMua.Service.Test.OrderTest
{
    // Unit test cho HoaTheoMua.Service.Order.Service.GetInfoTaxCode.
    // Viết theo đúng format của AhaMoveService/EstimateShippingFee.cs (Arrange - Act - Assert,
    // mock AppDbContext bằng MockQueryable.Moq, mock HttpMessageHandler bằng Moq.Protected).
    public class GetInfoTaxCode
    {
        private const string TestTaxCode = "0312345678";

        private static AppDbContext CreateMockDbContext()
        {
            var mockOrders = new List<OrderEntity>().BuildMockDbSet();

            var configs = new List<ConfigEntity>
            {
                new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false }
            };
            var mockConfigs = configs.BuildMockDbSet();

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            mockDbContext.Setup(x => x.Orders).Returns(mockOrders.Object);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);

            return mockDbContext.Object;
        }

        // Dựng service với 1 HttpResponseMessage cố định cho lời gọi VietQR.
        private static OrderServiceImpl CreateService(HttpResponseMessage response)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("vietqr.io")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            var httpClient = new HttpClient(handler.Object);

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var currentUser = new Mock<ICurrentUserService>().Object;
            var productService = new Mock<ProductService>().Object;

            return new OrderServiceImpl(CreateMockDbContext(), currentUser, httpClient, productService, mockConfig.Object);
        }
        
        // HAPPY CASE: VietQR trả về HTTP 200, code "00" và có data
        // => trả về chuỗi "{Name}\n{Address}"
        [Fact]
        public async Task GetInfoTaxCode_ValidResponse_ReturnsNameAndAddress()
        {
            // Arrange
            var vietQrResponse = new OrderResponse.TaxcodeResponse
            {
                Code = "00",
                Desc = "Success",
                Data = new OrderResponse.CompanyData
                {
                    Name = "CONG TY TNHH ABC",
                    Address = "123 Duong Test, Quan 1, HCM"
                }
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(vietQrResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var result = await service.GetInfoTaxCode(TestTaxCode);

            // Assert
            Assert.Equal("CONG TY TNHH ABC\n123 Duong Test, Quan 1, HCM", result);
        }
        
        // EDGE CASE 1: VietQR trả về status không thành công
        // => BadRequestException "VietQR API HTTP Error"
        [Fact]
        public async Task GetInfoTaxCode_HttpErrorStatus_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("error", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetInfoTaxCode(TestTaxCode);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("VietQR API HTTP Error", exception.Message);
        }
        
        // EDGE CASE 2: HTTP 200 nhưng data == null
        // => BadRequestException "VietQR API returned null or empty data"
        [Fact]
        public async Task GetInfoTaxCode_NullData_ThrowsBadRequestException()
        {
            // Arrange
            var vietQrResponse = new OrderResponse.TaxcodeResponse
            {
                Code = "00",
                Desc = "Success",
                Data = null!
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(vietQrResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetInfoTaxCode(TestTaxCode);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("VietQR API returned null or empty data", exception.Message);
        }
        
        // EDGE CASE 3: HTTP 200, có data nhưng code != "00"
        // => BadRequestException "VietQR API Error: {Desc}"
        [Fact]
        public async Task GetInfoTaxCode_CodeNotSuccess_ThrowsBadRequestException()
        {
            // Arrange
            var vietQrResponse = new OrderResponse.TaxcodeResponse
            {
                Code = "51",
                Desc = "Khong tim thay ma so thue",
                Data = new OrderResponse.CompanyData
                {
                    Name = "N/A",
                    Address = "N/A"
                }
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(vietQrResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetInfoTaxCode(TestTaxCode);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("VietQR API Error: Khong tim thay ma so thue", exception.Message);
        }
    }
}
