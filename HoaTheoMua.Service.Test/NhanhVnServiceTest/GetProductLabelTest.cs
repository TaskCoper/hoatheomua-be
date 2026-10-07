using System.Net;
using System.Net.Http.Json;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.NhanhVnService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Xunit;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using NhanhVnServiceImplementation = HoaTheoMua.Service.NhanhVnService.Service;
using RevalidateService = HoaTheoMua.Service.Revalidate.IService;
using FulfillmentService = HoaTheoMua.Service.Fulfillment.IService;

namespace HoaTheoMua.Service.Test.NhanhVnServiceTest
{
    // Unit test cho HoaTheoMua.Service.NhanhVnService.Service.GetProductLabel.
    public class GetProductLabelTest
    {
        // ================= Helpers =================

        private AppDbContext CreateMockDbContext()
        {
            var mockCompany = new List<CompanyAddressEntity>().BuildMockDbSet();

            var configs = new List<ConfigEntity>
            {
                new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false }
            };
            var mockConfigs = configs.BuildMockDbSet();

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            mockDbContext.Setup(x => x.CompanyAddresses).Returns(mockCompany.Object);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);

            return mockDbContext.Object;
        }

        private IConfiguration CreateMockConfiguration()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);
            return mockConfig.Object;
        }

        private HttpClient CreateHttpClient(HttpResponseMessage response)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            return new HttpClient(mockHttpMessageHandler.Object);
        }

        private NhanhVnServiceImplementation CreateService(HttpResponseMessage response)
        {
            return new NhanhVnServiceImplementation(
                CreateHttpClient(response),
                CreateMockDbContext(),
                CreateMockConfiguration(),
                Mock.Of<RevalidateService>(),
                Mock.Of<FulfillmentService>());
        }

        private ProductLabelRequest CreateRequest()
        {
            return new ProductLabelRequest
            {
                Filters = new ProductLabelFilter { Ids = new List<int> { 293 } }
            };
        }

        // ================= TEST CASE 01: Lấy product label thành công =================
        // API /product/tags trả HTTP 200, parse được, Code != 0 => trả về ProductLabelResponse
        [Fact]
        public async Task GetProductLabel_ValidResponse_ReturnsProductLabelResponse()
        {
            // Arrange
            var labelResponse = new ProductLabelResponse
            {
                Code = 1,
                Data = new List<CategoryData>
                {
                    new CategoryData
                    {
                        Id = 293,
                        BusinessId = 126981,
                        Name = "VẬT LIỆU",
                        Status = 1,
                        TextColor = "#000",
                        CreatedAt = 1782381582
                    }
                }
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(labelResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var result = await service.GetProductLabel(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.Single(result.Data);
            Assert.Equal("VẬT LIỆU", result.Data[0].Name);
        }

        // ================= TEST CASE 02: NhanhVn trả lỗi HTTP =================
        // API /product/tags trả HTTP non-200 => BadRequestException "NhanhVn API HTTP Error"
        [Fact]
        public async Task GetProductLabel_HttpErrorStatus_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("error", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetProductLabel(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }

        // ================= TEST CASE 03: Response null hoặc empty =================
        // API trả HTTP 200 nhưng body "null" => BadRequestException "NhanhVnResponse returned null or empty data"
        [Fact]
        public async Task GetProductLabel_NullBody_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetProductLabel(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }

        // ================= TEST CASE 04: Form field không hợp lệ =================
        // API trả response có Code == 0 => BadRequestException "ERR_INVALID_FORM_FIELDS"
        [Fact]
        public async Task GetProductLabel_ResponseCodeZero_ThrowsBadRequestException()
        {
            // Arrange
            var labelResponse = new ProductLabelResponse
            {
                Code = 0,
                Data = new List<CategoryData>()
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(labelResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetProductLabel(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }

        // ================= TEST CASE 05: Outgoing request is built correctly =================
        // Kiểm tra endpoint, HTTP method, appId/businessId query string và Authorization token.
        [Fact]
        public async Task GetProductLabel_BuildsAuthorizedPostRequestToConfiguredEndpoint()
        {
            // Arrange
            HttpRequestMessage? captured = null;
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(new ProductLabelResponse
                    {
                        Code = 1,
                        Data = new List<CategoryData>()
                    })
                });

            var service = new NhanhVnServiceImplementation(
                new HttpClient(mockHandler.Object),
                CreateMockDbContext(),
                CreateMockConfiguration(),
                Mock.Of<RevalidateService>(),
                Mock.Of<FulfillmentService>());

            // Act
            await service.GetProductLabel(CreateRequest());

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(HttpMethod.Post, captured!.Method);
            Assert.Equal(
                "http://nhanhvn.test/product/tags?appId=12345&businessId=67890",
                captured.RequestUri!.ToString());
            Assert.True(captured.Headers.Contains("Authorization"));
            Assert.Equal("test_token", captured.Headers.GetValues("Authorization").First());
        }
    }
}
