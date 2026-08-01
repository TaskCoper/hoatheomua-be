using System.Net;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using HoaTheoMua.Service.Test.TestSupport;
using Xunit;
using ProductServiceImpl = HoaTheoMua.Service.Product.Service;
using ProductRequest = HoaTheoMua.Service.Product.Request;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.ProductTest
{
    // Unit test cho HoaTheoMua.Service.Product.Service.GetProductDetails.
    // Viết theo đúng format của AhaMoveService/EstimateShippingFee.cs (Arrange - Act - Assert,
    // mock AppDbContext bằng MockQueryable.Moq, mock HttpMessageHandler bằng Moq.Protected).
    public class GetProductDetailsTests
    {
        // Helpers

        // Mock AppDbContext chỉ cần DbSet Configs (ConfigHelper đọc group "nhanhvn").
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

        private static ProductServiceImpl CreateService(HttpClient httpClient)
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockHttpContext = new Mock<IHttpContextAccessor>();

            return new ProductServiceImpl(
                CreateMockDbContext(),
                mockHttpContext.Object,
                httpClient,
                mockConfig.Object,
                MapperFactory.Create());
        }

        // Tạo HttpResponseMessage với body JSON thô.
        private static HttpResponseMessage JsonResponse(string rawJson, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(rawJson, Encoding.UTF8, "application/json")
            };
        }

        // Mock HttpMessageHandler trả về response cố định; capture request để assert URL/header.
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

        private static ProductRequest.ProductDetailRequest CreateRequest(string id = "4934210")
        {
            return new ProductRequest.ProductDetailRequest
            {
                Filters = new ProductRequest.ProductDetailFilters { Id = id }
            };
        }
        
        // TEST CASE 01: Lấy chi tiết sản phẩm thành công
        [Fact]
        public async Task GetProductDetails_ValidRequest_ReturnsProductDetailResponse()
        {
            // Arrange
            var rawJson = "{\"code\":1,\"data\":{\"id\":4934210,\"name\":\"Bo Coral Pink 2026\"}}";
            var handler = CreateHttpHandler(JsonResponse(rawJson));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetProductDetails(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.NotNull(result.Data);
            Assert.Equal(4934210, result.Data!.Id);
        }
        
        // TEST CASE 02: Tạo đúng URL gọi NhanhVn (/product/detail?appId=...&businessId=...)
        [Fact]
        public async Task GetProductDetails_BuildsCorrectNhanhVnUrl()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = CreateHttpHandler(
                JsonResponse("{\"code\":1,\"data\":{\"id\":1}}"),
                req => capturedRequest = req);
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            await service.GetProductDetails(CreateRequest());

            // Assert
            Assert.NotNull(capturedRequest);
            var url = capturedRequest!.RequestUri!.ToString();
            Assert.Contains("/product/detail", url);
            Assert.Contains("appId=12345", url);
            Assert.Contains("businessId=67890", url);
        }
        
        // TEST CASE 03: Gửi request có Authorization token
        [Fact]
        public async Task GetProductDetails_SendsAuthorizationHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = CreateHttpHandler(
                JsonResponse("{\"code\":1,\"data\":{\"id\":1}}"),
                req => capturedRequest = req);
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            await service.GetProductDetails(CreateRequest());

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest!.Headers.Contains("Authorization"));
            Assert.Equal("test_token", capturedRequest.Headers.GetValues("Authorization").First());
        }
        
        // TEST CASE 04: NhanhVn API trả về lỗi HTTP -> BadRequestException
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetProductDetails_HttpError_ThrowsBadRequestException(HttpStatusCode statusCode)
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{}", statusCode));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetProductDetails(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }
        
        // TEST CASE 05: NhanhVn response null (body "null") -> BadRequestException
        [Fact]
        public async Task GetProductDetails_NullResponse_ThrowsBadRequestException()
        {
            // Arrange: HTTP 200 nhưng body là "null" -> Deserialize trả về null.
            var handler = CreateHttpHandler(JsonResponse("null"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetProductDetails(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }
        
        // TEST CASE 06: NhanhVn trả về Code = 0 -> BadRequestException
        [Fact]
        public async Task GetProductDetails_CodeZero_ThrowsBadRequestException()
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{\"code\":0,\"data\":null}"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetProductDetails(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }
        
        // TEST CASE 07: JSON có tên property khác kiểu chữ (PropertyNameCaseInsensitive = true)
        [Fact]
        public async Task GetProductDetails_PropertyNameCaseInsensitive_DeserializesSuccessfully()
        {
            // Arrange: "CODE"/"DATA"/"ID" viết hoa khác model.
            var rawJson = "{\"CODE\":1,\"DATA\":{\"ID\":4934210,\"NAME\":\"Test\"}}";
            var handler = CreateHttpHandler(JsonResponse(rawJson));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetProductDetails(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.NotNull(result.Data);
            Assert.Equal(4934210, result.Data!.Id);
        }
        
        // TEST CASE 08: JSON có số trả về dạng string (NumberHandling.AllowReadingFromString)
        [Fact]
        public async Task GetProductDetails_NumberAsString_ParsesSuccessfully()
        {
            // Arrange: id trả về dạng string "4934210".
            var rawJson = "{\"code\":1,\"data\":{\"id\":\"4934210\",\"name\":\"Test\"}}";
            var handler = CreateHttpHandler(JsonResponse(rawJson));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetProductDetails(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(4934210, result.Data!.Id);
        }
        
        // TEST CASE 09: Response JSON sai format -> ném lỗi deserialize
        [Fact]
        public async Task GetProductDetails_MalformedJson_ThrowsJsonException()
        {
            // Arrange: body không phải JSON hợp lệ.
            var handler = CreateHttpHandler(JsonResponse("{ this is not valid json"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetProductDetails(CreateRequest());

            // Assert
            await Assert.ThrowsAsync<System.Text.Json.JsonException>(act);
        }
        
        // TEST CASE 11: Product ID không tồn tại - NhanhVn trả Code = 0 -> BadRequestException
        [Fact]
        public async Task GetProductDetails_ProductIdNotFound_ThrowsBadRequestException()
        {
            // Arrange: id không tồn tại, NhanhVn phản hồi Code = 0.
            var handler = CreateHttpHandler(JsonResponse("{\"code\":0,\"data\":null}"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetProductDetails(CreateRequest("999999999"));

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }
    }
}
