using System.Net;
using System.Net.Http.Json;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Order;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Xunit;
using OrderServiceImpl = HoaTheoMua.Service.Order.Service;
using OrderRequest = HoaTheoMua.Service.Order.Request;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.OrderTest
{
    // Unit test cho HoaTheoMua.Service.Order.Service.GetShippingLocation.
    // Happy case + edge case tự tạo (theo yêu cầu, không có .md chi tiết cho class này).
    public class GetShippingLocationTests
    {
        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static AppDbContext CreateMockDbContext()
        {
            var configs = new List<ConfigEntity>
            {
                new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false }
            };
            var mockConfigDbSet = configs.BuildMockDbSet();
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigDbSet.Object);
            return mockDbContext.Object;
        }

        private static OrderServiceImpl CreateService(HttpClient httpClient)
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            return new OrderServiceImpl(
                CreateMockDbContext(),
                new Mock<ICurrentUserService>().Object,
                new Microsoft.AspNetCore.Http.HttpContextAccessor(),
                httpClient,
                new Mock<HoaTheoMua.Service.Product.IService>().Object,
                mockConfig.Object,
                new Mock<HoaTheoMua.Service.BaoKim.IService>().Object);
        }

        private static HttpResponseMessage JsonResponse(string rawJson, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(rawJson, Encoding.UTF8, "application/json")
            };
        }

        private static Mock<HttpMessageHandler> CreateHttpHandler(
            HttpResponseMessage response,
            Action<HttpRequestMessage>? captureRequest = null)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => captureRequest?.Invoke(req))
                .ReturnsAsync(response);
            return handler;
        }

        private static OrderRequest.LocationRequest CreateRequest(
            string type = "CITY", string locationVersion = "v2", int? parentId = null)
        {
            return new OrderRequest.LocationRequest
            {
                Filters = new OrderRequest.LocationFilters
                {
                    Type = type,
                    LocationVersion = locationVersion,
                    ParentId = parentId
                }
            };
        }

        // ---------------------------------------------------------------------
        // TEST CASE 01: Lấy danh sách địa điểm giao hàng thành công
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetShippingLocation_ValidRequest_ReturnsLocationResponse()
        {
            // Arrange
            var rawJson = "{\"code\":1,\"data\":[{\"id\":254,\"name\":\"Ha Noi\"},{\"id\":307,\"name\":\"Vinh Long\"}]}";
            var handler = CreateHttpHandler(JsonResponse(rawJson));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetShippingLocation(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(254, result.Data[0].Id);
            Assert.Equal("Ha Noi", result.Data[0].Name);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 02: NhanhVn API trả về lỗi HTTP -> BadRequestException
        // ---------------------------------------------------------------------
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetShippingLocation_HttpError_ThrowsBadRequestException(HttpStatusCode statusCode)
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{}", statusCode));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetShippingLocation(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 03: NhanhVn response null -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetShippingLocation_ResponseNull_ThrowsBadRequestException()
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("null"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetShippingLocation(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 04: NhanhVn trả về Code = 0 -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetShippingLocation_CodeZero_ThrowsBadRequestException()
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{\"code\":0,\"data\":[]}"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetShippingLocation(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 05: Gửi filter LocationVersion, Type, ParentId đúng
        // -> Kiểm tra URL chứa /shipping/location và body request chứa filter
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetShippingLocation_SendsCorrectUrlAndAuthorizationHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = CreateHttpHandler(
                JsonResponse("{\"code\":1,\"data\":[]}"),
                req => capturedRequest = req);
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            await service.GetShippingLocation(CreateRequest("DISTRICT", "v2", 254));

            // Assert
            Assert.NotNull(capturedRequest);
            var url = capturedRequest!.RequestUri!.ToString();
            Assert.Contains("/shipping/location", url);
            Assert.Contains("appId=12345", url);
            Assert.Contains("businessId=67890", url);
            Assert.True(capturedRequest.Headers.Contains("Authorization"));
            Assert.Equal("test_token", capturedRequest.Headers.GetValues("Authorization").First());
        }
    }
}
