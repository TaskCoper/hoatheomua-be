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

namespace HoaTheoMua.Service.Test.OrderTest
{
    public class GetOrderPaginationResponseTests
    {
        private static readonly Guid TestUserId = Guid.Parse("cba7c21c-d6f2-4b30-9b4e-8c7c10b4f0b2");

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static OrderEntity CreateOrder(
            Guid? id = null,
            Guid? userId = null,
            bool isDeleted = false,
            DateTimeOffset? createdAt = null)
        {
            return new OrderEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? TestUserId,
                PaymentGateway = HoaTheoMua.Repository.Enum.PaymentGateway.BaoKim,
                ShippingGateway = HoaTheoMua.Repository.Enum.ShippingGateway.Ahamove,
                Status = "Pending",
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
                IsDeleted = isDeleted,
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
                OrderDetails = new List<OrderDetailEntity>()
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

        // Test case 01: Lấy danh sách order thành công
        [Fact]
        public async Task GetOrderPaginationResponse_ValidRequest_ReturnsPaginationResult()
        {
            // Arrange
            var orders = new List<OrderEntity> { CreateOrder(), CreateOrder() };
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 10);

            // Assert
            Assert.NotNull(result);
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Order.Response.GetOrderResponse>();
            Assert.Equal(2, items.Count());
            Assert.Equal(1, result.Value.PageIndex);
            Assert.Equal(10, result.Value.PageSize);
            Assert.Equal(2, result.Value.TotalCount);
            Assert.False(result.Value.HasNextPage);
            Assert.False(result.Value.HasPreviousPage);
        }

        // Test case 02: Không tìm thấy UserId trong token
        [Fact]
        public async Task GetOrderPaginationResponse_NoUserIdInToken_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.GetRequiredUserId()).Throws(new UnauthorizedException("User not found"));
            var service = CreateService(dbContext, currentUserMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetOrderPaginationResponse(1, 10));
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

        // Test case 03: UserId trong token sai định dạng Guid -> UnauthorizedException
        [Fact]
        public async Task GetOrderPaginationResponse_UserIdInvalidGuid_ThrowsUnauthorizedException()
        {
            // Arrange: a real CurrentUserService whose "UserId" claim is not a valid Guid.
            // Guid.TryParse fails, so GetRequiredUserId() throws UnauthorizedException
            // (the service never sees a FormatException).
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUser = CreateRealCurrentUserService("not-a-valid-guid");
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => service.GetOrderPaginationResponse(1, 10));
            Assert.Equal("User not found", exception.Message);
        }

        // Test case 04: pageIndex nhỏ hơn hoặc bằng 0 (Tự động gán = 1)
        [Fact]
        public async Task GetOrderPaginationResponse_PageIndexLessThanOrEqualZero_DefaultsToOne()
        {
            // Arrange
            var orders = new List<OrderEntity> { CreateOrder() };
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(0, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Value.PageIndex);
        }

        // Test case 05: pageSize nhỏ hơn hoặc bằng 0 (Tự động gán = 10)
        [Fact]
        public async Task GetOrderPaginationResponse_PageSizeLessThanOrEqualZero_DefaultsToTen()
        {
            // Arrange
            var orders = new List<OrderEntity> { CreateOrder() };
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Value.PageSize);
        }

        // Test case 06: pageSize lớn hơn 100 (Tự động gán = 100)
        [Fact]
        public async Task GetOrderPaginationResponse_PageSizeGreaterThanOneHundred_ClampsToOneHundred()
        {
            // Arrange
            var orders = new List<OrderEntity>();
            for (int i = 0; i < 110; i++)
            {
                orders.Add(CreateOrder());
            }
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 150);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Value.PageSize);
        }

        // Test case 07: Không có order nào
        [Fact]
        public async Task GetOrderPaginationResponse_NoOrders_ReturnsEmptyList()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<OrderEntity>()).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 10);

            // Assert
            Assert.NotNull(result);
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Order.Response.GetOrderResponse>();
            Assert.Empty(items);
            Assert.Equal(0, result.Value.TotalCount);
        }

        // Test case 08: Không lấy order đã bị xóa mềm
        [Fact]
        public async Task GetOrderPaginationResponse_ExcludesDeletedOrders()
        {
            // Arrange
            var orders = new List<OrderEntity>
            {
                CreateOrder(isDeleted: false),
                CreateOrder(isDeleted: true)
            };
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 10);

            // Assert
            Assert.NotNull(result);
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Order.Response.GetOrderResponse>();
            Assert.Single(items);
            Assert.Equal(1, result.Value.TotalCount);
        }

        // Test case 09: Không lấy order của user khác
        [Fact]
        public async Task GetOrderPaginationResponse_ExcludesOtherUsersOrders()
        {
            // Arrange
            var orders = new List<OrderEntity>
            {
                CreateOrder(userId: TestUserId),
                CreateOrder(userId: Guid.NewGuid()) // Other user
            };
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 10);

            // Assert
            Assert.NotNull(result);
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Order.Response.GetOrderResponse>();
            Assert.Single(items);
            Assert.Equal(1, result.Value.TotalCount);
        }

        // Test case 10: Order được sắp xếp theo CreatedAt giảm dần
        [Fact]
        public async Task GetOrderPaginationResponse_OrdersSortedByCreatedAtDescending()
        {
            // Arrange
            var baseTime = DateTimeOffset.UtcNow;
            var orderOlderId = Guid.NewGuid();
            var orderNewerId = Guid.NewGuid();
            var orders = new List<OrderEntity>
            {
                CreateOrder(id: orderOlderId, createdAt: baseTime.AddHours(-1)),
                CreateOrder(id: orderNewerId, createdAt: baseTime)
            };
            var dbContext = CreateMockDbContext(orders).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetOrderPaginationResponse(1, 10);

            // Assert
            Assert.NotNull(result);
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Order.Response.GetOrderResponse>().ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal(orderNewerId, items[0].OrderId); // Newer order comes first
            Assert.Equal(orderOlderId, items[1].OrderId);
        }
    }
}
