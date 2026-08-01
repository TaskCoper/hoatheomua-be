using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhamoveService;
using AhaMoveRequest = HoaTheoMua.Service.AhamoveService.AhaMoveRequest;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using OrderServiceImpl = HoaTheoMua.Service.Order.Service;
using OrderRequest = HoaTheoMua.Service.Order.Request;
using ProductService = HoaTheoMua.Service.Product.IService;
using ProductRequestDto = HoaTheoMua.Service.Product.Request;
using ProductDetailResponse = HoaTheoMua.Service.Product.NhanhVnProductDetailsResponse;
using NhanhVnProductListResponse = HoaTheoMua.Service.Product.NhanhVnProductListResponse;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using OrderDetailEntity = HoaTheoMua.Repository.Entity.OrderDetails;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using TransactionEntity = HoaTheoMua.Repository.Entity.Transaction;

namespace HoaTheoMua.Service.Test.OrderTest
{
    // Unit test cho HoaTheoMua.Service.Order.Service.CreateOrder.
    // Viết theo đúng format của AhaMoveService/EstimateShippingFee.cs (Arrange - Act - Assert,
    // mock AppDbContext bằng MockQueryable.Moq, mock HttpMessageHandler bằng Moq.Protected).
    public class CreateOrderTests
    {
        private static readonly Guid TestUserId = Guid.Parse("cba7c21c-d6f2-4b30-9b4e-8c7c10b4f0b2");

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static OrderDetailEntity CreateOrderDetail(
            string name = "San pham A", int itemId = 101, int number = 2, decimal price = 50000m)
        {
            return new OrderDetailEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                ItemId = itemId,
                Number = number,
                Price = price,
                Height = 10,
                Weight = 1,
                Length = 10,
                Width = 10,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static OrderEntity CreateOrderEntity(
            Guid? id = null,
            string status = "Pending",
            decimal totalFee = 100000m,
            long orderTime = 0,
            List<OrderDetailEntity>? details = null,
            Guid? userId = null)
        {
            return new OrderEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? TestUserId,
                Ward = 1,
                City = 1,
                Address = "123 Duong Test",
                ShortAddress = "123 Duong Test, Q1, HCM",
                FullName = "Nguyen Van A",
                Phone = "0987654321",
                Remarks = "Giao gio hanh chinh",
                Status = status,
                TrackNumber = "TRACK-001",
                OrderTime = orderTime,
                TotalFee = totalFee,
                ProductFee = totalFee,
                ShippingFee = 0m,
                Latitude = 10.762622m,
                Longitude = 106.660172m,
                TagIds = new List<long>(),
                IsExportInvoice = false,
                CreatedAt = DateTimeOffset.UtcNow,
                OrderDetails = details ?? new List<OrderDetailEntity> { CreateOrderDetail() }
            };
        }

        private static CompanyAddressEntity CreateCompanyAddress()
        {
            return new CompanyAddressEntity
            {
                Id = Guid.NewGuid(),
                AddressLine = "123 Kho Test",
                Ward = "Ward 1",
                City = "Ho Chi Minh",
                Country = "Vietnam",
                PhoneNumber = "0912345678",
                FullName = "Shop Test",
                Remarks = "Den noi lay hang doc ma don",
                Latitude = 10.762622m,
                Longitude = 106.660172m,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        // Mock AppDbContext với DbSet cần thiết và Configs.
        private static Mock<AppDbContext> CreateMockDbContext(
            List<OrderEntity>? orders = null,
            List<CompanyAddressEntity>? companyAddresses = null,
            List<OrderDetailEntity>? capturedOrderDetails = null,
            double? freeShipDistance = null)
        {
            var mockOrders = (orders ?? new List<OrderEntity>()).BuildMockDbSet();
            var orderDetailsList = capturedOrderDetails ?? new List<OrderDetailEntity>();
            var mockOrderDetails = orderDetailsList.BuildMockDbSet();
            var mockCompany = (companyAddresses ?? new List<CompanyAddressEntity>()).BuildMockDbSet();

            mockOrderDetails.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<OrderDetailEntity>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<OrderDetailEntity>, CancellationToken>((items, ct) => orderDetailsList.AddRange(items))
                .Returns(Task.CompletedTask);

            mockOrders.Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                .Callback<OrderEntity, CancellationToken>((item, ct) => (orders ?? new List<OrderEntity>()).Add(item))
                .ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<OrderEntity>)null!);

            var configs = new List<ConfigEntity>
            {
                new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.default_depot", Value = "{\"value\": 129255}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false },
                new ConfigEntity { Key = "ahamove.base_url", Value = "{\"value\": \"http://ahamove.test\"}", Group = "ahamove", IsDeleted = false }
            };
            if (freeShipDistance.HasValue)
            {
                configs.Add(new ConfigEntity
                {
                    Key = "ahamove.free_ship",
                    Value = "{\"value\": " + freeShipDistance.Value + "}",
                    Group = "ahamove",
                    IsDeleted = false
                });
            }
            var mockConfigs = configs.BuildMockDbSet();

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            mockDbContext.Setup(x => x.Orders).Returns(mockOrders.Object);
            mockDbContext.Setup(x => x.OrderDetails).Returns(mockOrderDetails.Object);
            mockDbContext.Setup(x => x.CompanyAddresses).Returns(mockCompany.Object);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);
            mockDbContext
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            return mockDbContext;
        }

