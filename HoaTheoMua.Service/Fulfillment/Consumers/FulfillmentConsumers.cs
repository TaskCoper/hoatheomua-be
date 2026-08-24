using MassTransit;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.Fulfillment.Consumers;

// Thin by design - the same rule the controllers follow. All the work lives in Fulfillment.Service
// so it stays unit-testable without a bus, and so the legacy inline path can call the same code.
//
// Each endpoint runs UseEntityFrameworkOutbox, so the service's own SaveChanges and the message
// published here commit in one transaction: an order id can never be persisted without its
// follow-up message, nor the message sent without the id.

public class DispatchDeliveryConsumer : IConsumer<DispatchDelivery>
{
    private readonly IService _fulfillment;

    public DispatchDeliveryConsumer(IService fulfillment) => _fulfillment = fulfillment;

    public async Task Consume(ConsumeContext<DispatchDelivery> context)
    {
        var plan = await _fulfillment.DispatchDeliveryAsync(context.Message.OrderId, context.CancellationToken);

        await context.Publish(new DeliveryDispatched
        {
            OrderId = context.Message.OrderId,
            Plan = plan
        });
    }
}

public class CreatePosOrderConsumer : IConsumer<CreatePosOrder>
{
    private readonly IService _fulfillment;

    public CreatePosOrderConsumer(IService fulfillment) => _fulfillment = fulfillment;

    public async Task Consume(ConsumeContext<CreatePosOrder> context)
    {
        var result = await _fulfillment.CreatePosOrderAsync(
            context.Message.OrderId, context.Message.Plan, context.CancellationToken);

        await context.Publish(new PosOrderCreated
        {
            OrderId = context.Message.OrderId,
            NhanhOrderId = result.NhanhOrderId,
            TrackingUrl = result.TrackingUrl
        });
    }
}

public class RevalidateProductCacheConsumer : IConsumer<RevalidateProductCache>
{
    private readonly IService _fulfillment;

    public RevalidateProductCacheConsumer(IService fulfillment) => _fulfillment = fulfillment;

    public async Task Consume(ConsumeContext<RevalidateProductCache> context)
    {
        await _fulfillment.RevalidateOrderProductsAsync(context.Message.OrderId, context.CancellationToken);

        await context.Publish(new CacheRevalidated { OrderId = context.Message.OrderId });
    }
}

public class CompensateFulfillmentConsumer : IConsumer<CompensateFulfillment>
{
    private readonly IService _fulfillment;

    public CompensateFulfillmentConsumer(IService fulfillment) => _fulfillment = fulfillment;

    public async Task Consume(ConsumeContext<CompensateFulfillment> context)
    {
        var result = await _fulfillment.CompensateAsync(
            context.Message.OrderId,
            context.Message.AhamoveOrderId,
            context.Message.NhanhOrderId,
            context.Message.Reason,
            context.CancellationToken);

        // Published even when a cancellation was refused: the saga has to reach Failed either way,
        // carrying the detail so ops know what is still outstanding.
        await context.Publish(new FulfillmentCompensated
        {
            OrderId = context.Message.OrderId,
            DeliveryCleared = result.DeliveryCleared,
            PosOrderCleared = result.PosOrderCleared,
            Details = result.Details
        });
    }
}

public class MarkFulfillmentFailedConsumer : IConsumer<MarkFulfillmentFailed>
{
    private readonly IService _fulfillment;

    public MarkFulfillmentFailedConsumer(IService fulfillment) => _fulfillment = fulfillment;

    public async Task Consume(ConsumeContext<MarkFulfillmentFailed> context)
    {
        await _fulfillment.MarkFulfillmentFailedAsync(
            context.Message.OrderId, context.Message.Step, context.Message.Reason, context.CancellationToken);
    }
}

/// <summary>
/// Single hook for alerting: everything a consumer throws passes through here once the retry policy
/// is done with it. The other three observer kinds MassTransit offers are omitted deliberately -
/// sends and publishes are already queryable in the OutboxMessage table, and exhausted messages
/// already land in the transport's _error queue.
/// </summary>
public class FulfillmentConsumeObserver : IConsumeObserver
{
    private readonly ILogger<FulfillmentConsumeObserver> _logger;

    public FulfillmentConsumeObserver(ILogger<FulfillmentConsumeObserver> logger) => _logger = logger;

    public Task PreConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;

    public Task PostConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        // Must not throw: an observer that throws breaks message processing itself.
        _logger.LogError(exception,
            "Consumer faulted for {MessageType} (messageId={MessageId})",
            typeof(T).Name, context.MessageId);
        return Task.CompletedTask;
    }
}
