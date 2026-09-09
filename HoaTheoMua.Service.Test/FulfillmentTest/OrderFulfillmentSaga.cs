using System;
using System.Threading;
using System.Threading.Tasks;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Fulfillment;
using HoaTheoMua.Service.Fulfillment.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace HoaTheoMua.Service.Test.FulfillmentTest;

/// <summary>
/// Drives the real state machine and the real consumers over the in-memory transport, with only
/// Fulfillment.IService mocked. That is the seam worth testing: the routing and the compensation
/// decisions, not the HTTP calls (those are covered by the service's own tests).
/// </summary>
public class OrderFulfillmentSagaTest
{
    private static readonly DeliveryDispatchResult DispatchedPlan = new()
    {
        Dispatched = true,
        ShippingGateway = ShippingGateway.Ahamove,
        ShippingPartyOrderId = "AHA-123",
        ShippingTrackingUrl = "https://track/AHA-123",
        DelayTimeUnix = 1_800_000_000,
        TagIds = [294L],
        PrivateDescription = "pick-up note"
    };

    // Scheduled order: Ahamove is deliberately never called, so nothing exists to compensate.
    private static readonly DeliveryDispatchResult ScheduledPlan = new()
    {
        Dispatched = false,
        ShippingGateway = ShippingGateway.Ahamove,
        DelayTimeUnix = 0,
        TagIds = [],
        PrivateDescription = "pick-up note"
    };

    private static ServiceProvider BuildProvider(Mock<IService> fulfillment) =>
        new ServiceCollection()
            .AddSingleton(fulfillment.Object)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumersFromNamespaceContaining<DispatchDeliveryConsumer>();
                cfg.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

