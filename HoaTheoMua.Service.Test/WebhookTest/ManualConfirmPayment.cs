using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Fulfillment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;
using WebhookServiceImpl = HoaTheoMua.Service.Webhook.Service;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.WebhookTest;

public class ManualConfirmPaymentTests
{
    [Fact]
    public async Task ManualConfirmPayment_PendingOrder_MarksPaidAndCreatesTransaction()
    {
        // Arrange: đơn chưa thanh toán, chưa có giao dịch.
        var order = CreateOrder("Pending");
        var transactions = new List<Transaction>();
        var dbContext = CreateDbContext([order], transactions);
        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint.Setup(x => x.Publish(It.IsAny<OrderPaid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(dbContext.Object, publishEndpoint.Object);

        // Act.
        await service.ManualConfirmPayment(order.Id);

        // Assert: cập nhật đơn, ghi đúng giao dịch và kích hoạt Saga một lần.
        Assert.Equal("Paid", order.Status);
        Assert.NotNull(order.UpdatedAt);
        var transaction = Assert.Single(transactions);
        Assert.Equal(order.Id, transaction.OrderId);
        Assert.Equal(order.TotalFee, transaction.TransferAmount);
        Assert.Equal($"manual:{order.Id:N}", transaction.RequestId);
        Assert.Equal(order.PaymentGateway.ToString(), transaction.Gateway);
        Assert.Equal(order.PaymentMethod, transaction.paymentMethod);
        Assert.Equal("Manual", transaction.TransferType);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        publishEndpoint.Verify(x => x.Publish(
            It.Is<OrderPaid>(message => message.OrderId == order.Id
                && message.TransactionId == transaction.Id
                && message.Amount == order.TotalFee),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManualConfirmPayment_PaidOrder_RepeatedCallsDoNotCreateTransaction()
    {
        // Arrange: đơn đã được xử lý thanh toán trước đó.
        var order = CreateOrder("Paid");
        var transactions = new List<Transaction>();
        var dbContext = CreateDbContext([order], transactions);
        var publishEndpoint = new Mock<IPublishEndpoint>(MockBehavior.Strict);
        var service = CreateService(dbContext.Object, publishEndpoint.Object);

        // Act: admin gọi lại nhiều lần trên đơn đã Paid.
        await service.ManualConfirmPayment(order.Id);
        await service.ManualConfirmPayment(order.Id);

        // Assert: không ghi thêm giao dịch, không lưu hay phát lại sự kiện.
        Assert.Equal("Paid", order.Status);
        Assert.Empty(transactions);
        dbContext.Verify(x => x.Transactions.Add(It.IsAny<Transaction>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        publishEndpoint.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ManualConfirmPayment_OrderNotFound_ThrowsNotFoundException()
    {
        // Arrange: database không có đơn cần xác nhận.
        var transactions = new List<Transaction>();
        var dbContext = CreateDbContext([], transactions);
        var publishEndpoint = new Mock<IPublishEndpoint>(MockBehavior.Strict);
        var service = CreateService(dbContext.Object, publishEndpoint.Object);

        // Act.
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.ManualConfirmPayment(Guid.NewGuid()));

        // Assert: báo đúng lỗi và không phát sinh dữ liệu thanh toán.
        Assert.Equal("Không tìm thấy đơn hàng", exception.Message);
        Assert.Empty(transactions);
        dbContext.Verify(x => x.Transactions.Add(It.IsAny<Transaction>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        publishEndpoint.VerifyNoOtherCalls();
    }

    private static OrderEntity CreateOrder(string status) => new()
    {
        Id = Guid.NewGuid(),
        Ward = 1,
        City = 1,
        Address = "123 Đường Test",
        ShortAddress = "Địa chỉ test",
        FullName = "Khách hàng test",
        Phone = "0901234567",
        FullNameReceiver = "Người nhận test",
        PhoneReceiver = "0901234567",
        Remarks = string.Empty,
        TrackNumber = string.Empty,
        Status = status,
        OrderTime = 0,
        TotalFee = 100000m,
        PaymentGateway = PaymentGateway.Sepay,
        PaymentMethod = PaymentMethod.VNPayQR,
        ShippingGateway = ShippingGateway.Ahamove
    };

    private static Mock<AppDbContext> CreateDbContext(
        List<OrderEntity> orders, List<Transaction> transactions)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var dbContext = new Mock<AppDbContext>(options);
        dbContext.Setup(x => x.Orders).Returns(orders.BuildMockDbSet().Object);
        dbContext.Setup(x => x.OrderDetails)
            .Returns(new List<OrderDetails>().BuildMockDbSet().Object);
        dbContext.Setup(x => x.Configs)
            .Returns(new List<ConfigEntity>().BuildMockDbSet().Object);

        var transactionSet = transactions.BuildMockDbSet();
        transactionSet.Setup(x => x.Add(It.IsAny<Transaction>()))
            .Callback<Transaction>(transaction => transactions.Add(transaction));
        dbContext.Setup(x => x.Transactions).Returns(transactionSet.Object);
        dbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        return dbContext;
    }

    private static WebhookServiceImpl CreateService(
        AppDbContext dbContext, IPublishEndpoint publishEndpoint) => new(
        dbContext,
        new ConfigurationBuilder().Build(),
        new Mock<HoaTheoMua.Service.Fulfillment.IService>(MockBehavior.Strict).Object,
        publishEndpoint);
}
