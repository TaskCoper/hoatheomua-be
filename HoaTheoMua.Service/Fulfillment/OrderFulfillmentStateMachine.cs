using HoaTheoMua.Repository.Entity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.Fulfillment;

/// <summary>
/// Orchestrates everything that used to run inline inside FulfillPaidOrder: dispatch the courier,
/// create the POS order, purge the storefront cache - and undo whatever got booked when a step fails.
/// </summary>
/// <remarks>
/// CorrelationId is the Order.Id, so an order can only ever have one saga; a redelivered webhook
/// correlates onto the existing instance instead of starting a second fulfilment.
///
/// There is no message scheduler and no timeout: escalation is driven by the Fault&lt;T&gt; messages
/// MassTransit publishes once a consumer exhausts its retry policy.
/// </remarks>
public sealed class OrderFulfillmentStateMachine : MassTransitStateMachine<OrderFulfillmentState>
{
    // Kept well under the 64-char CurrentState column.
    public State DispatchingDelivery { get; private set; } = null!;
    public State CreatingPosOrder { get; private set; } = null!;
    public State Revalidating { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;

    // Deliberately NOT MassTransit's built-in Final: two distinct terminal states let ops query
    // "which orders need a manual refund" straight off CurrentState. Neither finalizes, so the row
    // survives as an audit trail.
    public State Fulfilled { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<OrderPaid> OrderPaid { get; private set; } = null!;
    public Event<DeliveryDispatched> DeliveryDispatched { get; private set; } = null!;
    public Event<PosOrderCreated> PosOrderCreated { get; private set; } = null!;
    public Event<CacheRevalidated> CacheRevalidated { get; private set; } = null!;
    public Event<FulfillmentCompensated> FulfillmentCompensated { get; private set; } = null!;

    public Event<Fault<DispatchDelivery>> DispatchDeliveryFaulted { get; private set; } = null!;
    public Event<Fault<CreatePosOrder>> CreatePosOrderFaulted { get; private set; } = null!;
    public Event<Fault<RevalidateProductCache>> RevalidateProductCacheFaulted { get; private set; } = null!;
    public Event<Fault<CompensateFulfillment>> CompensateFulfillmentFaulted { get; private set; } = null!;

    public OrderFulfillmentStateMachine(ILogger<OrderFulfillmentStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPaid, x =>
        {
            x.CorrelateById(context => context.Message.OrderId);
            x.SelectId(context => context.Message.OrderId);
        });
        Event(() => DeliveryDispatched, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PosOrderCreated, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => CacheRevalidated, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => FulfillmentCompensated, x => x.CorrelateById(context => context.Message.OrderId));

        // Fault<T>.Message is the original command, so the order id sits one level deeper.
        Event(() => DispatchDeliveryFaulted, x => x.CorrelateById(context => context.Message.Message.OrderId));
        Event(() => CreatePosOrderFaulted, x => x.CorrelateById(context => context.Message.Message.OrderId));
        Event(() => RevalidateProductCacheFaulted, x => x.CorrelateById(context => context.Message.Message.OrderId));
        Event(() => CompensateFulfillmentFaulted, x => x.CorrelateById(context => context.Message.Message.OrderId));

        Initially(
            When(OrderPaid)
                .Then(context =>
                {
                    context.Saga.Gateway = context.Message.Gateway;
                    context.Saga.TransactionId = context.Message.TransactionId;
                    context.Saga.CreatedAt = DateTimeOffset.UtcNow;
                    context.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .Publish(context => new DispatchDelivery { OrderId = context.Saga.CorrelationId })
                .TransitionTo(DispatchingDelivery));

        During(DispatchingDelivery,
            When(DeliveryDispatched)
                .Then(context =>
                {
                    // Recorded even when Dispatched is false: the failure branch reads it to decide
                    // whether there is a courier to cancel at all.
                    context.Saga.DeliveryDispatched = context.Message.Plan.Dispatched;
                    context.Saga.AhamoveOrderId = context.Message.Plan.AhamoveOrderId;
                    context.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .Publish(context => new CreatePosOrder
                {
                    OrderId = context.Saga.CorrelationId,
                    Plan = context.Message.Plan
                })
                .TransitionTo(CreatingPosOrder),

            // Nothing was created externally yet, so there is nothing to compensate.
            When(DispatchDeliveryFaulted)
                .Then(context =>
                {
                    RecordFailure(context.Saga, nameof(DispatchDelivery), Describe(context.Message.Exceptions));
                    logger.LogCritical(
                        "Fulfilment failed at DispatchDelivery for order {OrderId}: {Reason}",
                        context.Saga.CorrelationId, context.Saga.FailureReason);
                })
                .Publish(context => ToMarkFailed(context.Saga))
                .TransitionTo(Failed));

        During(CreatingPosOrder,
            When(PosOrderCreated)
                .Then(context =>
                {
                    context.Saga.NhanhOrderId = context.Message.NhanhOrderId;
                    context.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .Publish(context => new RevalidateProductCache { OrderId = context.Saga.CorrelationId })
                .TransitionTo(Revalidating),

            When(CreatePosOrderFaulted)
                .Then(context =>
                {
                    RecordFailure(context.Saga, nameof(CreatePosOrder), Describe(context.Message.Exceptions));
                    logger.LogCritical(
                        "Fulfilment failed at CreatePosOrder for order {OrderId} (ahamoveOrderId={AhamoveOrderId}): {Reason}",
                        context.Saga.CorrelationId, context.Saga.AhamoveOrderId, context.Saga.FailureReason);
                })
                .IfElse(
                    context => HasOutstandingBooking(context.Saga),
                    compensate => compensate
                        .Then(context => context.Saga.CompensationAttempts++)
                        .Publish(context => new CompensateFulfillment
                        {
                            OrderId = context.Saga.CorrelationId,
                            AhamoveOrderId = context.Saga.AhamoveOrderId,
                            // Normally null here - CreatePosOrder is the step that just failed - but
                            // set if an earlier attempt created a POS order and then died before
                            // persisting it. Passing it through means that orphan gets cleaned too.
                            NhanhOrderId = context.Saga.NhanhOrderId,
                            Reason = context.Saga.FailureReason ?? "POS order creation failed"
                        })
                        .TransitionTo(Compensating),
                    // Scheduled order: Ahamove was never called, so go straight to Failed.
                    nothingBooked => nothingBooked
                        .Publish(context => ToMarkFailed(context.Saga))
                        .TransitionTo(Failed)));

        During(Compensating,
            // Both branches land in Failed. A compensation that could not complete must still be
            // visible to ops rather than retrying forever - FailureReason carries what happened.
            When(FulfillmentCompensated)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTimeOffset.UtcNow;

                    if (!context.Message.DeliveryCleared || !context.Message.PosOrderCleared)
                    {
                        context.Saga.FailureReason = Truncate(
                            $"{context.Saga.FailureReason} | compensation incomplete: {context.Message.Details}");
                    }

                    logger.LogCritical(
                        "Compensation finished for order {OrderId}: deliveryCleared={DeliveryCleared} "
                        + "posOrderCleared={PosOrderCleared} ({Details}). Manual refund required.",
                        context.Saga.CorrelationId, context.Message.DeliveryCleared,
                        context.Message.PosOrderCleared, context.Message.Details);
                })
                .Publish(context => ToMarkFailed(context.Saga))
                .TransitionTo(Failed),

            When(CompensateFulfillmentFaulted)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    context.Saga.FailureReason = Truncate(
                        $"{context.Saga.FailureReason} | compensation faulted: {Describe(context.Message.Exceptions)}");
                    logger.LogCritical(
                        "Compensation faulted for order {OrderId} (ahamoveOrderId={AhamoveOrderId} "
                        + "nhanhOrderId={NhanhOrderId}). Manual cleanup required.",
                        context.Saga.CorrelationId, context.Saga.AhamoveOrderId, context.Saga.NhanhOrderId);
                })
                .Publish(context => ToMarkFailed(context.Saga))
                .TransitionTo(Failed));

        During(Revalidating,
            When(CacheRevalidated)
                .Then(context => context.Saga.UpdatedAt = DateTimeOffset.UtcNow)
                .TransitionTo(Fulfilled),

            // Deliberately NOT compensating. A failed storefront cache purge is not a reason to
            // cancel a courier who is already on the way; the order really is fulfilled.
            When(RevalidateProductCacheFaulted)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    logger.LogError(
                        "Cache revalidation failed for order {OrderId}, order still counts as fulfilled: {Reason}",
                        context.Saga.CorrelationId, Describe(context.Message.Exceptions));
                })
                .TransitionTo(Fulfilled));

        // Redeliveries landing after the saga is done are expected traffic, not errors. Without
        // this they would fault with "not accepted in state" and end up in the _error queue.
        During(Fulfilled, Failed,
            Ignore(OrderPaid),
            Ignore(DeliveryDispatched),
            Ignore(PosOrderCreated),
            Ignore(CacheRevalidated),
            Ignore(FulfillmentCompensated),
            Ignore(DispatchDeliveryFaulted),
            Ignore(CreatePosOrderFaulted),
            Ignore(RevalidateProductCacheFaulted),
            Ignore(CompensateFulfillmentFaulted));
    }

    // "Outstanding" means a real booking exists on someone else's system that we are responsible
    // for clearing up.
    private static bool HasOutstandingBooking(OrderFulfillmentState saga)
        => (saga.DeliveryDispatched && !string.IsNullOrWhiteSpace(saga.AhamoveOrderId))
           || saga.NhanhOrderId.HasValue;

    private static void RecordFailure(OrderFulfillmentState saga, string step, string reason)
    {
        saga.FailedStep = step;
        saga.FailureReason = Truncate(reason);
        saga.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static MarkFulfillmentFailed ToMarkFailed(OrderFulfillmentState saga) => new()
    {
        OrderId = saga.CorrelationId,
        Step = saga.FailedStep ?? "Unknown",
        Reason = saga.FailureReason ?? "Unknown failure",
        AhamoveOrderId = saga.AhamoveOrderId,
        NhanhOrderId = saga.NhanhOrderId
    };

    private static string Describe(ExceptionInfo[]? exceptions)
        => exceptions is { Length: > 0 }
            ? string.Join(" | ", exceptions.Select(x => $"{x.ExceptionType}: {x.Message}"))
            : "Unknown failure";

    // FailureReason is capped at 1000 chars in the EF configuration; a NhanhVn rejection body can
    // easily run longer than that and would otherwise fail the saga's own SaveChanges.
    private static string Truncate(string value)
        => value.Length <= 1000 ? value : value[..1000];
}