    private static Mock<IService> HappyFulfillment(DeliveryDispatchResult? plan = null)
    {
        var mock = new Mock<IService>();
        mock.Setup(x => x.DispatchDeliveryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan ?? DispatchedPlan);
        mock.Setup(x => x.CreatePosOrderAsync(It.IsAny<Guid>(), It.IsAny<DeliveryDispatchResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PosOrderResult { NhanhOrderId = 987, TrackingUrl = "https://nhanh/987" });
        mock.Setup(x => x.RevalidateOrderProductsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.CompensateAsync(It.IsAny<Guid>(), It.IsAny<ShippingGateway?>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompensationOutcome { DeliveryCleared = true, PosOrderCleared = true, Details = "cancelled" });
        mock.Setup(x => x.MarkFulfillmentFailedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static OrderPaid PaidFor(Guid orderId) => new()
    {
        OrderId = orderId,
        TransactionId = Guid.NewGuid(),
        Gateway = "BaoKim",
        RequestId = "REQ-1",
        Amount = 160_000m
    };

    // TC01: happy path chạy hết 3 bước và dừng ở Fulfilled
    [Fact]
    public async Task HappyPath_RunsAllThreeSteps_AndEndsFulfilled()
    {
        var fulfillment = HappyFulfillment();
        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(PaidFor(orderId));

        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();
        Assert.True(await saga.Consumed.Any<OrderPaid>());
        Assert.True(await harness.Published.Any<CacheRevalidated>());

        var instanceId = await saga.Exists(orderId, x => x.Fulfilled);
        Assert.NotNull(instanceId);

        var instance = saga.Sagas.Contains(orderId);
        Assert.Equal("AHA-123", instance.ShippingPartyOrderId);
        Assert.Equal(987, instance.NhanhOrderId);
        Assert.False(await harness.Published.Any<CompensateFulfillment>());
    }

    // TC02: NhanhVn hỏng sau khi Ahamove đã đặt xe -> phải bù trừ rồi mới Failed
    [Fact]
    public async Task PosOrderFails_AfterCourierBooked_CompensatesThenFails()
    {
        var fulfillment = HappyFulfillment();
        fulfillment.Setup(x => x.CreatePosOrderAsync(It.IsAny<Guid>(), It.IsAny<DeliveryDispatchResult>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulfillmentPermanentException("NhanhVn rejected the order"));

        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(PaidFor(orderId));

        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();
        Assert.NotNull(await saga.Exists(orderId, x => x.Failed));

        // Bù trừ phải mang đúng id đơn Ahamove đã đặt - đây chính là thông tin bản cũ làm mất.
        Assert.True(await harness.Published.Any<CompensateFulfillment>(
            x => x.Context.Message.ShippingPartyOrderId == "AHA-123"));
        fulfillment.Verify(x => x.CompensateAsync(orderId, ShippingGateway.Ahamove, "AHA-123", null, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        var instance = saga.Sagas.Contains(orderId);
        Assert.Equal(nameof(CreatePosOrder), instance.FailedStep);
        // Đúng 1 vòng bù trừ: phát lệnh một lần và nó chạy xong ngay, không phải phát lại.
        Assert.Equal(1, instance.CompensationAttempts);
    }

    // TC03: đơn hẹn giờ chưa từng gọi Ahamove -> Failed thẳng, KHÔNG bù trừ
    [Fact]
    public async Task PosOrderFails_OnScheduledOrder_FailsWithoutCompensating()
    {
        var fulfillment = HappyFulfillment(ScheduledPlan);
        fulfillment.Setup(x => x.CreatePosOrderAsync(It.IsAny<Guid>(), It.IsAny<DeliveryDispatchResult>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulfillmentPermanentException("NhanhVn rejected the order"));

        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(PaidFor(orderId));

        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();
        Assert.NotNull(await saga.Exists(orderId, x => x.Failed));

        Assert.False(await harness.Published.Any<CompensateFulfillment>());
        fulfillment.Verify(x => x.CompensateAsync(It.IsAny<Guid>(), It.IsAny<ShippingGateway?>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Không có vòng bù trừ nào được phát -> 0, phân biệt rõ với trường hợp có bù trừ.
        Assert.Equal(0, saga.Sagas.Contains(orderId).CompensationAttempts);
    }

    // TC04: Ahamove hỏng ngay bước đầu -> chưa tạo gì bên ngoài nên không bù trừ
    [Fact]
    public async Task DispatchFails_FailsWithoutCompensating()
    {
        var fulfillment = HappyFulfillment();
        fulfillment.Setup(x => x.DispatchDeliveryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulfillmentTransientException("Ahamove unreachable"));

        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(PaidFor(orderId));

        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();
        Assert.NotNull(await saga.Exists(orderId, x => x.Failed));

        Assert.False(await harness.Published.Any<CompensateFulfillment>());
        Assert.Equal(nameof(DispatchDelivery), saga.Sagas.Contains(orderId).FailedStep);
    }

    // TC05: purge cache hỏng KHÔNG được hủy chuyến giao hàng có thật
    [Fact]
    public async Task RevalidateFails_StillEndsFulfilled_AndDoesNotCompensate()
    {
        var fulfillment = HappyFulfillment();
        fulfillment.Setup(x => x.RevalidateOrderProductsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulfillmentTransientException("storefront down"));

        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(PaidFor(orderId));

        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();
        Assert.NotNull(await saga.Exists(orderId, x => x.Fulfilled));
        Assert.False(await harness.Published.Any<CompensateFulfillment>());
    }

    // TC06: bù trừ fault liên tục -> saga phát lại đúng 3 vòng rồi bỏ cuộc, KHÔNG spin vô hạn
    [Fact]
    public async Task CompensationKeepsFaulting_StopsAtTheRoundCap()
    {
        var fulfillment = HappyFulfillment();
        fulfillment.Setup(x => x.CreatePosOrderAsync(It.IsAny<Guid>(), It.IsAny<DeliveryDispatchResult>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulfillmentPermanentException("NhanhVn rejected the order"));
        fulfillment.Setup(x => x.CompensateAsync(It.IsAny<Guid>(), It.IsAny<ShippingGateway?>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulfillmentTransientException("Ahamove unreachable"));

        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(PaidFor(orderId));

        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();

        // Vẫn phải tới được Failed - bù trừ hỏng không được phép treo đơn ở Compensating mãi.
        Assert.NotNull(await saga.Exists(orderId, x => x.Failed));

        var instance = saga.Sagas.Contains(orderId);
        Assert.Equal(3, instance.CompensationAttempts);
        Assert.Equal(3, await harness.Published.SelectAsync<CompensateFulfillment>().Count());
        Assert.Contains("compensation round", instance.FailureReason);
    }

    // TC07: OrderPaid gửi lại lần hai chỉ tương quan vào saga cũ, không chạy fulfilment lần nữa
    [Fact]
    public async Task DuplicateOrderPaid_DoesNotRunFulfilmentTwice()
    {
        var fulfillment = HappyFulfillment();
        await using var provider = BuildProvider(fulfillment);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var saga = harness.GetSagaStateMachineHarness<OrderFulfillmentStateMachine, OrderFulfillmentState>();

        await harness.Bus.Publish(PaidFor(orderId));
        Assert.NotNull(await saga.Exists(orderId, x => x.Fulfilled));

        await harness.Bus.Publish(PaidFor(orderId));
        Assert.True(await saga.Consumed.SelectAsync<OrderPaid>().Count() >= 2);

        fulfillment.Verify(x => x.DispatchDeliveryAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        fulfillment.Verify(x => x.CreatePosOrderAsync(orderId, It.IsAny<DeliveryDispatchResult>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
