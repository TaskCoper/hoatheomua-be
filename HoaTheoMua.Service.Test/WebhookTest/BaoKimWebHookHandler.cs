using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Xunit;
using AppDbContext = HoaTheoMua.Repository.AppDbContext;
using WebhookServiceImpl = HoaTheoMua.Service.Webhook.Service;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using OrderDetailEntity = HoaTheoMua.Repository.Entity.OrderDetails;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using TransactionEntity = HoaTheoMua.Repository.Entity.Transaction;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.WebhookTest
{
    public class BaoKimWebHookHandlerTests
    {
        // Helpers

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

        private static OrderEntity CreateOrder(
            Guid? id = null,
            string status = "Pending",
            decimal totalFee = 100000m,
            long orderTime = 0,
            List<OrderDetailEntity>? details = null)
        {
            return new OrderEntity
            {
                Id = id ?? Guid.NewGuid(),
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

        // Models BaoKim's real fee arithmetic, verified against both the documented sample and a
        // live callback:
        //   transaction.total_amount = grossAmount + user_fee   (what the buyer is charged)
        //   transaction.amount       = grossAmount - merchant_fee  (NET settled to us; can go
        //                                                           negative when fees exceed the
        //                                                           order)
        // grossAmount is order.total_amount. Defaulting merchant_fee to 0 the way the docs' sample
        // does would make `amount` coincidentally equal the order total and hide that difference.
        private static BaoKimTransaction CreateBaoKimTransaction(
            long id = 940826,
            int status = 1,
            decimal grossAmount = 100000m,
            decimal merchantFee = 0m,
            decimal userFee = 0m,
            string? description = "Thanh toan don hang")
        {
            return new BaoKimTransaction
            {
                Id = id,
                OrderId = 991186,
                MrcOrderId = Guid.NewGuid().ToString(),
                Type = 1,
                PaymentMethod = 1,
                MerchantFee = merchantFee,
                UserFee = userFee,
                TotalAmount = grossAmount + userFee,
                Amount = grossAmount - merchantFee,
                Description = description,
                Status = status,
                CompletedAt = new DateTime(2026, 6, 9, 3, 46, 32, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 6, 9, 3, 45, 45, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 6, 9, 3, 46, 32, DateTimeKind.Utc)
            };
        }

        private static BaoKimPaymentCallback CreateCallback(
            Guid orderId,
            int resultCode = 200,
            int orderStatus = 1,
            decimal totalAmount = 100000m,
            List<BaoKimTransaction>? transactions = null)
        {
            return new BaoKimPaymentCallback
            {
                RequestId = "B2B6A278C9815BD7EQ4AVPVZCI",
                RequestTime = "2026-06-09 10:46:32",
                MerchantCode = "hoatheomua",
                Operation = "PAYMENT_TRANS",
                PaymentResult = new BaoKimPaymentResult
                {
                    ResultCode = resultCode,
                    ResultMessage = "Payment successful",
                    Order = new BaoKimOrder
                    {
                        Id = 991186,
                        MrcOrderId = orderId.ToString(),
                        TotalAmount = totalAmount,
                        PaymentMethod = 1,
                        Status = orderStatus,
                        CompletedAt = new DateTime(2026, 6, 9, 3, 46, 32, DateTimeKind.Utc),
                        CreatedAt = new DateTime(2026, 6, 9, 3, 45, 41, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2026, 6, 9, 3, 46, 32, DateTimeKind.Utc)
                    },
                    Transaction = transactions ?? new List<BaoKimTransaction>
                    {
                        CreateBaoKimTransaction(grossAmount: totalAmount)
                    }
                },
                TokenResult = null
            };
        }

        private static Mock<AppDbContext> CreateMockDbContext(
            List<OrderEntity> orders,
            List<CompanyAddressEntity> companyAddresses,
            List<TransactionEntity>? capturedTransactions = null)
        {
            var mockOrders = orders.BuildMockDbSet();
            var mockCompany = companyAddresses.BuildMockDbSet();

            var configs = new List<ConfigEntity>
            {
                new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.base_url", Value = "{\"value\": \"http://nhanhvn.test\"}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.app_id", Value = "{\"value\": 12345}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "nhanhvn.business_id", Value = "{\"value\": 67890}", Group = "nhanhvn", IsDeleted = false },
                new ConfigEntity { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false },
                new ConfigEntity { Key = "ahamove.base_url", Value = "{\"value\": \"http://ahamove.test\"}", Group = "ahamove", IsDeleted = false }
            };
            var mockConfigs = configs.BuildMockDbSet();

            var mockTransactions = new List<TransactionEntity>().BuildMockDbSet();
            if (capturedTransactions != null)
            {
                mockTransactions
                    .Setup(x => x.Add(It.IsAny<TransactionEntity>()))
                    .Callback<TransactionEntity>(t => capturedTransactions.Add(t));
            }

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            mockDbContext.Setup(x => x.Orders).Returns(mockOrders.Object);
            mockDbContext.Setup(x => x.CompanyAddresses).Returns(mockCompany.Object);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);
            mockDbContext.Setup(x => x.Transactions).Returns(mockTransactions.Object);
            mockDbContext
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            return mockDbContext;
        }

        private static WebhookServiceImpl CreateService(AppDbContext dbContext, HttpClient httpClient)
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockOrderService = new Mock<HoaTheoMua.Service.Order.IService>();
            var mockConfigService = new Mock<HoaTheoMua.Service.Config.IService>();

            var mockAhaMoveService = new Mock<HoaTheoMua.Service.AhamoveService.IService>();
            mockAhaMoveService
                .Setup(x => x.GetServiceDetailsAhaMove())
                .ReturnsAsync(new ServiceDetailsResponse());

            return new WebhookServiceImpl(
                dbContext,
                httpClient,
                mockConfig.Object,
                mockOrderService.Object,
                mockConfigService.Object,
                mockAhaMoveService.Object);
        }

        // SendAsync không được setup, nên mọi lời gọi HTTP sẽ nổ thay vì âm thầm thành công.
        // Các test "chỉ ack" bên dưới dựa vào đó để chứng minh chúng thoát sớm.
        private static HttpClient EmptyHttpClient()
        {
            return new HttpClient(new Mock<HttpMessageHandler>().Object);
        }

        private static HttpResponseMessage AhamoveSuccess(
            string orderId = "AHA-123", string sharedLink = "https://track/aha")
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { order_id = orderId, shared_link = sharedLink })
            };
        }

        private static HttpResponseMessage NhanhSuccess(
            int id = 555, string trackingUrl = "https://track/nhanh")
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { code = 1, data = new { id, trackingUrl } })
            };
        }

        private static Mock<HttpMessageHandler> CreateHttpHandler(
            HttpResponseMessage ahamoveResponse,
            HttpResponseMessage nhanhResponse)
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("ahamove")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(ahamoveResponse);

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("nhanhvn")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(nhanhResponse);

            return handler;
        }

        // TEST CASE 01: Payload mẫu nguyên văn trong tài liệu BaoKim phải deserialize được.
        // BaoKim gửi datetime dạng "2026-06-09 10:46:32" (không 'T', không offset) mà
        // System.Text.Json mặc định từ chối -> thiếu converter thì payload không bind được và
        // handler không bao giờ chạy.
        [Fact]
        public void BaoKimPaymentCallback_DeserializesDocumentedSamplePayload()
        {
            // Arrange: JSON lấy nguyên văn từ tài liệu webhook của BaoKim.
            const string json = """
            {
              "request_id": "B2B6A278C9815BD7EQ4AVPVZCI",
              "request_time": "2026-06-09 10:46:32",
              "merchant_code": "123workvn",
              "operation": "PAYMENT_TRANS",
              "payment_result": {
                "result_code": 200,
                "result_message": "Payment successful",
                "order": {
                  "id": 991186,
                  "mrc_order_id": "MERCHANT_DEPOSIT_48_1780976739_UXWUUAIE",
                  "total_amount": 10000,
                  "payment_method": 1,
                  "status": 1,
                  "completed_at": "2026-06-09 10:46:32",
                  "created_at": "2026-06-09 10:45:41",
                  "updated_at": "2026-06-09 10:46:32"
                },
                "transaction": [
                  {
                    "id": 940826,
                    "order_id": 991186,
                    "mrc_order_id": "MERCHANT_DEPOSIT_48_1780976739_UXWUUAIE",
                    "type": 1,
                    "payment_method": 1,
                    "merchant_fee": 0,
                    "user_fee": 330,
                    "total_amount": 10330,
                    "amount": 10000,
                    "description": null,
                    "status": 1,
                    "bnpl": null,
                    "installment": null,
                    "completed_at": "2026-06-09 10:46:32",
                    "created_at": "2026-06-09 10:45:45",
                    "updated_at": "2026-06-09 10:46:32"
                  }
                ]
              },
              "token_result": null
            }
            """;

            // Act
            var callback = JsonSerializer.Deserialize<BaoKimPaymentCallback>(json);

            // Assert
            Assert.NotNull(callback);
            Assert.Equal("PAYMENT_TRANS", callback!.Operation);
            Assert.Equal(200, callback.PaymentResult.ResultCode);
            Assert.Equal("MERCHANT_DEPOSIT_48_1780976739_UXWUUAIE", callback.PaymentResult.Order.MrcOrderId);
            Assert.Equal(10000m, callback.PaymentResult.Order.TotalAmount);
            Assert.Equal(1, callback.PaymentResult.Order.Status);

            var transaction = Assert.Single(callback.PaymentResult.Transaction);
            Assert.Equal(940826, transaction.Id);
            Assert.Equal(10000m, transaction.Amount);
            Assert.Null(transaction.Description);

            // "2026-06-09 10:46:32" là giờ VN (UTC+7) -> 03:46:32 UTC, Kind phải là Utc vì Npgsql
            // yêu cầu như vậy cho cột timestamptz.
            Assert.Equal(new DateTime(2026, 6, 9, 3, 46, 32, DateTimeKind.Utc), transaction.CompletedAt);
            Assert.Equal(DateTimeKind.Utc, transaction.CompletedAt!.Value.Kind);
        }

        // TEST CASE 02: "transaction": null tường minh -> ghi đè property initializer thành null.
        // Nếu handler không dùng null-conditional thì đây là NullReferenceException -> 500 -> BaoKim
        // gửi lại 10 lần.
        [Fact]
        public void BaoKimPaymentCallback_ExplicitNullTransaction_DeserializesToNull()
        {
            // Arrange
            const string json = """
            {
              "request_id": "REQ-1",
              "request_time": "2026-06-09 10:46:32",
              "merchant_code": "hoatheomua",
              "operation": "PAYMENT_TRANS",
              "payment_result": {
                "result_code": 200,
                "result_message": "Payment successful",
                "order": {
                  "id": 991186,
                  "mrc_order_id": "abc",
                  "total_amount": 10000,
                  "payment_method": 1,
                  "status": 1,
                  "completed_at": null,
                  "created_at": "2026-06-09 10:45:41",
                  "updated_at": "2026-06-09 10:46:32"
                },
                "transaction": null
              },
              "token_result": null
            }
            """;

            // Act
            var callback = JsonSerializer.Deserialize<BaoKimPaymentCallback>(json);

            // Assert
            Assert.NotNull(callback);
            Assert.Null(callback!.PaymentResult.Transaction);
            Assert.Null(callback.PaymentResult.Order.CompletedAt);
        }

        // TEST CASE 03: Thanh toán thành công -> order chuyển Paid, gọi Ahamove + NhanhVn.
        [Fact]
        public async Task BaoKimWebHookHandler_SuccessfulPayment_ProcessesPaymentSuccessfully()
        {
            // Arrange: OrderTime == 0 để đi qua nhánh gọi Ahamove.
            var order = CreateOrder(orderTime: 0);
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
            var service = CreateService(dbContext, new HttpClient(handler.Object));

            var request = CreateCallback(order.Id, totalAmount: order.TotalFee);

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            Assert.Equal("Paid", order.Status);
            Assert.Equal("AHA-123", order.AhahaMoveOrderId);
            Assert.Equal("https://track/aha", order.AhahaMoveTrackingUrl);
            Assert.Equal(555, order.NhanhOrderId);
            Assert.Equal("https://track/nhanh", order.NhanhTrackingUrl);
            Assert.Single(capturedTransactions);
        }

        // TEST CASE 04: Transaction được tạo với đầy đủ field lấy từ callback.
        [Fact]
        public async Task BaoKimWebHookHandler_Success_CreatesTransactionWithCorrectFields()
        {
            // Arrange: OrderTime > 0 để bỏ qua Ahamove.
            var order = CreateOrder(orderTime: 1700000000);
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
            var service = CreateService(dbContext, new HttpClient(handler.Object));

            var request = CreateCallback(order.Id, totalAmount: order.TotalFee);

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            var transaction = Assert.Single(capturedTransactions);
            Assert.Equal("B2B6A278C9815BD7EQ4AVPVZCI", transaction.RequestId); // request_id callback
            Assert.Equal("BaoKim", transaction.Gateway);
            Assert.Equal(PaymentMethod.VA, transaction.paymentMethod); // payment_method = 1
            Assert.Equal(order.Id.ToString(), transaction.Code); // mrc_order_id
            Assert.Equal("991186", transaction.ReferenceCode);   // order.id phía BaoKim
            Assert.Equal("in", transaction.TransferType);
            Assert.Equal(order.TotalFee, transaction.TransferAmount);
            Assert.Equal(order.Id, transaction.OrderId);
            // Npgsql từ chối Kind khác Utc trên cột timestamptz.
            Assert.Equal(DateTimeKind.Utc, transaction.TransactionDate.Kind);
        }

        // TEST CASE 05: Thanh toán thất bại (result_code != 200) -> ack, order giữ nguyên Pending.
        [Fact]
        public async Task BaoKimWebHookHandler_FailedResultCode_AcksWithoutFulfilling()
        {
            // Arrange
            var order = CreateOrder();
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(order.Id, resultCode: 400, totalAmount: order.TotalFee);

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            Assert.Equal("Pending", order.Status);
            Assert.Empty(capturedTransactions);
        }

        // TEST CASE 06: order.status != 1 (chưa hoàn thành) -> ack, không fulfil.
        [Fact]
        public async Task BaoKimWebHookHandler_OrderStatusNotCompleted_AcksWithoutFulfilling()
        {
            // Arrange
            var order = CreateOrder();
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(order.Id, orderStatus: 0, totalAmount: order.TotalFee);

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            Assert.Equal("Pending", order.Status);
            Assert.Empty(capturedTransactions);
        }

        // TEST CASE 07: Webhook gửi lại khi order đã Paid -> ack (không ném), không fulfil lại.
        // BaoKim gửi lại tới 10 lần nếu không nhận code 0, nên replay là traffic bình thường.
        [Fact]
        public async Task BaoKimWebHookHandler_OrderAlreadyPaid_AcksWithoutRefulfilling()
        {
            // Arrange
            var order = CreateOrder(status: "Paid");
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(order.Id, totalAmount: order.TotalFee);

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert: không tạo transaction trùng, không gọi lại Ahamove/NhanhVn.
            Assert.Equal("Paid", order.Status);
            Assert.Empty(capturedTransactions);
        }

        // TEST CASE 08: mrc_order_id không phải GUID -> BadRequestException.
        [Fact]
        public async Task BaoKimWebHookHandler_InvalidMrcOrderId_ThrowsBadRequestException()
        {
            // Arrange
            var order = CreateOrder();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(order.Id);
            request.PaymentResult.Order.MrcOrderId = "KHONG-PHAI-GUID";

            // Act
            var act = async () => await service.BaoKimWebHookHandler(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Invalid mrc_order_id format", exception.Message);
        }

        // TEST CASE 09: Không tìm thấy order -> NotFoundException.
        [Fact]
        public async Task BaoKimWebHookHandler_OrderNotFound_ThrowsNotFoundException()
        {
            // Arrange: DB rỗng.
            var dbContext = CreateMockDbContext(
                new List<OrderEntity>(),
                new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(Guid.NewGuid());

            // Act
            var act = async () => await service.BaoKimWebHookHandler(request);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("Order not found for mrc_order_id", exception.Message);
        }

        // TEST CASE 10: Số tiền không khớp TotalFee -> ConflictException.
        [Fact]
        public async Task BaoKimWebHookHandler_AmountMismatch_ThrowsConflictException()
        {
            // Arrange
            var order = CreateOrder(totalFee: 100000m);
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(order.Id, totalAmount: 99999m);

            // Act
            var act = async () => await service.BaoKimWebHookHandler(request);

            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(act);
            Assert.Equal("Paid amount does not match the order total", exception.Message);
        }

        // TEST CASE 11: Order đã bị huỷ -> ConflictException (không âm thầm fulfil).
        [Fact]
        public async Task BaoKimWebHookHandler_OrderCancelled_ThrowsConflictException()
        {
            // Arrange
            var order = CreateOrder(status: "Cancelled");
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = CreateCallback(order.Id, totalAmount: order.TotalFee);

            // Act
            var act = async () => await service.BaoKimWebHookHandler(request);

            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(act);
            Assert.Contains("Order is not Pending", exception.Message);
        }

        // TEST CASE 12: BaoKim tách nhiều transaction -> chọn đúng cái đã hoàn thành (status == 1).
        [Fact]
        public async Task BaoKimWebHookHandler_MultipleTransactions_PicksCompletedOne()
        {
            // Arrange
            var order = CreateOrder(orderTime: 1700000000);
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
            var service = CreateService(dbContext, new HttpClient(handler.Object));

            // RequestId nay lay tu request_id (chung cho ca callback), nen phai phan biet hai
            // transaction bang Description/Amount — thu thuc su doi theo cai duoc chon.
            var request = CreateCallback(order.Id, totalAmount: order.TotalFee,
                transactions: new List<BaoKimTransaction>
                {
                    CreateBaoKimTransaction(
                        id: 111, status: 0, grossAmount: 1m, description: "Giao dich that bai"),
                    CreateBaoKimTransaction(
                        id: 222, status: 1, grossAmount: order.TotalFee, description: "Giao dich hoan thanh")
                });

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            var transaction = Assert.Single(capturedTransactions);
            Assert.Equal("Giao dich hoan thanh", transaction.Description);
            Assert.Equal(order.TotalFee, transaction.TransferAmount);
        }

        // TEST CASE 13: transaction rỗng/null nhưng order vẫn báo hoàn thành -> fallback sang
        // thông tin của order, không được NullReferenceException.
        [Fact]
        public async Task BaoKimWebHookHandler_NullTransactionList_FallsBackToOrderFields()
        {
            // Arrange
            var order = CreateOrder(orderTime: 1700000000);
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
            var service = CreateService(dbContext, new HttpClient(handler.Object));

            var request = CreateCallback(order.Id, totalAmount: order.TotalFee);
            request.PaymentResult.Transaction = null!; // BaoKim gửi "transaction": null

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            var transaction = Assert.Single(capturedTransactions);
            Assert.Equal(order.TotalFee, transaction.TransferAmount); // fallback: order.total_amount
            // request_id nam o cap cao nhat nen van con du ngay ca khi transaction bi null.
            Assert.Equal("B2B6A278C9815BD7EQ4AVPVZCI", transaction.RequestId);
            Assert.Equal("Paid", order.Status);
        }

        // TEST CASE 15: merchant_fee lon hon gia tri don -> transaction.amount AM (-177600).
        // So lieu lay tu callback that: order.total_amount=160000, merchant_fee=337600,
        // amount=-177600. TransferAmount phai la so tien khach tra (160000), khong phai so tien
        // rong sau phi. Payload mau trong tai lieu co merchant_fee=0 nen amount trung voi tong
        // tien don — che mat khac biet nay.
        [Fact]
        public async Task BaoKimWebHookHandler_MerchantFeeExceedsOrder_StoresGrossNotNegativeNet()
        {
            // Arrange
            var order = CreateOrder(orderTime: 1700000000, totalFee: 160000m);
            var capturedTransactions = new List<TransactionEntity>();
            var dbContext = CreateMockDbContext(
                new List<OrderEntity> { order },
                new List<CompanyAddressEntity> { CreateCompanyAddress() },
                capturedTransactions).Object;

            var handler = CreateHttpHandler(AhamoveSuccess(), NhanhSuccess());
            var service = CreateService(dbContext, new HttpClient(handler.Object));

            var baoKimTransaction = CreateBaoKimTransaction(
                id: 1163292, status: 1, grossAmount: 160000m, merchantFee: 337600m, userFee: 0m);

            // Kiem tra lai gia dinh: BaoKim that su gui amount am.
            Assert.Equal(-177600m, baoKimTransaction.Amount);

            var request = CreateCallback(order.Id, totalAmount: 160000m,
                transactions: new List<BaoKimTransaction> { baoKimTransaction });

            // Act
            await service.BaoKimWebHookHandler(request);

            // Assert
            var transaction = Assert.Single(capturedTransactions);
            Assert.Equal(160000m, transaction.TransferAmount);
            Assert.True(transaction.TransferAmount > 0,
                "TransferAmount khong duoc am khi khach da thanh toan thanh cong");
            Assert.Equal("Paid", order.Status);
        }

        // TEST CASE 14: payment_result thiếu -> BadRequestException.
        [Fact]
        public async Task BaoKimWebHookHandler_MissingPaymentResult_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(
                new List<OrderEntity>(),
                new List<CompanyAddressEntity> { CreateCompanyAddress() }).Object;

            var service = CreateService(dbContext, EmptyHttpClient());

            var request = new BaoKimPaymentCallback
            {
                RequestId = "REQ-1",
                RequestTime = "2026-06-09 10:46:32",
                MerchantCode = "hoatheomua",
                Operation = "PAYMENT_TRANS",
                PaymentResult = null!
            };

            // Act
            var act = async () => await service.BaoKimWebHookHandler(request);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Missing payment_result.order", exception.Message);
        }
    }
}
