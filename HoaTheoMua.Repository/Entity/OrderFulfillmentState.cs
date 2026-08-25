using HoaTheoMua.Repository.Abtraction;
using MassTransit;

namespace HoaTheoMua.Repository.Entity;

/// <summary>
/// Persisted progress of one order's fulfilment. CorrelationId IS the Order.Id, so an order can
/// only ever have one saga and a redelivered webhook correlates onto the existing instance.
/// </summary>
/// <remarks>
/// Deliberately NOT a BaseEntity&lt;Guid&gt;: that would give the row both an Id and a
/// CorrelationId, and EF's convention would pick Id as the primary key while MassTransit only ever
/// populates CorrelationId. Soft-delete has no meaning for a saga instance either.
///
/// IAuditableEntity is implemented for consistency with the rest of the schema, but nothing in
/// AppDbContext populates it (there is no SaveChanges interceptor) - the state machine sets both
/// timestamps by hand on every transition.
/// </remarks>
public class OrderFulfillmentState : SagaStateMachineInstance, IAuditableEntity
{
    public Guid CorrelationId { get; set; }          // == Order.Id
    public string CurrentState { get; set; } = null!;

    public string? Gateway { get; set; }             // "BaoKim" | "SePay"
    public Guid? TransactionId { get; set; }

    public string? AhamoveOrderId { get; set; }
    public int? NhanhOrderId { get; set; }

    /// <summary>
    /// False for scheduled orders, where Ahamove is never called - the POS-failure branch reads
    /// this to decide whether there is a courier to cancel at all.
    /// </summary>
    public bool DeliveryDispatched { get; set; }

    public string? FailedStep { get; set; }
    public string? FailureReason { get; set; }
    /// <summary>
    /// How many saga-level compensation rounds have been issued. 1 = compensation ran once; higher
    /// means an earlier round exhausted the compensate-fulfillment endpoint's own retries and the
    /// saga reissued the command. 0 = nothing was compensated at all, because nothing had been
    /// booked yet (the failure hit before Ahamove, or the order was scheduled so it was never called).
    /// </summary>
    public int CompensationAttempts { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
