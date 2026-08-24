namespace HoaTheoMua.Service.Fulfillment;

/// <summary>
/// The three side-effecting steps that used to run inline inside Webhook.FulfillPaidOrder, split so
/// each can be driven independently by the saga - and so each persists its own result the moment
/// the external call returns, instead of holding everything in the change tracker until one final
/// SaveChanges that a later failure would discard.
/// </summary>
/// <remarks>
/// Every method is idempotent: it re-reads the order and skips the outbound call when the result is
/// already recorded (AhahaMoveOrderId / NhanhOrderId). That is what stops a redelivered webhook -
/// or a retried message - from dispatching a second courier.
/// </remarks>
public interface IService
{
    /// <summary>
    /// Derives the delivery plan and, for instant orders, books the Ahamove courier. Always returns
    /// a plan even when no courier is booked, because the POS step needs the derived
    /// DelayTimeUnix / TagIds / PrivateDescription either way.
    /// </summary>
    Task<DeliveryDispatchResult> DispatchDeliveryAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the NhanhVn POS order, reusing the plan verbatim so the delivery time it shows
    /// matches what Ahamove was actually told.
    /// </summary>
    Task<PosOrderResult> CreatePosOrderAsync(Guid orderId, DeliveryDispatchResult plan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges the storefront cache for every product on the order (and their parents). Best-effort:
    /// a failure here never invalidates a delivery that really happened.
    /// </summary>
    Task RevalidateOrderProductsAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels whatever the failed fulfilment already booked outside our database. Attempts both
    /// legs even if the first fails, and never throws on a rejection - a compensation that cannot
    /// complete still has to reach ops rather than loop. Retryable transport failures DO throw, so
    /// the endpoint retry policy gets a chance before the saga gives up.
    /// </summary>
    Task<CompensationOutcome> CompensateAsync(Guid orderId, string? ahamoveOrderId, int? nhanhOrderId,
        string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminal bookkeeping: park the order in FulfillmentFailed so it stops looking like a healthy
    /// paid order, and log loudly enough that ops can find it. The refund itself is manual.
    /// </summary>
    Task MarkFulfillmentFailedAsync(Guid orderId, string step, string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ops view over the saga table. Because the design has no timers, this is also how a stalled
    /// fulfilment is found: filter on a non-terminal state and sort by how long it has sat there.
    /// </summary>
    /// <param name="state">CurrentState to filter on, e.g. "Failed" or "CreatingPosOrder".</param>
    /// <param name="stuckForMinutes">Only return sagas untouched for at least this long.</param>
    Task<IReadOnlyList<FulfillmentStateView>> GetFulfillmentStatesAsync(
        string? state, int? stuckForMinutes, int limit, CancellationToken cancellationToken = default);
}

/// <summary>Flattened saga row for the ops endpoint.</summary>
public record FulfillmentStateView
{
    public Guid OrderId { get; init; }
    public string CurrentState { get; init; } = string.Empty;
    public string? Gateway { get; init; }
    public string? AhamoveOrderId { get; init; }
    public int? NhanhOrderId { get; init; }
    public bool DeliveryDispatched { get; init; }
    public string? FailedStep { get; init; }
    public string? FailureReason { get; init; }
    public int CompensationAttempts { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Outcome of the POS step. Not a bus message - see PosOrderCreated for that.</summary>
public record PosOrderResult
{
    public int NhanhOrderId { get; init; }
    public string? TrackingUrl { get; init; }
}

/// <summary>
/// What compensation managed to clear. A flag is true when nothing is left outstanding on that leg
/// - either it was cancelled, or there was never anything there.
/// </summary>
public record CompensationOutcome
{
    public bool DeliveryCleared { get; init; }
    public bool PosOrderCleared { get; init; }
    public string? Details { get; init; }
}
