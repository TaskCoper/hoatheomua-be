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
    public class GetDepotDetails
    {
        private AppDbContext CreateMockDbContext()
        {
            var companyAddresses = new List<CompanyAddressEntity>();
            var mockDbSet = companyAddresses.BuildMockDbSet();

            var configs = new List<HoaTheoMua.Repository.Entity.Config>
            {
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false }
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
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
            return new HttpClient(mockHttpMessageHandler.Object);
        }

        private AhaMoveServiceImplementation CreateService(HttpResponseMessage response)
        {
            return new AhaMoveServiceImplementation(CreateHttpClient(response), CreateMockDbContext(), CreateMockConfiguration());
        }

        private NhanhVnRequest.DepotRequest CreateRequest()
        {
            return new NhanhVnRequest.DepotRequest
            {
                Filters = new NhanhVnRequest.DepotFilters { Ids = new List<int> { 1 } }
            };
        }
        
        // Nhanh.vn trả về HTTP 200, Code != 0 và có data => trả về DepotResponse hợp lệ
        [Fact]
        public async Task GetDepotDetails_ValidResponse_ReturnsDepotResponse()
        {
            // Arrange
            var depotResponse = new DepotResponse
            {
                Code = 1,
                Data = new List<DepotData>
                {
                    new DepotData { Id = 100, Code = "DEPOT_1", Name = "Kho Test" }
                }
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(depotResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var result = await service.GetDepotDetails(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.Single(result.Data);
            Assert.Equal("DEPOT_1", result.Data[0].Code);
        }

        // ================= EDGE CASE 1: HTTP status lỗi =================

        // Nhanh.vn trả về status không thành công => ném BadRequestException "NhanhVn API HTTP Error"
        [Fact]
        public async Task GetDepotDetails_HttpErrorStatus_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("error", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetDepotDetails(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }

        // ================= EDGE CASE 2: Body deserialize ra null =================

        // Nhanh.vn trả về HTTP 200 nhưng body là "null" => ném BadRequestException "returned null or empty data"
        [Fact]
        public async Task GetDepotDetails_NullBody_ThrowsBadRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetDepotDetails(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }

        // ================= EDGE CASE 3: Code == 0 =================

        // Nhanh.vn trả về HTTP 200 nhưng Code == 0 => ném BadRequestException "ERR_INVALID_FORM_FIELDS"
        [Fact]
        public async Task GetDepotDetails_ResponseCodeZero_ThrowsBadRequestException()
        {
            // Arrange
            var depotResponse = new DepotResponse
            {
                Code = 0,
                Data = new List<DepotData>()
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(depotResponse)
            };

            var service = CreateService(httpResponse);

            // Act
            var act = async () => await service.GetDepotDetails(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }
    }
}
