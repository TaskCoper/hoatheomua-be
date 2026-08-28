using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
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
using Moq.Protected;
using Xunit;
using OrderServiceImpl = HoaTheoMua.Service.Order.Service;
using ProductService = HoaTheoMua.Service.Product.IService;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using OrderDetailEntity = HoaTheoMua.Repository.Entity.OrderDetails;
using TransactionEntity = HoaTheoMua.Repository.Entity.Transaction;

namespace HoaTheoMua.Service.Test.OrderTest
{
    public class GetStatusOrderTests
    {
        private static readonly Guid TestUserId = Guid.Parse("cba7c21c-d6f2-4b30-9b4e-8c7c10b4f0b2");

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static OrderEntity CreateOrder(
            Guid? id = null,
            Guid? userId = null,
            string status = "Pending")
        {
            return new OrderEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? TestUserId,
                Status = status,
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = false,
                TrackNumber = "",
                Remarks = "",
                OrderTime = 0,
                Ward = 1,
                City = 1,
                Address = "Test Address",
                ShortAddress = "Test Short Address",
                FullName = "Test Full Name",
                Phone = "0987654321",
                PhoneReceiver = "0987654321",
                FullNameReceiver = "Test Receiver",
                OrderDetails = new List<OrderDetailEntity>(),
                Transactions = new List<TransactionEntity>()
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
                new HttpContextAccessor(),
                new HttpClient(),
                mockProductService.Object,
                mockConfig.Object,
                new Mock<HoaTheoMua.Service.BaoKim.IService>().Object);
        }

        private static ICurrentUserService CreateMockCurrentUserService(Guid userId, bool isVerified = true)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(x => x.GetRequiredUserId()).Returns(userId);
            mock.Setup(x => x.IsVerified).Returns(isVerified);
            return mock.Object;
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

        // ---------------------------------------------------------------------
        // TEST CASE 01: Lấy trạng thái order thành công
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetStatusOrder_ValidRequest_ReturnsOrderStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: TestUserId, status: "Paid");
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetStatusOrder(orderId);

            // Assert
            Assert.Equal("Paid", result);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 02: Không tìm thấy UserId trong token -> UnauthorizedException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetStatusOrder_NoUserIdInToken_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.GetRequiredUserId()).Throws(new UnauthorizedException("User not found"));
            currentUserMock.Setup(x => x.IsVerified).Returns(true);
            var service = CreateService(dbContext, currentUserMock.Object);

            // Act
            var act = async () => await service.GetStatusOrder(Guid.NewGuid());

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(act);
            Assert.Equal("User not found", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 03: UserId trong token sai định dạng Guid -> UnauthorizedException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetStatusOrder_UserIdInvalidGuid_ThrowsUnauthorizedException()
        {
            // Arrange: a real CurrentUserService whose "UserId" claim is not a valid Guid.
            // Guid.TryParse fails, so GetRequiredUserId() throws UnauthorizedException
            // (the service never sees a FormatException).
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUser = CreateRealCurrentUserService("not-a-valid-guid");
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.GetStatusOrder(Guid.NewGuid()));
            Assert.Equal("User not found", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 04: User chưa verify -> UnauthorizedException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetStatusOrder_UserNotVerified_ThrowsUnauthorizedException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: TestUserId);
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId, isVerified: false);
            var service = CreateService(dbContext, currentUser);

            // Act
            var act = async () => await service.GetStatusOrder(orderId);

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(act);
            Assert.Equal("User is not verified", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 05: Order không tồn tại -> NotFoundException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetStatusOrder_OrderNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var act = async () => await service.GetStatusOrder(orderId);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal($"Order ID {orderId} not found", exception.Message);
        }

        // ---------------------------------------------------------------------
        // TEST CASE 06: Order thuộc user khác -> NotFoundException
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetStatusOrder_OrderBelongsToOtherUser_ThrowsNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var order = CreateOrder(id: orderId, userId: otherUserId);
            var dbContext = CreateMockDbContext(new List<OrderEntity> { order }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var act = async () => await service.GetStatusOrder(orderId);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal($"Order ID {orderId} not found", exception.Message);
        }
    }
}
