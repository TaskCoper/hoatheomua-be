using System.Net;
using System.Net.Http.Json;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Xunit;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using AhaMoveServiceImplementation = HoaTheoMua.Service.AhamoveService.Service;

namespace HoaTheoMua.Service.Test.AhaMoveServiceTest
{
    public class GetServiceDetailsAhaMove
    {

        private AppDbContext CreateMockDbContext()
        {
            var companyAddresses = new List<CompanyAddressEntity>();
            var mockDbSet = companyAddresses.BuildMockDbSet();

            var configs = new List<HoaTheoMua.Repository.Entity.Config>
            {
                new HoaTheoMua.Repository.Entity.Config { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "ahamove.base_url", Value = "{\"value\": \"http://ahamove.test\"}", Group = "ahamove", IsDeleted = false }
            };
            var mockConfigDbSet = configs.BuildMockDbSet();

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            mockDbContext.Setup(x => x.CompanyAddresses).Returns(mockDbSet.Object);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigDbSet.Object);

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
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("ahamove")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
            return new HttpClient(mockHttpMessageHandler.Object);
        }

        private AhaMoveServiceImplementation CreateService(HttpResponseMessage response)
        {
            return new AhaMoveServiceImplementation(CreateHttpClient(response), CreateMockDbContext(), CreateMockConfiguration());
        }

        // ================= HAPPY CASE =================

        // AhaMove trả về HTTP 200 với body hợp lệ => trả về ServiceDetailsResponse
        [Fact]
        public async Task GetServiceDetailsAhaMove_ValidResponse_ReturnsServiceDetails()
        {
            // Arrange
            var serviceDetails = new ServiceDetailsResponse
            {
                Id = "SGN-BIKE",
                Name = "AhaMove Bike",
                NameViVn = "Giao hàng xe máy",
                Enable = true
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(serviceDetails)
            };

            var service = CreateService(httpResponse);

            // Act
            var result = await service.GetServiceDetailsAhaMove();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SGN-BIKE", result.Id);
            Assert.Equal("AhaMove Bike", result.Name);
            Assert.True(result.Enable);
        }

        // ================= EDGE CASE 1: HTTP status lỗi =================

        // AhaMove trả về status không thành công => ném BadRequestException "Ahamove API HTTP Error"
        [Fact]
        public async Task GetServiceDetailsAhaMove_HttpErrorStatus_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("unauthorized", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetServiceDetailsAhaMove();

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("Ahamove API HTTP Error", exception.Message);
        }

        // ================= EDGE CASE 2: Body deserialize ra null =================

        // AhaMove trả về HTTP 200 nhưng body là "null" => ném BadRequestException "returned null or empty data"
        [Fact]
        public async Task GetServiceDetailsAhaMove_NullBody_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetServiceDetailsAhaMove();

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Ahamove returned null or empty data", exception.Message);
        }

        // ================= EDGE CASE 3: Outgoing request is built correctly =================
        // Verifies the endpoint, HTTP method (GET), and the Ahamove Authorization token —
        // the request-building the service exists to do.
        [Fact]
        public async Task GetServiceDetailsAhaMove_BuildsAuthorizedGetRequestToConfiguredEndpoint()
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
                    Content = JsonContent.Create(new ServiceDetailsResponse { Id = "SGN-BIKE" })
                });

            var service = new AhaMoveServiceImplementation(
                new HttpClient(mockHandler.Object),
                CreateMockDbContext(),
                CreateMockConfiguration());

            // Act
            await service.GetServiceDetailsAhaMove();

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(HttpMethod.Get, captured!.Method);
            Assert.Equal("http://ahamove.test/v3/services/SGN-BIKE", captured.RequestUri!.ToString());
            Assert.True(captured.Headers.Contains("Authorization"));
            Assert.Equal("test_api_key", captured.Headers.GetValues("Authorization").First());
        }
    }
}
