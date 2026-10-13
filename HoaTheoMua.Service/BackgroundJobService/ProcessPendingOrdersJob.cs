using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace HoaTheoMua.Service.BackgroundJobService;

[DisallowConcurrentExecution]
public sealed class ProcessPendingOrdersJob(
    AppDbContext dbContext,
    IOptions<PendingOrderJobOptions> options,
    TimeProvider timeProvider,
    ILogger<ProcessPendingOrdersJob> logger) : IJob
{
    private const string PendingStatus = "Pending";
    private const string CancelledStatus = "Cancelled";

    public async Task Execute(IJobExecutionContext context)
    {
        var timeoutMinutes = options.Value.TimeoutMinutes;
        var now = timeProvider.GetUtcNow();
        var threshold = now.AddMinutes(-timeoutMinutes);

        var expiredPendingOrders = dbContext.Orders
            .Where(order =>
                !order.IsDeleted &&
                order.Status == PendingStatus &&
                order.CreatedAt <= threshold);

        int cancelledCount;
        if (dbContext.Database.IsRelational())
        {
            cancelledCount = await expiredPendingOrders
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(order => order.Status, CancelledStatus)
                        .SetProperty(order => order.UpdatedAt, now),
                    context.CancellationToken);
        }
        else
        {
            var orders = await expiredPendingOrders.ToListAsync(context.CancellationToken);
            foreach (var order in orders)
            {
                order.Status = CancelledStatus;
                order.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync(context.CancellationToken);
            cancelledCount = orders.Count;
        }

        if (cancelledCount > 0)
        {
            logger.LogInformation(
                "Cancelled {CancelledCount} pending orders older than {TimeoutMinutes} minutes.",
                cancelledCount,
                timeoutMinutes);
        }
    }
}
