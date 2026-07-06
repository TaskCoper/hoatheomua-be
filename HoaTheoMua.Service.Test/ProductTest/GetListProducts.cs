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
using Xunit;
using ProductServiceImpl = HoaTheoMua.Service.Product.Service;
using ProductRequest = HoaTheoMua.Service.Product.Request;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.Product
{
    // Unit test cho HoaTheoMua.Service.Product.Service.GetListProducts.
    // Viết theo đúng format của AhaMoveService/EstimateShippingFee.cs (Arrange - Act - Assert,
    // mock AppDbContext bằng MockQueryable.Moq, mock HttpMessageHandler bằng Moq.Protected).
    public class GetListProductsTests
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
                mockConfig.Object);
        }

        private static HttpResponseMessage JsonResponse(string rawJson, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(rawJson, Encoding.UTF8, "application/json")
            };
        }

        // Mock HttpMessageHandler; captureRequest để assert URL/header, captureBody để assert nội dung gửi lên.
        private static Mock<HttpMessageHandler> CreateHttpHandler(
            HttpResponseMessage response,
            Action<HttpRequestMessage>? captureRequest = null,
            Action<string>? captureBody = null)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    captureRequest?.Invoke(req);
                    if (captureBody != null && req.Content != null)
                        captureBody(req.Content.ReadAsStringAsync().Result);
                })
                .ReturnsAsync(response);
            return handler;
        }

        private static ProductRequest.ProductListRequest CreateRequest()
        {
            return new ProductRequest.ProductListRequest
            {
                Filters = new ProductRequest.ProductFilters { Status = new List<int> { 1 } },
                Paginator = new ProductRequest.ProductPaginator { Size = 50 }
            };
        }
        
        // TEST CASE 01: Lấy danh sách sản phẩm thành công
        [Fact]
        public async Task GetListProducts_ValidRequest_ReturnsProductResponse()
        {
            // Arrange
            var rawJson = "{\"code\":1,\"data\":[{\"id\":4934210,\"name\":\"Bo Coral Pink 2026\"}]}";
            var handler = CreateHttpHandler(JsonResponse(rawJson));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetListProducts(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.Single(result.Data);
            Assert.Equal(4934210, result.Data[0].Id);
        }
        
        // TEST CASE 02: Gửi request có field null -> field null không được serialize gửi lên
        [Fact]
        public async Task GetListProducts_NullFields_AreNotSerialized()
        {
            // Arrange: Filters = null, Paginator chỉ có Size (Sort/Next = null).
            string requestBody = string.Empty;
            var handler = CreateHttpHandler(
                JsonResponse("{\"code\":1,\"data\":[]}"),
                captureBody: body => requestBody = body);
            var service = CreateService(new HttpClient(handler.Object));

            var request = new ProductRequest.ProductListRequest
            {
                Filters = null,
                Paginator = new ProductRequest.ProductPaginator { Size = 10 }
            };

            // Act
            await service.GetListProducts(request);

            // Assert: field null (filters, sort, next) không xuất hiện trong body.
            Assert.DoesNotContain("filters", requestBody);
            Assert.DoesNotContain("sort", requestBody);
            Assert.DoesNotContain("next", requestBody);
            Assert.Contains("size", requestBody);
        }
        
        // TEST CASE 03: Tạo đúng URL gọi NhanhVn (/product/list?appId=...&businessId=...)
        [Fact]
        public async Task GetListProducts_BuildsCorrectNhanhVnUrl()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = CreateHttpHandler(
                JsonResponse("{\"code\":1,\"data\":[]}"),
                captureRequest: req => capturedRequest = req);
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            await service.GetListProducts(CreateRequest());

            // Assert
            Assert.NotNull(capturedRequest);
            var url = capturedRequest!.RequestUri!.ToString();
            Assert.Contains("/product/list", url);
            Assert.Contains("appId=12345", url);
            Assert.Contains("businessId=67890", url);
        }
        
        // TEST CASE 04: Gửi request có Authorization token
        [Fact]
        public async Task GetListProducts_SendsAuthorizationHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = CreateHttpHandler(
                JsonResponse("{\"code\":1,\"data\":[]}"),
                captureRequest: req => capturedRequest = req);
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            await service.GetListProducts(CreateRequest());

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest!.Headers.Contains("Authorization"));
            Assert.Equal("test_token", capturedRequest.Headers.GetValues("Authorization").First());
        }
        
        // TEST CASE 05: NhanhVn API trả về lỗi HTTP -> BadRequestException
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetListProducts_HttpError_ThrowsBadRequestException(HttpStatusCode statusCode)
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{}", statusCode));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetListProducts(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }
        
        // TEST CASE 06: NhanhVn response null (body "null") -> BadRequestException
        [Fact]
        public async Task GetListProducts_NullResponse_ThrowsBadRequestException()
        {
            // Arrange: HTTP 200 nhưng body là "null" -> deserialize trả về null.
            var handler = CreateHttpHandler(JsonResponse("null"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetListProducts(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }
        
        // TEST CASE 07: NhanhVn trả về Code = 0 -> BadRequestException
        [Fact]
        public async Task GetListProducts_CodeZero_ThrowsBadRequestException()
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{\"code\":0,\"data\":[]}"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetListProducts(CreateRequest());

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("ERR_INVALID_FORM_FIELDS", exception.Message);
        }
        
        // TEST CASE 08: Data rỗng nhưng Code != 0 -> vẫn trả về response (theo code hiện tại)
        [Fact]
        public async Task GetListProducts_EmptyDataButCodeNotZero_ReturnsResponse()
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{\"code\":1,\"data\":[]}"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var result = await service.GetListProducts(CreateRequest());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Code);
            Assert.Empty(result.Data);
        }
        
        // TEST CASE 09: Response JSON sai format -> ném lỗi deserialize
        [Fact]
        public async Task GetListProducts_MalformedJson_ThrowsJsonException()
        {
            // Arrange: body không phải JSON hợp lệ.
            var handler = CreateHttpHandler(JsonResponse("{ this is not valid json"));
            var service = CreateService(new HttpClient(handler.Object));

            // Act
            var act = async () => await service.GetListProducts(CreateRequest());

            // Assert
            await Assert.ThrowsAsync<System.Text.Json.JsonException>(act);
        }
        
        // TEST CASE 12: Log raw request và raw response bằng Console.WriteLine
        [Fact]
        public async Task GetListProducts_Success_WritesDebugLog()
        {
            // Arrange
            var handler = CreateHttpHandler(JsonResponse("{\"code\":1,\"data\":[]}"));
            var service = CreateService(new HttpClient(handler.Object));

            var originalOut = Console.Out;
            var sw = new StringWriter();
            Console.SetOut(sw);

            try
            {
                // Act
                await service.GetListProducts(CreateRequest());
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            // Assert
            Assert.Contains("[DEBUG NHANH.VN RAW DATA]", sw.ToString());
        }
    }
}
