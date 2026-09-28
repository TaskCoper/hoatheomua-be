using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.XanhSm;

public class XanhSmDeliveryCompletionService(AppDbContext dbContext,
    NhanhVnService.INhanhVnDeliveryStatusClient nhanhVn, IConfiguration configuration,
    ILogger<XanhSmDeliveryCompletionService> logger)
{
    public async Task ProcessAsync(XanhSmOrderChanged message, CancellationToken cancellationToken = default)
    {
        if (message.Environment != configuration[$"{nameof(XanhSmWebhookOptions)}:Environment"])
            throw new FulfillmentPermanentException("XanhSM message environment does not match configuration");

        if (string.IsNullOrWhiteSpace(message.Status))
            throw new FulfillmentPermanentException("XanhSM order event is missing status");
        if (message.Status != "COMPLETED" || message.SubStatus is "RETURNING" or "RETURNED" or "FAILED")
            return;

        // The consumer EF outbox owns the transaction. Lock the order across the external call
        // and local marker update so distinct events for the same delivery cannot race.
        if (dbContext.Database.IsRelational() && dbContext.Database.CurrentTransaction == null)
            throw new InvalidOperationException("XanhSM completion requires the consumer outbox transaction");

        var query = dbContext.Orders.Where(o => o.ShippingGateway == ShippingGateway.XanhSm
                                               && o.ShippingPartyOrderId == message.ShippingOrderId);
        if (dbContext.Database.IsRelational())
            query = dbContext.Orders.FromSqlInterpolated(
                $"SELECT * FROM \"Orders\" WHERE \"ShippingGateway\" = {(int)ShippingGateway.XanhSm} AND \"ShippingPartyOrderId\" = {message.ShippingOrderId} FOR UPDATE");

        var order = await query.SingleOrDefaultAsync(cancellationToken);
        if (order == null)
        {
            // Booking/POS persistence may still be in progress when the first event arrives.
            throw new FulfillmentTransientException("XanhSM delivery has no matching local order yet");
        }
        if (order.NhanhCompletedShippingOrderId == message.ShippingOrderId)
            return;

        if (order.NhanhOrderId is not > 0)
            throw new FulfillmentTransientException("NhanhVN order has not been created yet");
        if (order.Status == Fulfillment.Service.FulfillmentFailedStatus)
            throw new FulfillmentPermanentException("Cannot complete NhanhVN for a failed fulfillment");

        await nhanhVn.MarkDeliveredAsync(order.NhanhOrderId.Value, cancellationToken);
        order.NhanhCompletedShippingOrderId = message.ShippingOrderId;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("XanhSM delivery {ShippingOrderId} completed; NhanhVN order {NhanhOrderId} synchronized (event {EventId})",
            message.ShippingOrderId, order.NhanhOrderId, message.EventId);
    }

}
