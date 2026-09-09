using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.Fulfillment;

// Message contracts for the order-fulfillment saga.
//
// The bus serializes these with Newtonsoft + TypeNameHandling.Objects, which embeds the CLR type
// name in the payload. Renaming or moving any type in this file breaks messages already sitting in
// a RabbitMQ queue or in the OutboxMessage table - treat these names as frozen once deployed.
// (The Ahamove-specific names were generalised to ShippingPartyOrderId/ShippingGateway BEFORE the
// first deployment, when XanhSm became the second courier; that was the last rename.)

/// <summary>
/// Everything the delivery step derives once and the POS step must reuse verbatim.
/// </summary>
/// <remarks>
/// DelayTimeUnix and TagIds are the reason this record exists. The legacy FulfillPaidOrder computed
/// delayTime = now + ahamove.idle_time a single time and fed it to both Ahamove
/// (order_time/idle_until) and NhanhVn (DeliveryDate/DeliveryTime); fastTagId was likewise appended
/// inside the Ahamove branch but sent on to NhanhVn. Splitting the two calls into separate consumers
/// means they no longer run at the same instant, so recomputing either value in the POS step would
/// drift and NhanhVn would show a delivery time Ahamove never agreed to.
/// </remarks>
public record DeliveryDispatchResult
{
    /// <summary>
    /// False when the order is scheduled (OrderTime > 0), carries delivery tags, or (Ahamove only)
    /// the ahamove.instant_delivery config is off - in those cases no courier is booked and there
    /// is correspondingly nothing to compensate.
    /// </summary>
    public bool Dispatched { get; init; }

    /// <summary>Copied from Order.ShippingGateway so the POS step and compensation know which courier this is.</summary>
    public ShippingGateway ShippingGateway { get; init; }

    /// <summary>Courier-side id: the Ahamove order id or the XanhSm delivery id.</summary>
    public string? ShippingPartyOrderId { get; init; }
    public string? ShippingTrackingUrl { get; init; }

    /// <summary>
    /// Unix seconds the courier will collect the parcel: now + 90 minutes for flower preparation
    /// for both Ahamove and XanhSm.
    /// </summary>
    public long DelayTimeUnix { get; init; }

    /// <summary>Already includes vatTagId / fastTagId as applicable.</summary>
    public List<long> TagIds { get; init; } = [];

    public string PrivateDescription { get; init; } = string.Empty;
}

// --------------------------------- Kick-off event ---------------------------------

/// <summary>
/// Published by the payment webhook inside the same transaction that flips the order to Paid and
/// inserts the Transaction row, via the MassTransit bus outbox. The money is already taken by the
/// time this is published - fulfilment failing later never un-takes it.
/// </summary>
public record OrderPaid
{
    public Guid OrderId { get; init; }
    public Guid TransactionId { get; init; }
    public string Gateway { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

// --------------------------------- Commands ---------------------------------
//
// The saga publishes rather than sends these, so it never has to hardcode a queue name. Each has
// exactly one consumer, which makes publish and send equivalent here - and every consumer is
// additionally idempotent (it re-reads the order and skips work already recorded), so even a second
// subscriber could not double-dispatch a courier.

public record DispatchDelivery
{
    public Guid OrderId { get; init; }
}

public record CreatePosOrder
{
    public Guid OrderId { get; init; }
    public DeliveryDispatchResult Plan { get; init; } = new();
}

public record RevalidateProductCache
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Undo whatever the failed fulfilment already created outside our database. One message covers
/// both legs rather than two, because the saga only ever needs a single answer back before it can
/// move to Failed, and because both cancellations have to be attempted even if the first one fails.
/// </summary>
public record CompensateFulfillment
{
    public Guid OrderId { get; init; }
    public ShippingGateway? ShippingGateway { get; init; }
    public string? ShippingPartyOrderId { get; init; }
    public int? NhanhOrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Terminal bookkeeping: park the order in FulfillmentFailed and alert ops, who have to issue the
/// refund by hand. Kept as a message so the state machine stays free of scoped services.
/// </summary>
public record MarkFulfillmentFailed
{
    public Guid OrderId { get; init; }
    public string Step { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public ShippingGateway? ShippingGateway { get; init; }
    public string? ShippingPartyOrderId { get; init; }
    public int? NhanhOrderId { get; init; }
}

// --------------------------------- Events back to the saga ---------------------------------

public record DeliveryDispatched
{
    public Guid OrderId { get; init; }
    public DeliveryDispatchResult Plan { get; init; } = new();
}

public record PosOrderCreated
{
    public Guid OrderId { get; init; }
    public int NhanhOrderId { get; init; }
    public string? TrackingUrl { get; init; }
}

public record CacheRevalidated
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Raised whether or not the cancellations actually succeeded. A compensation that could not
/// complete must still move the saga to Failed so ops can see it, rather than spin forever.
/// </summary>
/// <remarks>
/// Both flags mean "nothing is left outstanding on that leg" - either it was cancelled, or there
/// was nothing there to cancel. False means a real booking survives and a human has to clear it.
/// </remarks>
public record FulfillmentCompensated
{
    public Guid OrderId { get; init; }
    public bool DeliveryCleared { get; init; }
    public bool PosOrderCleared { get; init; }
    public string? Details { get; init; }
}
