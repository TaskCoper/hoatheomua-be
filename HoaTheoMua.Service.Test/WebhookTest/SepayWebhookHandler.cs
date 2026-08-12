// using System.Net;
// using System.Net.Http.Json;
// using System.Text.Json;
// using HoaTheoMua.Service.AhamoveService;
// using HoaTheoMua.Service.Exceptions;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using MockQueryable.Moq;
// using Moq;
// using Moq.Protected;
// using Xunit;
// using AppDbContext = HoaTheoMua.Repository.AppDbContext;
// using WebhookServiceImpl = HoaTheoMua.Service.Webhook.Service;
// using WebhookRequest = HoaTheoMua.Service.Webhook.Request;
// using OrderRoot = HoaTheoMua.Service.Webhook.OrderRoot;
// using OrderEntity = HoaTheoMua.Repository.Entity.Order;
// using OrderDetailEntity = HoaTheoMua.Repository.Entity.OrderDetails;
// using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
// using AddressEntity = HoaTheoMua.Repository.Entity.Address;
// using TransactionEntity = HoaTheoMua.Repository.Entity.Transaction;
// using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
//
// namespace HoaTheoMua.Service.Test.WebhookTest
// {
//
//     public class SepayWebhookHandlerTests
//     {
//         // Prefix GUID dùng chung cho test case mã bị cắt ngắn (24 ký tự hex đầu giống nhau).
//         private const string SharedPrefixHex = "d950e85c1a9b4a22947b2c8e"; // 24 hex
//         private static readonly Guid PrefixGuid1 = new Guid("d950e85c-1a9b-4a22-947b-2c8e0185e8a1");
//         private static readonly Guid PrefixGuid2 = new Guid("d950e85c-1a9b-4a22-947b-2c8e0185e8b2");
//         private static readonly Guid TestAddressId = Guid.Parse("6f765ce1-ad3b-47b7-bff0-3ca4d0b33d55");
//         
//         // Helpers
//
//         private static OrderDetailEntity CreateOrderDetail(
//             string name = "San pham A", int itemId = 101, int number = 2, decimal price = 50000m)
//         {
//             return new OrderDetailEntity
//             {
//                 Id = Guid.NewGuid(),
//                 Name = name,
//                 ItemId = itemId,
//                 Number = number,
//                 Price = price,
//                 Height = 10,
//                 Weight = 1,
//                 Length = 10,
//                 Width = 10,
//                 CreatedAt = DateTimeOffset.UtcNow
//             };
//         }
//
//         private static OrderEntity CreateOrder(
//             Guid? id = null,
//             string status = "Pending",
//             decimal totalFee = 100000m,
//             long orderTime = 0,
//             List<OrderDetailEntity>? details = null)
//         {
//             return new OrderEntity
//             {
//                 Id = id ?? Guid.NewGuid(),
//                 AddressId = TestAddressId,
//                 Ward = 1,
//                 City = 1,
//                 Address = "123 Duong Test",
//                 ShortAddress = "123 Duong Test, Q1, HCM",
//                 FullName = "Nguyen Van A",
//                 Phone = "0987654321",
//                 Remarks = "Giao gio hanh chinh",
//                 Status = status,
//                 TrackNumber = "TRACK-001",
//                 OrderTime = orderTime,
//                 TotalFee = totalFee,
//                 ProductFee = totalFee,
//                 ShippingFee = 0m,
//                 Latitude = 10.762622m,
//                 Longitude = 106.660172m,
//                 TagIds = new List<long>(),
//                 IsExportInvoice = false,
//                 CreatedAt = DateTimeOffset.UtcNow,
//                 OrderDetails = details ?? new List<OrderDetailEntity> { CreateOrderDetail() }
//             };
//         }
//
//         private static CompanyAddressEntity CreateCompanyAddress()
//         {
//             return new CompanyAddressEntity
//             {
//                 Id = Guid.NewGuid(),
//                 AddressLine = "123 Kho Test",
//                 Ward = "Ward 1",
//                 City = "Ho Chi Minh",
//                 Country = "Vietnam",
//                 PhoneNumber = "0912345678",
//                 FullName = "Shop Test",
//                 Remarks = "Den noi lay hang doc ma don",
//                 Latitude = 10.762622m,
//                 Longitude = 106.660172m,
//                 IsDeleted = false,
//                 CreatedAt = DateTimeOffset.UtcNow
//             };
//         }
//
//         private static AddressEntity CreateAddress()
//         {
//             return new AddressEntity
//             {
//                 Id = TestAddressId,
//                 UserId = Guid.NewGuid(),
//                 ReceiverName = "Nguyen Van A",
//                 PhoneNumber = "0987654321",
//                 AddressLine = "123 Duong Test",
//                 City = "Ho Chi Minh",
//                 Country = "Vietnam",
//                 IsDeleted = false,
//                 CreatedAt = DateTimeOffset.UtcNow
//             };
//         }
//
//         private static WebhookRequest.SepayWebhookRequest CreateWebhookRequest(
//             string content = "",
//             string code = "",
//             decimal transferAmount = 100000m)
//         {
//             return new WebhookRequest.SepayWebhookRequest
//             {
//                 Id = 12345,
//                 Gateway = "VietinBank",
//                 TransactionDate = "2026-06-23T19:18:33Z",
//                 AccountNumber = "0011000123456",
//                 SubAccount = "SUB-1",
//                 Code = code,
//                 Content = content,
//                 TransferType = "in",
//                 Description = "Chuyen tien",
//                 TransferAmount = transferAmount,
//                 ReferenceCode = "REF-001",
//                 Accumulated = 500000m
//             };
//         }
//
//         // Mock AppDbContext với đầy đủ các DbSet mà SepayWebhookHandler đụng tới.
//         // capturedTransactions (nếu truyền vào) sẽ nhận Transaction được Add để assert.
//         private static Mock<AppDbContext> CreateMockDbContext(
//             List<OrderEntity> orders,
//             List<CompanyAddressEntity> companyAddresses,
//             List<TransactionEntity>? capturedTransactions = null)
//         {
//             var mockOrders = orders.BuildMockDbSet();
//             var mockCompany = companyAddresses.BuildMockDbSet();
//             var mockAddresses = new List<AddressEntity> { CreateAddress() }.BuildMockDbSet();
//
//             var configs = new List<ConfigEntity>
//             {
//                 new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
//                 new ConfigEntity { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
//                 new ConfigEntity { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
//                 new ConfigEntity { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false },
//                 new ConfigEntity { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false },
//                 new ConfigEntity { Key = "ahamove.base_url", Value = "{\"value\": \"http://ahamove.test\"}", Group = "ahamove", IsDeleted = false }
//             };
//             var mockConfigs = configs.BuildMockDbSet();
//
//             var mockTransactions = new List<TransactionEntity>().BuildMockDbSet();
//             if (capturedTransactions != null)
//             {
//                 mockTransactions
//                     .Setup(x => x.Add(It.IsAny<TransactionEntity>()))
//                     .Callback<TransactionEntity>(t => capturedTransactions.Add(t));
//             }
//
//             var options = new DbContextOptionsBuilder<AppDbContext>().Options;
//             var mockDbContext = new Mock<AppDbContext>(options);
//
//             mockDbContext.Setup(x => x.Orders).Returns(mockOrders.Object);
//             mockDbContext.Setup(x => x.CompanyAddresses).Returns(mockCompany.Object);
//             mockDbContext.Setup(x => x.Addresses).Returns(mockAddresses.Object);
//             mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);
//             mockDbContext.Setup(x => x.Transactions).Returns(mockTransactions.Object);
//             mockDbContext
//                 .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(1);
//
//             return mockDbContext;
//         }
//
//         private static WebhookServiceImpl CreateService(AppDbContext dbContext, HttpClient httpClient)
//         {
//             var mockConfig = new Mock<IConfiguration>();
//             var mockConfigSection = new Mock<IConfigurationSection>();
//             mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
//             mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);
//
//             var mockOrderService = new Mock<HoaTheoMua.Service.Order.IService>();
//             var mockConfigService = new Mock<HoaTheoMua.Service.Config.IService>();
//
//             var mockAhaMoveService = new Mock<IService>();
//             mockAhaMoveService
//                 .Setup(x => x.GetServiceDetailsAhaMove())
//                 .ReturnsAsync(new ServiceDetailsResponse());
//
//             return new WebhookServiceImpl(
//                 dbContext,
//                 httpClient,
//                 mockConfig.Object,
//                 mockOrderService.Object,
//                 mockConfigService.Object,
//                 mockAhaMoveService.Object);
//         }
//
//         private static HttpClient EmptyHttpClient()
//         {
//             return new HttpClient(new Mock<HttpMessageHandler>().Object);
//         }
//
//         private static HttpResponseMessage AhamoveSuccess(
//             string orderId = "AHA-123", string sharedLink = "https://track/aha")
//         {
//             return new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.OK,
//                 Content = JsonContent.Create(new { order_id = orderId, shared_link = sharedLink })
//             };
//         }
//
//         private static HttpResponseMessage NhanhSuccess(
//             int id = 555, string trackingUrl = "https://track/nhanh")
//         {
//             return new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.OK,
//                 Content = JsonContent.Create(new { code = 1, data = new { id, trackingUrl } })
//             };
//         }
//
//         // Handler định tuyến theo host: URL chứa "ahamove" -> ahamoveResponse, chứa "nhanhvn" -> nhanhResponse.
//         private static Mock<HttpMessageHandler> CreateHttpHandler(
//             HttpResponseMessage ahamoveResponse,
//             HttpResponseMessage nhanhResponse,
//             Action<string>? captureAhamoveBody = null,
//             Action<string>? captureNhanhBody = null)
//         {
//             var handler = new Mock<HttpMessageHandler>();
//
//             handler.Protected()
//                 .Setup<Task<HttpResponseMessage>>(
//                     "SendAsync",
//                     ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("ahamove")),
//                     ItExpr.IsAny<CancellationToken>())
//                 .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
//                 {
//                     if (captureAhamoveBody != null && r.Content != null)
//                         captureAhamoveBody(r.Content.ReadAsStringAsync().Result);
//                 })
//                 .ReturnsAsync(ahamoveResponse);
//
//             handler.Protected()
//                 .Setup<Task<HttpResponseMessage>>(
//                     "SendAsync",
//                     ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("nhanhvn")),
//                     ItExpr.IsAny<CancellationToken>())
//                 .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
//                 {
//                     if (captureNhanhBody != null && r.Content != null)
//                         captureNhanhBody(r.Content.ReadAsStringAsync().Result);
//                 })
//                 .ReturnsAsync(nhanhResponse);
//
//             return handler;
//         }
//         
//         // TEST CASE 01: Xử lý webhook thành công với orderId trong Content
//         [Fact]
//         public async Task SepayWebhookHandler_ValidOrderIdInContent_ProcessesPaymentSuccessfully()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 0); // OrderTime == 0 để đi qua nhánh gọi Ahamove
//             var capturedTransactions = new List<TransactionEntity>();
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() },
//                 capturedTransactions).Object;
//
//             var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"Thanh toan don HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             Assert.Equal("Paid", order.Status);
//             Assert.Equal("AHA-123", order.AhahaMoveOrderId);
//             Assert.Equal("https://track/aha", order.AhahaMoveTrackingUrl);
//             Assert.Equal(555, order.NhanhOrderId);
//             Assert.Equal("https://track/nhanh", order.NhanhTrackingUrl);
//             Assert.Single(capturedTransactions);
//         }
//         
//         // TEST CASE 02: Xử lý webhook thành công với orderId trong Code (Content không hợp lệ)
//         [Fact]
//         public async Task SepayWebhookHandler_ValidOrderIdInCode_ProcessesPaymentSuccessfully()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 0);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: "Noi dung khong co ma hop le",
//                 code: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             Assert.Equal("Paid", order.Status);
//         }
//         
//         // TEST CASE 03: Xử lý webhook thành công với mã bị cắt ngắn (prefix match 1 order)
//         [Fact]
//         public async Task SepayWebhookHandler_TruncatedCodeMatchesSingleOrder_ProcessesPayment()
//         {
//             // Arrange: OrderTime > 0 để bỏ qua nhánh Ahamove, chỉ cần mock NhanhVn.
//             var order = CreateOrder(id: PrefixGuid1, orderTime: 1700000000);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             // 24 ký tự hex đầu -> rơi vào nhánh prefix (chưa đủ full GUID 32 ký tự)
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{SharedPrefixHex}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             Assert.Equal("Paid", order.Status);
//         }
//         
//         // TEST CASE 04: Không tìm thấy mã thanh toán hợp lệ -> BadRequestException
//         [Fact]
//         public async Task SepayWebhookHandler_NoValidPaymentCode_ThrowsBadRequestException()
//         {
//             // Arrange
//             var order = CreateOrder();
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var service = CreateService(dbContext, EmptyHttpClient());
//
//             var request = CreateWebhookRequest(
//                 content: "Chuyen tien khong co ma",
//                 code: "REF12345");
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Equal("Invalid description format", exception.Message);
//         }
//         
//         // TEST CASE 05: Prefix match nhiều hơn 1 order -> ConflictException
//         [Fact]
//         public async Task SepayWebhookHandler_TruncatedCodeMatchesMultipleOrders_ThrowsConflictException()
//         {
//             // Arrange: 2 order có 24 ký tự hex đầu giống nhau.
//             var order1 = CreateOrder(id: PrefixGuid1);
//             var order2 = CreateOrder(id: PrefixGuid2);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order1, order2 },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var service = CreateService(dbContext, EmptyHttpClient());
//
//             var request = CreateWebhookRequest(content: $"HTM-{SharedPrefixHex}");
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<ConflictException>(act);
//             Assert.Equal("Ambiguous payment code", exception.Message);
//         }
//         
//         // TEST CASE 06: Không tìm thấy order trong database -> NotFoundException
//         [Fact]
//         public async Task SepayWebhookHandler_OrderNotFound_ThrowsNotFoundException()
//         {
//             // Arrange: DB rỗng, nhưng Content chứa 1 GUID hợp lệ.
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity>(),
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var service = CreateService(dbContext, EmptyHttpClient());
//
//             var request = CreateWebhookRequest(content: $"HTM-{Guid.NewGuid()}");
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<NotFoundException>(act);
//             Assert.Equal("Order not found in description", exception.Message);
//         }
//         
//         // TEST CASE 07: Order không ở trạng thái Pending -> NotFoundException
//         [Fact]
//         public async Task SepayWebhookHandler_OrderNotPending_ThrowsNotFoundException()
//         {
//             // Arrange
//             var order = CreateOrder(status: "Cancelled");
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var service = CreateService(dbContext, EmptyHttpClient());
//
//             var request = CreateWebhookRequest(content: $"HTM-{order.Id}");
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<NotFoundException>(act);
//             Assert.Equal("Order not on Pending", exception.Message);
//         }
//         
//         // TEST CASE 08: Số tiền chuyển khoản không khớp
//         // .md ghi BadRequestException("Số tiền không khớp"), nhưng code thực tế ném
//         // ConflictException("Transfer amount must be equal to transfer amount").
//         [Fact]
//         public async Task SepayWebhookHandler_TransferAmountMismatch_ThrowsConflictException()
//         {
//             // Arrange
//             var order = CreateOrder(totalFee: 100000m);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var service = CreateService(dbContext, EmptyHttpClient());
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: 99999m); // Khác TotalFee
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<ConflictException>(act);
//             Assert.Equal("Transfer amount must be equal to transfer amount", exception.Message);
//         }
//         
//         // TEST CASE 09: Ahamove API trả về lỗi HTTP -> Exception
//         [Fact]
//         public async Task SepayWebhookHandler_AhamoveHttpError_ThrowsException()
//         {
//             // Arrange: OrderTime == 0 để đi vào nhánh gọi Ahamove.
//             var order = CreateOrder(orderTime: 0);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var ahamoveError = new HttpResponseMessage { StatusCode = HttpStatusCode.BadGateway };
//             var handler = CreateHttpHandler(ahamoveError, NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Contains("Ahamove API HTTP Error", exception.Message);
//         }
//         
//         // TEST CASE 10: Ahamove response deserialize ra null -> Exception
//         [Fact]
//         public async Task SepayWebhookHandler_AhamoveReturnsNull_ThrowsException()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 0);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             // HTTP 200 nhưng body là "null" -> ReadFromJsonAsync trả về null.
//             var ahamoveNull = new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.OK,
//                 Content = JsonContent.Create<object?>(null)
//             };
//             var handler = CreateHttpHandler(ahamoveNull, NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Equal("Ahamove returned null or empty data", exception.Message);
//         }
//         
//         // TEST CASE 11: Ahamove trả về thành công -> cập nhật OrderId & TrackingUrl
//         [Fact]
//         public async Task SepayWebhookHandler_AhamoveSuccess_UpdatesAhamoveOrderInfo()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 0);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var handler = CreateHttpHandler(
//                 AhamoveSuccess("AHA-999", "https://track/aha-999"),
//                 NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             Assert.Equal("AHA-999", order.AhahaMoveOrderId);
//             Assert.Equal("https://track/aha-999", order.AhahaMoveTrackingUrl);
//         }
//         
//         // TEST CASE 12: NhanhVn API trả về lỗi HTTP -> Exception
//         [Fact]
//         public async Task SepayWebhookHandler_NhanhVnHttpError_ThrowsException()
//         {
//             // Arrange: OrderTime > 0 để bỏ qua Ahamove, tập trung vào lỗi NhanhVn.
//             var order = CreateOrder(orderTime: 1700000000);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var nhanhError = new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.InternalServerError,
//                 Content = JsonContent.Create(new { message = "server error" })
//             };
//             var handler = CreateHttpHandler(AhamoveSuccess(), nhanhError);
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Contains("NhanhVn API HTTP Error", exception.Message);
//         }
//         
//         // TEST CASE 13: NhanhVn response có data null -> Exception (order creation failed)
//         [Fact]
//         public async Task SepayWebhookHandler_NhanhVnDataNull_ThrowsException()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 1700000000);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             // code == 1 nhưng data không phải object (null) -> code coi là thất bại.
//             var nhanhDataNull = new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.OK,
//                 Content = JsonContent.Create(new { code = 1, data = (object?)null })
//             };
//             var handler = CreateHttpHandler(AhamoveSuccess(), nhanhDataNull);
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Contains("NhanhVn order creation failed", exception.Message);
//         }
//         
//         // TEST CASE 14: NhanhVn trả về Code = 0 -> Exception (order creation failed)
//         [Fact]
//         public async Task SepayWebhookHandler_NhanhVnCodeZero_ThrowsException()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 1700000000);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var nhanhCodeZero = new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.OK,
//                 Content = JsonContent.Create(new { code = 0, messages = new[] { "invalid" } })
//             };
//             var handler = CreateHttpHandler(AhamoveSuccess(), nhanhCodeZero);
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Contains("NhanhVn order creation failed", exception.Message);
//         }
//         
//         // TEST CASE 15: NhanhVn trả về thành công -> cập nhật NhanhOrderId & TrackingUrl
//         [Fact]
//         public async Task SepayWebhookHandler_NhanhVnSuccess_UpdatesNhanhOrderInfo()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 1700000000);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var handler = CreateHttpHandler(
//                 AhamoveSuccess(),
//                 NhanhSuccess(777, "https://track/nhanh-777"));
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             Assert.Equal(777, order.NhanhOrderId);
//             Assert.Equal("https://track/nhanh-777", order.NhanhTrackingUrl);
//         }
//         
//         // TEST CASE 16: Tạo transaction với đầy đủ field từ request
//         [Fact]
//         public async Task SepayWebhookHandler_Success_CreatesTransactionWithCorrectFields()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 1700000000);
//             var capturedTransactions = new List<TransactionEntity>();
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() },
//                 capturedTransactions).Object;
//
//             var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             var transaction = Assert.Single(capturedTransactions);
//             Assert.Equal(request.Id.ToString(), transaction.RequestId);
//             Assert.Equal(request.Gateway, transaction.Gateway);
//             Assert.Equal(request.AccountNumber, transaction.AccountNumber);
//             Assert.Equal(request.SubAccount, transaction.SubAccount);
//             Assert.Equal(request.Code, transaction.Code);
//             Assert.Equal(request.Content, transaction.Content);
//             Assert.Equal(request.TransferType, transaction.TransferType);
//             Assert.Equal(request.Description, transaction.Description);
//             Assert.Equal(request.TransferAmount, transaction.TransferAmount);
//             Assert.Equal(request.Accumulated, transaction.Accumulated);
//             Assert.Equal(request.ReferenceCode, transaction.ReferenceCode);
//             Assert.Equal(order.Id, transaction.OrderId);
//         }
//         
//         // TEST CASE 19: Mapping OrderDetails -> AhamoveItem đúng
//         [Fact]
//         public async Task SepayWebhookHandler_MapsOrderDetailsToAhamoveItems()
//         {
//             // Arrange
//             var details = new List<OrderDetailEntity>
//             {
//                 CreateOrderDetail("San pham A", itemId: 101, number: 2, price: 50000m),
//                 CreateOrderDetail("San pham B", itemId: 102, number: 3, price: 30000m)
//             };
//             var order = CreateOrder(orderTime: 0, details: details);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             string ahamoveBody = string.Empty;
//             var handler = CreateHttpHandler(
//                 AhamoveSuccess(),
//                 NhanhSuccess(),
//                 captureAhamoveBody: body => ahamoveBody = body);
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             var ahamoveRequest = JsonSerializer.Deserialize<AhaMoveRequest.AhaMoveShippingFeeRequest>(ahamoveBody);
//             Assert.NotNull(ahamoveRequest);
//             Assert.NotNull(ahamoveRequest!.Items);
//             Assert.Equal(2, ahamoveRequest.Items!.Count);
//
//             var firstItem = ahamoveRequest.Items[0];
//             Assert.Equal(details[0].Id.ToString(), firstItem.Id);
//             Assert.Equal(details[0].Name, firstItem.Name);
//             Assert.Equal((int)details[0].Price, firstItem.Price);
//             Assert.Equal(details[0].Number, firstItem.Num);
//         }
//         
//         // TEST CASE 20: Mapping OrderDetails -> PackageDetail đúng
//         [Fact]
//         public async Task SepayWebhookHandler_MapsOrderDetailsToPackageDetail()
//         {
//             // Arrange
//             var details = new List<OrderDetailEntity>
//             {
//                 CreateOrderDetail("San pham A", itemId: 101, number: 2, price: 50000m)
//             };
//             var order = CreateOrder(orderTime: 0, details: details);
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             string ahamoveBody = string.Empty;
//             var handler = CreateHttpHandler(
//                 AhamoveSuccess(),
//                 NhanhSuccess(),
//                 captureAhamoveBody: body => ahamoveBody = body);
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             var ahamoveRequest = JsonSerializer.Deserialize<AhaMoveRequest.AhaMoveShippingFeeRequest>(ahamoveBody);
//             Assert.NotNull(ahamoveRequest);
//             Assert.NotNull(ahamoveRequest!.PackageDetail);
//             var package = Assert.Single(ahamoveRequest.PackageDetail!);
//             Assert.Equal(details[0].Height, package.Height);
//             Assert.Equal(details[0].Weight, package.Weight);
//             Assert.Equal(details[0].Length, package.Length);
//             Assert.Equal(details[0].Width, package.Width);
//         }
//         
//         // TEST CASE 21: Mapping OrderDetails -> NhanhVn Product đúng
//         [Fact]
//         public async Task SepayWebhookHandler_MapsOrderDetailsToNhanhVnProducts()
//         {
//             // Arrange
//             var details = new List<OrderDetailEntity>
//             {
//                 CreateOrderDetail("San pham A", itemId: 101, number: 2, price: 50000m),
//                 CreateOrderDetail("San pham B", itemId: 102, number: 3, price: 30000m)
//             };
//             var order = CreateOrder(orderTime: 1700000000, details: details); // bỏ qua Ahamove
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             string nhanhBody = string.Empty;
//             var handler = CreateHttpHandler(
//                 AhamoveSuccess(),
//                 NhanhSuccess(),
//                 captureNhanhBody: body => nhanhBody = body);
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             var nhanhRequest = JsonSerializer.Deserialize<OrderRoot>(nhanhBody);
//             Assert.NotNull(nhanhRequest);
//             Assert.NotNull(nhanhRequest!.Products);
//             Assert.Equal(2, nhanhRequest.Products.Count);
//
//             var firstProduct = nhanhRequest.Products[0];
//             Assert.Equal(details[0].ItemId, firstProduct.Id);
//             Assert.Equal(details[0].Price, firstProduct.Price);
//             Assert.Equal(details[0].Number, firstProduct.Quantity);
//         }
//         
//         // TEST CASE 22: Order không có OrderDetails -> vẫn xử lý thành công (theo code hiện tại)
//         [Fact]
//         public async Task SepayWebhookHandler_EmptyOrderDetails_StillProcessesSuccessfully()
//         {
//             // Arrange
//             var order = CreateOrder(orderTime: 0, details: new List<OrderDetailEntity>());
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
//             var service = CreateService(dbContext, new HttpClient(handler.Object));
//
//             var request = CreateWebhookRequest(
//                 content: $"HTM-{order.Id}",
//                 transferAmount: order.TotalFee);
//
//             // Act
//             await service.SepayWebhookHandler(request);
//
//             // Assert
//             Assert.Equal("Paid", order.Status);
//         }
//         
//         // TEST CASE 24: Webhook bị gửi lại lần 2 (order đã Paid) -> NotFoundException
//         [Fact]
//         public async Task SepayWebhookHandler_OrderAlreadyPaid_ThrowsNotFoundException()
//         {
//             // Arrange
//             var order = CreateOrder(status: "Paid");
//             var dbContext = CreateMockDbContext(
//                 new List<OrderEntity> { order },
//                 new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;
//
//             var service = CreateService(dbContext, EmptyHttpClient());
//
//             var request = CreateWebhookRequest(content: $"HTM-{order.Id}");
//
//             // Act
//             var act = async () => await service.SepayWebhookHandler(request);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<NotFoundException>(act);
//             Assert.Equal("Order not on Pending", exception.Message);
//         }
//     }
// }
