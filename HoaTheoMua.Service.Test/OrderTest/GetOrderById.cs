using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;
using OrderServiceImpl = HoaTheoMua.Service.Order.Service;
using ProductService = HoaTheoMua.Service.Product.IService;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using OrderDetailEntity = HoaTheoMua.Repository.Entity.OrderDetails;
using TransactionEntity = HoaTheoMua.Repository.Entity.Transaction;

namespace HoaTheoMua.Service.Test.OrderTest
{
    public class GetOrderByIdTests
    {
        private static readonly Guid TestUserId = Guid.Parse("cba7c21c-d6f2-4b30-9b4e-8c7c10b4f0b2");

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static OrderEntity CreateOrder(
            Guid? id = null,
            Guid? userId = null,
            bool isDeleted = false,
            decimal totalFee = 100000m,
            List<OrderDetailEntity>? details = null,
            List<TransactionEntity>? transactions = null)
        {
            var orderId = id ?? Guid.NewGuid();
            
            return new OrderEntity
            {
                Id = orderId,
                UserId = userId ?? TestUserId,
                Status = "Pending",
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = isDeleted,
                TrackNumber = "TRACK-001",
                Remarks = "Test Remark",
                OrderTime = 1782234000,
                Ward = 1,
                City = 1,
                Address = "Test Address",
                ShortAddress = "Test Short Address",
                FullName = "Test Full Name",
                Phone = "0987654321",
                TotalFee = totalFee,
                OrderDetails = details ?? new List<OrderDetailEntity>(),
                Transactions = transactions ?? new List<TransactionEntity>(),
                PaymentUrl = $"https://qr.sepay.vn/img?acc=96247BENTRAN&bank=BIDV&amount=782000&des=HTM-{orderId}&template=qronly"
            };
        }

        // Transaction has a wide set of required members that these tests never assert on; this
        // keeps each case down to the few fields it actually cares about.
        private static TransactionEntity CreateTransaction(
            Guid orderId,
            decimal transferAmount,
            string code,
            string gateway,
            DateTime transactionDate,
            DateTimeOffset createdAt)
        {
            return new TransactionEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                RequestId = $"REQ-{code}",
                Gateway = gateway,
                Code = code,
                TransferAmount = transferAmount,
                TransactionDate = transactionDate,
                CreatedAt = createdAt,
                AccountNumber = string.Empty,
                SubAccount = string.Empty,
                Content = string.Empty,
                TransferType = string.Empty,
                Description = string.Empty,
                ReferenceCode = string.Empty
            };
        }

