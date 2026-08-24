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

}


/// <summary>Outcome of the POS step. Not a bus message - see PosOrderCreated for that.</summary>
public record PosOrderResult
{
    public int NhanhOrderId { get; init; }
    public string? TrackingUrl { get; init; }
}
