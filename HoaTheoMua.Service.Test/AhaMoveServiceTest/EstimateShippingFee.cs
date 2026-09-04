using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    public class AhaMoveServiceTests
    {
        private static readonly Guid DefaultProductId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        private CompanyAddressEntity CreateAddress(bool isDeleted = false)
        {
            return new CompanyAddressEntity
            {
                Id = Guid.NewGuid(),
                AddressLine = "123 Test",
                Ward = "Ward 1",
                City = "Ho Chi Minh",
                PhoneNumber = "0987654321",
                FullName = "Shop Test",
                Remarks = "Ghi chu",
                Country = "Vietnam",
                Latitude = 10.762622m,
                Longitude = 106.660172m,
                IsDeleted = isDeleted,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private AppDbContext CreateMockDbContext(
            List<CompanyAddressEntity> companyAddresses,
            List<HoaTheoMua.Repository.Entity.Product>? products = null)
        {
            var mockDbSet = companyAddresses.BuildMockDbSet();
            var mockProducts = (products ?? new List<HoaTheoMua.Repository.Entity.Product>
            {
                new()
                {
                    Id = DefaultProductId,
                    NhanhProductId = "1",
                    ProductName = "Sản phẩm A",
                    Price = 100000m,
                    Height = 10m,
                    Weight = 1m,
                    Length = 10m,
                    Width = 10m,
                    IsActive = true,
                    IsDeleted = false
                }
            }).BuildMockDbSet();
            
            var configs = new List<HoaTheoMua.Repository.Entity.Config>
            {
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false },
                new HoaTheoMua.Repository.Entity.Config { Key = "ahamove.base_url", Value = "{\"value\": \"http://ahamove.test\"}", Group = "ahamove", IsDeleted = false }
            };
            var mockConfigDbSet = configs.BuildMockDbSet();

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            mockDbContext
                .Setup(x => x.CompanyAddresses)
                .Returns(mockDbSet.Object);

            mockDbContext
                .Setup(x => x.Configs)
                .Returns(mockConfigDbSet.Object);

            mockDbContext
                .Setup(x => x.Products)
                .Returns(mockProducts.Object);

            return mockDbContext.Object;
        }
        
        // EDGE CASE 1: Company không tồn tại trong db
        [Fact]
        public async Task EstimateShippingFee_CompanyAddressNotFound_ThrowsException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<CompanyAddressEntity>());

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var service = new AhaMoveServiceImplementation(httpClient, dbContext, mockConfig.Object);

            var request = new AhaMoveRequest.EstimateShippingFeeRequest();

            // Act
            var act = async () => await service.EstimateShippingFee(request);

            // async Task Estimat()
            // {
            //     await service.EstimateShippingFee(request);
            // }
            
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(act);
            Assert.Equal("Company address not found", exception.Message);
        }
        
        // EDGE CASE 2: Response của nhanhvn trả về code == 0
        [Fact]
        public async Task EstimateShippingFee_ProductMissingFromDb_ThrowsBadRequestException()
        {
            // Arrange
            var companyAddress = CreateAddress();
            var dbContext = CreateMockDbContext(
                new List<CompanyAddressEntity> { companyAddress },
                new List<HoaTheoMua.Repository.Entity.Product>());

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var nhanhVnResponseData = new NhanhProductListResponse
            {
                Code = 0,
                Data = new List<NhanhProductData>()
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(nhanhVnResponseData)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new AhaMoveServiceImplementation(httpClient, dbContext, mockConfig.Object);

            var request = new AhaMoveRequest.EstimateShippingFeeRequest
            {
                Items = new List<AhaMoveRequest.Items>
                {
                    new AhaMoveRequest.Items { ProductId = Guid.NewGuid(), Number = 1 }
                }
            };

            // Act
            var act = async () => await service.EstimateShippingFee(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Có sản phẩm không tồn tại hoặc đã ngừng hoạt động", exception.Message);
        }
        
        // EDGE CASE 3: ProductId truyền vô không tồn tại dẫn đến lỗi bị crash
        [Fact]
            public async Task EstimateShippingFee_ProductIdNotFoundInDb_ThrowsBadRequestException()
            {
                // Arrange
                var companyAddress = CreateAddress();
                var dbContext = CreateMockDbContext(
                    new List<CompanyAddressEntity> { companyAddress },
                    new List<HoaTheoMua.Repository.Entity.Product>());

                var mockConfig = new Mock<IConfiguration>();
                var mockConfigSection = new Mock<IConfigurationSection>();
                mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
                mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

                var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

                var nhanhProductData = new NhanhProductData
                {
                    Id = 999, 
                    Name = "Sản phẩm A",
                    Prices = new NhanhPrices { Retail = 100000 },
                    Shipping = new NhanhShipping { Height = 10, Weight = 1, Length = 10, Width = 10 }
                };

                var nhanhVnResponseData = new NhanhProductListResponse
                {
                    Code = 1,
                    Data = new List<NhanhProductData> { nhanhProductData }
                };

                mockHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("nhanhvn")),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(nhanhVnResponseData)
                    });

                var httpClient = new HttpClient(mockHttpMessageHandler.Object);
                var service = new AhaMoveServiceImplementation(httpClient, dbContext, mockConfig.Object);

                var request = new AhaMoveRequest.EstimateShippingFeeRequest
                {
                    Items = new List<AhaMoveRequest.Items>
                    {
                        new AhaMoveRequest.Items { ProductId = Guid.NewGuid(), Number = 2 }
                    }
                };

                // Act
                var act = async () => await service.EstimateShippingFee(request);

                // Assert
                var exception = await Assert.ThrowsAsync<BadRequestException>(act);
                Assert.Equal("Có sản phẩm không tồn tại hoặc đã ngừng hoạt động", exception.Message);
            }
        
        // EDGE CASE 4: Kiểm tra logic OrderTime và IdleUntil khi gọi AhaMove
        [Fact]
        public async Task EstimateShippingFee_OrderTimeGreaterThanZero_MapsIdleUntilCorrectly()
        {
            // Arrange
            var companyAddress = CreateAddress();
            var dbContext = CreateMockDbContext(new List<CompanyAddressEntity> { companyAddress });

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var nhanhProductData = new NhanhProductData
            {
                Id = 1,
                Name = "Sản phẩm A",
                Prices = new NhanhPrices { Retail = 100000 },
                Shipping = new NhanhShipping { Height = 10, Weight = 1, Length = 10, Width = 10 }
            };

            var nhanhVnResponseData = new NhanhProductListResponse
            {
                Code = 1,
                Data = new List<NhanhProductData> { nhanhProductData }
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(nhanhVnResponseData)
                });

            string capturedAhamoveRequestBody = string.Empty;

            var ahamoveResponseData = new List<ServiceResponse>
            {
                new ServiceResponse { ServiceId = "SGN-BIKE" }
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ahamove")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    capturedAhamoveRequestBody = req.Content.ReadAsStringAsync().Result;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(ahamoveResponseData)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new AhaMoveServiceImplementation(httpClient, dbContext, mockConfig.Object);

            var request = new AhaMoveRequest.EstimateShippingFeeRequest
            {
                OrderTime = 1680000000, 
                Items = new List<AhaMoveRequest.Items>
                {
                    new AhaMoveRequest.Items { ProductId = DefaultProductId, Number = 2 }
                }
            };

            // Act
            await service.EstimateShippingFee(request);

            // Assert
            var ahamoveRequestObj = JsonSerializer.Deserialize<AhaMoveRequest.AhaMoveShippingFeeRequest>(capturedAhamoveRequestBody);
            
            Assert.NotNull(ahamoveRequestObj);
            Assert.Equal(1680000000, ahamoveRequestObj.OrderTime);
            Assert.Equal(1680000000, ahamoveRequestObj.IdleUntil);
        }
        
        // EDGE CASE 5: AhaMove response trả về null hoặc rỗng
        [Fact]
        public async Task EstimateShippingFee_AhamoveReturnsEmptyList_ThrowsBadRequestException()
        {
            // Arrange
            var companyAddress = CreateAddress();
            var dbContext = CreateMockDbContext(new List<CompanyAddressEntity> { companyAddress });

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var nhanhProductData = new NhanhProductData
            {
                Id = 1,
                Name = "Sản phẩm A",
                Prices = new NhanhPrices { Retail = 100000 },
                Shipping = new NhanhShipping { Height = 10, Weight = 1, Length = 10, Width = 10 }
            };

            var nhanhVnResponseData = new NhanhProductListResponse
            {
                Code = 1,
                Data = new List<NhanhProductData> { nhanhProductData }
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(nhanhVnResponseData)
                });

            var emptyAhamoveResponseData = new List<ServiceResponse>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ahamove")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(emptyAhamoveResponseData)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new AhaMoveServiceImplementation(httpClient, dbContext, mockConfig.Object);

            var request = new AhaMoveRequest.EstimateShippingFeeRequest
            {
                Items = new List<AhaMoveRequest.Items>
                {
                    new AhaMoveRequest.Items { ProductId = DefaultProductId, Number = 2 }
                }
            };

            // Act
            var act = async () => await service.EstimateShippingFee(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Ahamove returned null or empty data", exception.Message);
        }
        
        // HAPPY CASE: Chạy thành công
        [Fact]
        public async Task EstimateShippingFee_ValidRequest_ReturnsSuccessObject()
        {
            // Arrange
            var companyAddress = CreateAddress();
            var dbContext = CreateMockDbContext(new List<CompanyAddressEntity> { companyAddress });

            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var nhanhProductData = new NhanhProductData
            {
                Id = 1,
                Name = "Sản phẩm A",
                Prices = new NhanhPrices { Retail = 100000 },
                Shipping = new NhanhShipping { Height = 10, Weight = 1, Length = 10, Width = 10 }
            };

            var nhanhVnResponseData = new NhanhProductListResponse
            {
                Code = 1,
                Data = new List<NhanhProductData> { nhanhProductData }
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(nhanhVnResponseData)
                });

            var ahamoveResponseData = new List<ServiceResponse>
            {
                new ServiceResponse { ServiceId = "SGN-BIKE" }
            };

            // Capture the outgoing Ahamove request body (read inside the callback, before
            // HttpClient disposes the request) so the field mapping can be asserted.
            string? ahamoveRequestBody = null;
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ahamove")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                    ahamoveRequestBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(ahamoveResponseData)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new AhaMoveServiceImplementation(httpClient, dbContext, mockConfig.Object);

            var request = new AhaMoveRequest.EstimateShippingFeeRequest
            {
                Items = new List<AhaMoveRequest.Items>
                {
                    new AhaMoveRequest.Items { ProductId = DefaultProductId, Number = 2 }
                }
            };

            // Act
            var result = await service.EstimateShippingFee(request);

            // Assert: the method returns both upstream results...
            Assert.NotNull(result);
            var ahamoveResult = result.GetType().GetProperty("ahamoveResult")!.GetValue(result) as List<ServiceResponse>;
            var products = result.GetType().GetProperty("products")!.GetValue(result);
            Assert.NotNull(ahamoveResult);
            Assert.Single(ahamoveResult!);
            Assert.Equal("SGN-BIKE", ahamoveResult![0].ServiceId);
            Assert.NotNull(products);

            // ...and the outgoing Ahamove request maps the NhanhVn product data correctly:
            // _id from NhanhProductId, price from Product.Price,
            // num from the requested quantity, and PaymentMethod "CASH".
            Assert.NotNull(ahamoveRequestBody);
            using var doc = JsonDocument.Parse(ahamoveRequestBody!);
            var root = doc.RootElement;
            Assert.Equal("CASH", root.GetProperty("payment_method").GetString());
            var items = root.GetProperty("items");
            Assert.Equal(1, items.GetArrayLength());
            Assert.Equal("1", items[0].GetProperty("_id").GetString());
            Assert.Equal(100000, items[0].GetProperty("price").GetInt32());
            Assert.Equal(2, items[0].GetProperty("num").GetInt32());
        }
    }
}