        private static OrderServiceImpl CreateService(AppDbContext dbContext, HttpClient httpClient, ICurrentUserService currentUser, ProductService productService, HoaTheoMua.Service.BaoKim.IService? baoKimService = null)
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            return new OrderServiceImpl(
                dbContext,
                currentUser,
                httpClient,
                productService,
                mockConfig.Object,
                baoKimService ?? CreateMockBaoKimService());
        }

        // BaoKim is an injected dependency (not the shared HttpClient), so we mock it directly.
        // Both calls return the success code (100) with a payment URL so CreateOrder completes.
        private static HoaTheoMua.Service.BaoKim.IService CreateMockBaoKimService(
            string paymentUrl = "https://pay.baokim.vn/checkout/abc",
            string mrcOrderId = "MRC-001")
        {
            var mock = new Mock<HoaTheoMua.Service.BaoKim.IService>();

            mock.Setup(x => x.CreateAccessToken())
                .ReturnsAsync(new HoaTheoMua.Service.BaoKim.BaoKimResponse<HoaTheoMua.Service.BaoKim.TokenResponse>
                {
                    Code = 100,
                    Message = "OK",
                    Data = new HoaTheoMua.Service.BaoKim.TokenResponse
                    {
                        AccessToken = "test_access_token",
                        TokenType = "Bearer"
                    }
                });

            mock.Setup(x => x.CreateOrder(
                    It.IsAny<string>(),
                    It.IsAny<OrderEntity>()))
                .ReturnsAsync(new HoaTheoMua.Service.BaoKim.BaoKimResponse<HoaTheoMua.Service.BaoKim.CreateOrderResponse>
                {
                    Code = 100,
                    Message = "OK",
                    Data = new HoaTheoMua.Service.BaoKim.CreateOrderResponse
                    {
                        OrderId = 999,
                        MrcOrderId = mrcOrderId,
                        PaymentUrl = paymentUrl
                    }
                });

            return mock.Object;
        }