        private static Mock<AppDbContext> CreateMockDbContext(List<OrderEntity> orders)
        {
            var mockOrders = orders.BuildMockDbSet();
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);
            mockDbContext.Setup(x => x.Orders).Returns(mockOrders.Object);
            return mockDbContext;
        }

        private static OrderServiceImpl CreateService(AppDbContext dbContext, ICurrentUserService currentUser)
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            var mockProductService = new Mock<ProductService>();
            return new OrderServiceImpl(
                dbContext,
                currentUser,
                new HttpClient(),
                mockProductService.Object,
                mockConfig.Object,
                new Mock<HoaTheoMua.Service.BaoKim.IService>().Object);
        }

        private static ICurrentUserService CreateMockCurrentUserService(Guid userId)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(x => x.GetRequiredUserId()).Returns(userId);
            mock.Setup(x => x.IsVerified).Returns(true);
            return mock.Object;
        }

        // ---------------------------------------------------------------------
        // TEST CASES
        // ---------------------------------------------------------------------

        // Test case 01: Lấy chi tiết order thành công
        [Fact]
        public async Task GetOrderById_ValidRequest_ReturnsOrderDetailsSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var details = new List<OrderDetailEntity>
            {
                new OrderDetailEntity { Id = Guid.NewGuid(), Name = "Product A", ItemId = 101, Number = 2, Price = 50000m, Height = 10, Weight = 1, Length = 10, Width = 10 }
            };
            var transactions = new List<TransactionEntity>
            {
                CreateTransaction(orderId, 100000m, "TX-001", "Sepay", DateTime.UtcNow, DateTime.UtcNow)
            };
            var order = CreateOrder(id: orderId, userId: TestUserId, details: details, transactions: transactions);
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var response = await service.GetOrderById(orderId);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            var orderResult = Assert.IsType<HoaTheoMua.Service.Order.Response.GetOrderByIdResponse>(response.Value);
            Assert.Equal(orderId, orderResult.OrderId);
            Assert.Equal("Pending", orderResult.Status);
            Assert.Equal("TRACK-001", orderResult.TrackNumber);
            Assert.Equal("Test Remark", orderResult.Remark);
            Assert.Equal(1782234000, orderResult.OrderTime);
            Assert.Single(orderResult.Details);
            Assert.Equal(101, orderResult.Details[0].ItemId);
            Assert.NotNull(orderResult.Transaction);
            Assert.Equal(100000m, orderResult.Transaction.TransferAmount);
        }

        // Test case 02: Không tìm thấy UserId trong token
        [Fact]
        public async Task GetOrderById_NoUserIdInToken_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.GetRequiredUserId()).Throws(new UnauthorizedException("User not found"));
            var service = CreateService(dbContext, currentUserMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetOrderById(Guid.NewGuid()));
            Assert.Equal("User not found", exception.Message);
        }

        // Builds the real CurrentUserService over a mocked HttpContext so the actual
        // claim-parsing logic runs (Guid.TryParse), instead of stubbing the interface.
        private static ICurrentUserService CreateRealCurrentUserService(string? userIdClaim)
        {
            var claims = userIdClaim is null
                ? new List<Claim>()
                : new List<Claim> { new Claim("UserId", userIdClaim) };
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            };
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(x => x.HttpContext).Returns(httpContext);
            return new CurrentUserService(accessor.Object);
        }

        // Test case 03: UserId sai định dạng Guid -> UnauthorizedException
        [Fact]
        public async Task GetOrderById_UserIdInvalidGuid_ThrowsUnauthorizedException()
        {
            // Arrange: a real CurrentUserService whose "UserId" claim is not a valid Guid.
            // Guid.TryParse fails, so GetRequiredUserId() throws UnauthorizedException
            // (the service never sees a FormatException).
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUser = CreateRealCurrentUserService("not-a-valid-guid");
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.GetOrderById(Guid.NewGuid()));
            Assert.Equal("User not found", exception.Message);
        }

        // Test case 04: Order không tồn tại
        [Fact]
        public async Task GetOrderById_OrderNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetOrderById(Guid.NewGuid()));
            Assert.Equal("Order not found ", exception.Message);
        }

        // Test case 05: Order thuộc user khác
        [Fact]
        public async Task GetOrderById_OrderBelongsToOtherUser_ThrowsNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: Guid.NewGuid()); // Different user ID
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetOrderById(orderId));
            Assert.Equal("Order not found ", exception.Message);
        }

        // Test case 06: Order đã bị xóa mềm
        [Fact]
        public async Task GetOrderById_OrderIsDeleted_ThrowsNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: TestUserId, isDeleted: true);
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetOrderById(orderId));
            Assert.Equal("Order not found ", exception.Message);
        }

        // Test case 07: Tạo QR code URL chính xác
        [Fact]
        public async Task GetOrderById_GeneratesCorrectQrCodeUrl()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: TestUserId, totalFee: 782000m);
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var response = await service.GetOrderById(orderId);

            // Assert
            Assert.NotNull(response);
            var orderResult = Assert.IsType<HoaTheoMua.Service.Order.Response.GetOrderByIdResponse>(response.Value);
            var expectedQrUrl = $"https://qr.sepay.vn/img?acc=96247BENTRAN&bank=BIDV&amount=782000&des=HTM-{orderId}&template=qronly";
            Assert.Equal(expectedQrUrl, orderResult.PaymentUrl);
        }

        // Test case 08: Order chưa có transaction
        [Fact]
        public async Task GetOrderById_NoTransaction_ReturnsNullTransaction()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: TestUserId, transactions: new List<TransactionEntity>());
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var response = await service.GetOrderById(orderId);

            // Assert
            Assert.NotNull(response);
            var orderResult = Assert.IsType<HoaTheoMua.Service.Order.Response.GetOrderByIdResponse>(response.Value);
            Assert.Null(orderResult.Transaction);
        }

        // Test case 09: Order có nhiều transaction
        [Fact]
        public async Task GetOrderById_MultipleTransactions_ReturnsLatestTransaction()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var oldTx = CreateTransaction(
                orderId, 50000m, "TX-OLD", "Gateway", now.AddMinutes(-10), now.AddMinutes(-10));
            var newTx = CreateTransaction(
                orderId, 100000m, "TX-NEW", "Gateway", now, now);
            var order = CreateOrder(id: orderId, userId: TestUserId, transactions: new List<TransactionEntity> { oldTx, newTx });
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var response = await service.GetOrderById(orderId);

            // Assert
            Assert.NotNull(response);
            var orderResult = Assert.IsType<HoaTheoMua.Service.Order.Response.GetOrderByIdResponse>(response.Value);
            Assert.NotNull(orderResult.Transaction);
            Assert.Equal("TX-NEW", orderResult.Transaction.Code);
            Assert.Equal(100000m, orderResult.Transaction.TransferAmount);
        }
    }
}
