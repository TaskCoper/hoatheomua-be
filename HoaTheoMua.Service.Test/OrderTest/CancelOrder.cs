// using System.Net;
// using System.Threading;
// using HoaTheoMua.Repository;
// using HoaTheoMua.Service.Common;
// using HoaTheoMua.Service.Exceptions;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using MockQueryable.Moq;
// using Moq;
// using Moq.Protected;
// using Xunit;
// using OrderServiceImpl = HoaTheoMua.Service.Order.Service;
// using OrderEntity = HoaTheoMua.Repository.Entity.Order;
// using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
// using ProductService = HoaTheoMua.Service.Product.IService;
//
// namespace HoaTheoMua.Service.Test.OrderTest
// {
//     public class CancelOrder
//     {
//
//         private static OrderEntity CreateOrderEntity(Guid id, string status = "Pending")
//         {
//             return new OrderEntity
//             {
//                 Id = id,
//                 UserId = Guid.NewGuid(),
//                 Ward = 1,
//                 City = 1,
//                 Address = "123 Duong Test",
//                 ShortAddress = "123 Duong Test, Q1, HCM",
//                 FullName = "Nguyen Van A",
//                 Phone = "0987654321",
//                 Remarks = "Giao gio hanh chinh",
//                 Status = status,
//                 TrackNumber = "TRACK-001",
//                 OrderTime = 0,
//                 TotalFee = 100000m,
//                 CreatedAt = DateTimeOffset.UtcNow
//             };
//         }
//
//         // Mock AppDbContext chỉ với DbSet Orders (CancelOrder chỉ dùng tới Orders).
//         private static Mock<AppDbContext> CreateMockDbContext(List<OrderEntity> orders)
//         {
//             var mockOrders = orders.BuildMockDbSet();
//             mockOrders.Setup(x => x.Update(It.IsAny<OrderEntity>()))
//                 .Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<OrderEntity>)null!);
//
//             var configs = new List<ConfigEntity>
//             {
//                 new ConfigEntity { Key = "nhanhvn.token", Value = "{\"value\": \"test_token\"}", Group = "nhanhvn", IsDeleted = false },
//                 new ConfigEntity { Key = "ahamove.api_key", Value = "{\"value\": \"test_api_key\"}", Group = "ahamove", IsDeleted = false }
//             };
//             var mockConfigs = configs.BuildMockDbSet();
//
//             var options = new DbContextOptionsBuilder<AppDbContext>().Options;
//             var mockDbContext = new Mock<AppDbContext>(options);
//
//             mockDbContext.Setup(x => x.Orders).Returns(mockOrders.Object);
//             mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);
//             mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
//
//             return mockDbContext;
//         }
//
//         private static OrderServiceImpl CreateService(AppDbContext dbContext)
//         {
//             var mockConfig = new Mock<IConfiguration>();
//             var mockConfigSection = new Mock<IConfigurationSection>();
//             mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
//             mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);
//
//             var handler = new Mock<HttpMessageHandler>();
//             handler.Protected()
//                 .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
//                 .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
//             var httpClient = new HttpClient(handler.Object);
//
//             var currentUser = new Mock<ICurrentUserService>().Object;
//             var productService = new Mock<ProductService>().Object;
//
//             return new OrderServiceImpl(dbContext, currentUser, httpClient, productService, mockConfig.Object, new Mock<HoaTheoMua.Service.BaoKim.IService>().Object);
//         }
//         
//         // HAPPY CASE: Đơn đang Pending => hủy thành công, status đổi thành Cancelled
//         [Fact]
//         public async Task CancelOrder_PendingOrder_CancelsSuccessfully()
//         {
//             // Arrange
//             var orderId = Guid.NewGuid();
//             var order = CreateOrderEntity(orderId, status: "Pending");
//             var dbContextMock = CreateMockDbContext(new List<OrderEntity> { order });
//             var service = CreateService(dbContextMock.Object);
//
//             // Act
//             var result = await service.CancelOrder(orderId);
//
//             // Assert
//             Assert.Equal("Hủy đơn thành công", result);
//             Assert.Equal("Cancelled", order.Status);
//             Assert.NotNull(order.UpdatedAt);
//             dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         }
//         
//         // EDGE CASE 1: Không tồn tại đơn với Id truyền vào => BadRequestException
//         [Fact]
//         public async Task CancelOrder_OrderNotFound_ThrowsBadRequestException()
//         {
//             // Arrange
//             var dbContextMock = CreateMockDbContext(new List<OrderEntity>());
//             var service = CreateService(dbContextMock.Object);
//
//             // Act
//             var act = async () => await service.CancelOrder(Guid.NewGuid());
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Equal("Order not found", exception.Message);
//         }
//         
//         // EDGE CASE 2: Đơn tồn tại nhưng status != "Pending" => BadRequestException
//         // (query lọc theo Status == "Pending" nên coi như không tìm thấy)
//         [Fact]
//         public async Task CancelOrder_OrderNotPending_ThrowsBadRequestException()
//         {
//             // Arrange
//             var orderId = Guid.NewGuid();
//             var order = CreateOrderEntity(orderId, status: "Cancelled");
//             var dbContextMock = CreateMockDbContext(new List<OrderEntity> { order });
//             var service = CreateService(dbContextMock.Object);
//
//             // Act
//             var act = async () => await service.CancelOrder(orderId);
//
//             // Assert
//             var exception = await Assert.ThrowsAsync<BadRequestException>(act);
//             Assert.Equal("Order not found", exception.Message);
//             dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//         }
//     }
// }