        private static ICurrentUserService CreateMockCurrentUserService(Guid userId, bool isVerified = true)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(x => x.GetRequiredUserId()).Returns(userId);
            mock.Setup(x => x.IsVerified).Returns(isVerified);
            return mock.Object;
        }

        private static ProductService CreateMockProductService(
            List<NhanhVnProductListResponse.ProductDetailData>? listProducts = null,
            ProductDetailResponse.NhanhVnProductDetailResponse? productDetail = null)
        {
            var mock = new Mock<ProductService>();
            mock.Setup(x => x.GetListProducts(It.IsAny<ProductRequestDto.ProductListRequest>()))
                .ReturnsAsync(new NhanhVnProductListResponse.NhanhVnProductResponse
                {
                    Code = 1,
                    Data = listProducts ?? new List<NhanhVnProductListResponse.ProductDetailData>
                    {
                        new NhanhVnProductListResponse.ProductDetailData
                        {
                            Id = 101,
                            Name = "Test Product",
                            Prices = new NhanhVnProductListResponse.ProductPrices { Retail = 50000 },
                            Shipping = new NhanhVnProductListResponse.ProductShipping
                                { Height = 10, Weight = 1, Length = 10, Width = 10 },
                        }
                    }
                });

            mock.Setup(x => x.GetProductDetails(It.IsAny<ProductRequestDto.ProductDetailRequest>()))
                .ReturnsAsync(productDetail ?? new ProductDetailResponse.NhanhVnProductDetailResponse
                {
                    Code = 1,
                    Data = new ProductDetailResponse.ProductDetailData
                    {
                        Id = 101,
                        Name = "Test Product",
                        Prices = new ProductDetailResponse.ProductPrices { Retail = 50000 },
                        Shipping = new ProductDetailResponse.ProductShipping
                            { Height = 10, Weight = 1, Length = 10, Width = 10 },
                        Inventory = new ProductDetailResponse.ProductInventory
                        {
                            Depots = new List<ProductDetailResponse.DepotInventoryDetail>
                            { new ProductDetailResponse.DepotInventoryDetail { Id = 129255, Available = 10 } }
                        }
                    }
                });
            return mock.Object;
        }

        private static HttpResponseMessage AhamoveSuccess(decimal totalFee = 50000m, double distance = 10000)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new List<ServiceResponse>
                {
                    new ServiceResponse
                    {
                        ServiceId = "SGN-BIKE",
                        Data = new ServiceData { TotalFee = totalFee, Distance = distance }
                    }
                })
            };
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

        // ---------------------------------------------------------------------
        // TEST CASE 01: Tạo order thành công
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_ValidRequest_CreatesOrderSuccessfully()
        {
            // Arrange
            var orders = new List<OrderEntity>();
            var details = new List<OrderDetailEntity>();
            var dbContextMock = CreateMockDbContext(orders: orders, companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }, capturedOrderDetails: details);
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService();

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("nhanhvn")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101, Prices = new NhanhPrices { Retail = 50000 }, Shipping = new NhanhShipping() } } }) });

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("ahamove")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(AhamoveSuccess(50000m));

            var httpClient = new HttpClient(handler.Object);
            var service = CreateService(dbContextMock.Object, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Latitude = 10.1m, Longitude = 106.1m, Ward = 1, City = 1,
                Address = "Dia chi", ShortAddress = "Short Dia Chi",
                FullName = "User Test", Phone = "0901234567", Remarks = "Ghi chu",
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var result = await service.CreateOrder(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://pay.baokim.vn/checkout/abc", result.PaymentUrl);
            Assert.Equal("MRC-001", result.MrcOrderId);
            Assert.Single(orders);
            Assert.Equal("Pending", orders[0].Status);
            Assert.Equal(100000m, orders[0].TotalFee); // product 50k + ship 50k
            Assert.Equal(50000m, orders[0].ProductFee);
            Assert.Equal(50000m, orders[0].ShippingFee);
            Assert.Single(details);
            Assert.Equal(101, details[0].ItemId);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 02: Không tìm thấy UserId trong token -> UnauthorizedException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_NoUserIdInToken_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext().Object;
            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(x => x.GetRequiredUserId()).Throws(new UnauthorizedException("User not found"));
            currentUser.Setup(x => x.IsVerified).Returns(true);

            var service = CreateService(dbContext, new HttpClient(), currentUser.Object, CreateMockProductService());
            var request = new OrderRequest.CreateOrderRequest()
            {
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(act);
            Assert.Equal("User not found", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 04: User chưa verify -> UnauthorizedException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_UserNotVerified_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext().Object;
            var currentUser = CreateMockCurrentUserService(TestUserId, isVerified: false);

            var service = CreateService(dbContext, new HttpClient(), currentUser, CreateMockProductService());
            var request = new OrderRequest.CreateOrderRequest()
            {
                Gateway = "BaoKim"
            };;

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(act);
            Assert.Equal("User is not verified", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 05: NhanhVn API trả về lỗi HTTP -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_NhanhVnHttpError_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);

            var mockNhanhHandler = new Mock<HttpMessageHandler>();
            mockNhanhHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            var httpClient = new HttpClient(mockNhanhHandler.Object);
            var productService = CreateMockProductService();

            var service = CreateService(dbContext, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("NhanhVn API HTTP Error", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 06: NhanhVn response null hoặc không có Data -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_NhanhVnNullOrEmptyData_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);

            var mockNhanhHandler = new Mock<HttpMessageHandler>();
            mockNhanhHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create<NhanhProductListResponse?>(null)
                });

            var httpClient = new HttpClient(mockNhanhHandler.Object);
            var productService = CreateMockProductService();

            var service = CreateService(dbContext, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("NhanhVnResponse returned null or empty data", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 07: Sản phẩm không có dữ liệu tồn kho (GetProductDetails trả về Depot rỗng/null)
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_ProductNoDepotInventory_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService(productDetail: new ProductDetailResponse.NhanhVnProductDetailResponse
            {
                Code = 1,
                Data = new ProductDetailResponse.ProductDetailData
                {
                    Id = 101,
                    Inventory = new ProductDetailResponse.ProductInventory
                    { Depots = new List<ProductDetailResponse.DepotInventoryDetail>() }
                }
            });

            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101 } } }) });
            var service = CreateService(dbContext, new HttpClient(handler.Object), currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Sản phẩm Id=101 không có dữ liệu tồn kho", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 08: Sản phẩm không có tồn kho tại depot 129255
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_ProductNoInventoryAtDefaultDepot_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService(productDetail: new ProductDetailResponse.NhanhVnProductDetailResponse
            {
                Code = 1,
                Data = new ProductDetailResponse.ProductDetailData
                {
                    Id = 101,
                    Inventory = new ProductDetailResponse.ProductInventory
                    { Depots = new List<ProductDetailResponse.DepotInventoryDetail> { new ProductDetailResponse.DepotInventoryDetail { Id = 999, Available = 5 } } }
                }
            });

            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101 } } }) });
            var service = CreateService(dbContext, new HttpClient(handler.Object), currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Sản phẩm Id=101 không có dữ liệu tồn kho tại depot 129255", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 09: Số lượng mua vượt quá tồn kho -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_QuantityExceedsAvailable_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService(productDetail: new ProductDetailResponse.NhanhVnProductDetailResponse
            {
                Code = 1,
                Data = new ProductDetailResponse.ProductDetailData
                {
                    Id = 101,
                    Inventory = new ProductDetailResponse.ProductInventory
                    {
                        Depots = new List<ProductDetailResponse.DepotInventoryDetail>
                        { new ProductDetailResponse.DepotInventoryDetail { Id = 129255, Available = 1 } }
                    }
                }
            });

            var handler = CreateHttpHandler(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101 } } }) });
            var service = CreateService(dbContext, new HttpClient(handler.Object), currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 2 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("Không đủ hàng", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 10: Ahamove API trả về lỗi HTTP -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_AhamoveHttpError_ThrowsBadRequestException()
        {
            // Arrange
            var orders = new List<OrderEntity>();
            var dbContext = CreateMockDbContext(orders: orders, companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService();

            var handler = new Mock<HttpMessageHandler>();
            // URL nhanhvn trả về success, URL ahamove trả về error
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("nhanhvn")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101, Prices = new NhanhPrices { Retail = 10 }, Shipping = new NhanhShipping() } } }) });

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("ahamove")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            var httpClient = new HttpClient(handler.Object);
            var service = CreateService(dbContext, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("Ahamove API HTTP Error", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 11: Ahamove response null hoặc rỗng -> BadRequestException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_AhamoveNullOrEmptyData_ThrowsBadRequestException()
        {
            // Arrange
            var orders = new List<OrderEntity>();
            var dbContext = CreateMockDbContext(orders: orders, companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService();

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("nhanhvn")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101, Prices = new NhanhPrices { Retail = 10 }, Shipping = new NhanhShipping() } } }) });

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("ahamove")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create<List<ServiceResponse>?>(null) });

            var httpClient = new HttpClient(handler.Object);
            var service = CreateService(dbContext, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };

            // Act
            var act = async () => await service.CreateOrder(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Ahamove returned null or empty data", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 12: Tính tổng tiền order chính xác (ProductFee + ShippingFee + VatFee)
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateOrder_CalculatesTotalFeesCorrectly()
        {
            // Arrange
            var orders = new List<OrderEntity>();
            var dbContextMock = CreateMockDbContext(orders: orders, companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() });
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService();

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("nhanhvn")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(new NhanhProductListResponse { Code = 1, Data = new List<NhanhProductData> { new NhanhProductData { Id = 101, Prices = new NhanhPrices { Retail = 50000 }, Shipping = new NhanhShipping() } } }) });

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("ahamove")), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(AhamoveSuccess(30000m));

            var httpClient = new HttpClient(handler.Object);
            var service = CreateService(dbContextMock.Object, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Latitude = 10.1m, Longitude = 106.1m, Ward = 1, City = 1,
                Address = "Dia chi", ShortAddress = "Short Dia Chi",
                FullName = "User Test", Phone = "0901234567", Remarks = "Ghi chu",
                TaxCode = "123456789", // Để tính VatFee
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 2 } },
                Gateway = "BaoKim"
            };

            // Act
            await service.CreateOrder(request);

            // Assert
            var createdOrder = Assert.Single(orders);
            Assert.Equal(100000m, createdOrder.ProductFee);
            Assert.Equal(30000m, createdOrder.ShippingFee);
            Assert.Equal(100000m * 0.08m, createdOrder.Vat);
            Assert.Equal(100000m + 30000m + (100000m * 0.08m), createdOrder.TotalFee);
        }
        
        //Test case 13: Tạo đơn order được free shipping
        [Fact]
        public async Task CreateOrder_DistanceWithinFreeShipConfig_ShippingFreeIsZero()
        {
            //Setup
            //List rỗng đóng vai trò như bảng Orders.
            var orders = new List<OrderEntity>();
            
            var dbContextMock = CreateMockDbContext(
                orders: orders,
                companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() },
                freeShipDistance: 5000
                );
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService();
            
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("nhanhvn")), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                { 
                    StatusCode = HttpStatusCode.OK, 
                    Content = JsonContent.Create(new NhanhProductListResponse 
                    {
                        Code = 1, 
                        Data = new List<NhanhProductData>
                        { 
                            new NhanhProductData
                            {
                                Id = 101, 
                                Prices =  new NhanhPrices { Retail = 50000 }, 
                                Shipping =  new NhanhShipping() ,
                            } 
                        } 
                    }) 
                });
            
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("ahamove")), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(AhamoveSuccess(30000m, distance: 4000));
            
            var httpClient = new HttpClient(handler.Object);
            var service = CreateService(dbContextMock.Object, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest()
            {
                Latitude = 10.1m, Longitude = 106.1m, Ward = 1, City = 1,
                Address = "Dia chi", ShortAddress = "Short Dia Chi",
                FullName = "User Test", Phone = "0901234567", Remarks = "Ghi chu",
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };
            
            //
            await service.CreateOrder(request);
            
            //
            var createdOrder = Assert.Single(orders);
            Assert.Equal(0m, createdOrder.ShippingFee);
            Assert.Equal(50000m, createdOrder.TotalFee);
        }
        
        //Test case 14: Tạo đơn order ngoài bán kính thì vẫn tính phí bình thường
        [Fact]
        public async Task CreateOrder_DistanceOutOfFreeShipConfig_HaveShippingFee()
        {
            var orders = new List<OrderEntity>();
            var dbContextMock = CreateMockDbContext(
                orders: orders,
                companyAddresses: new List<CompanyAddressEntity> { CreateCompanyAddress() },
                freeShipDistance: 5000);
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var productService = CreateMockProductService();
            
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content =  JsonContent.Create(new NhanhProductListResponse
                    {
                        Code = 1,
                        Data = new List<NhanhProductData>
                        {
                            new NhanhProductData
                            {
                                Id = 101,
                                Prices =  new NhanhPrices { Retail = 50000 },
                                Shipping =  new NhanhShipping() ,
                            }
                        }
                    })
                });

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("ahamove")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(AhamoveSuccess(30000m, distance: 9000));
            
            var httpClient = new HttpClient(handler.Object);
            var service = CreateService(dbContextMock.Object, httpClient, currentUser, productService);

            var request = new OrderRequest.CreateOrderRequest
            {
                Latitude = 10.1m, Longitude = 106.1m, Ward = 1, City = 1,
                Address = "Dia chi", ShortAddress = "Short Dia Chi",
                FullName = "User Test", Phone = "0901234567", Remarks = "Ghi chu",
                Items = new List<AhaMoveRequest.Items> { new AhaMoveRequest.Items { ItemId = 101, Number = 1 } },
                Gateway = "BaoKim"
            };
            
            //
            await service.CreateOrder(request);
            
            //
            var createdOrder = Assert.Single(orders);
            Assert.Equal(30000m, createdOrder.ShippingFee);
            Assert.Equal(80000m, createdOrder.TotalFee);
            
        }
    }
}
