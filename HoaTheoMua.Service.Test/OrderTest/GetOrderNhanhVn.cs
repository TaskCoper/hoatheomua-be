using System.Net;
using System.Net.Http.Json;
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
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.OrderTest
{
    // Unit test cho HoaTheoMua.Service.Order.Service.GetOrderNhanhVn.
    public class GetOrderNhanhVnTests
    {
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
                httpClient,
                new Mock<HoaTheoMua.Service.Product.IService>().Object,
                mockConfig.Object);
        }

        private static Mock<HttpMessageHandler> CreateHttpHandler(HttpResponseMessage response)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            return handler;
        }

        [Fact]
        public async Task GetOrderNhanhVn_Success_ReturnsOrderListRootResponse()
        {
            // Arrange
            var responseData = new OrderListRootResponse { Code = 1, Data = new List<OrderData>() };
            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(responseData) });
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetOrderNhanhVn(new NhanhVnOrderListRequest.OrderListRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
        }

        [Fact]
        public async Task GetOrderNhanhVn_HttpError_ThrowsBadRequestException()
        {
            // Arrange
            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetOrderNhanhVn(new NhanhVnOrderListRequest.OrderListRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }

        [Fact]
        public async Task GetOrderNhanhVn_ResponseNull_ThrowsBadRequestException()
        {
            // Arrange
            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create<OrderListRootResponse?>(null) });
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetOrderNhanhVn(new NhanhVnOrderListRequest.OrderListRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }

        [Fact]
        public async Task GetOrderNhanhVn_CodeZero_ThrowsBadRequestException()
        {
            // Arrange
            var responseData = new OrderListRootResponse { Code = 0 };
            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(responseData) });
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetOrderNhanhVn(new NhanhVnOrderListRequest.OrderListRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }
    }
}
