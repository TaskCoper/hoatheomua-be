using HoaTheoMua.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class PublicationConsumer(Worker worker) : IConsumer<PublicationRequested>
{
    public Task Consume(ConsumeContext<PublicationRequested> context)
        => worker.Process(context.Message.PublicationId, context.Message.Revision, context.CancellationToken);
}
public sealed class PublicationConsumerDefinition : ConsumerDefinition<PublicationConsumer>
{
    public const string QueueName = "postgen-publication";
    public PublicationConsumerDefinition() { EndpointName = QueueName; ConcurrentMessageLimit = 4; }
}

// The scanner only dispatches durable work. It never calls an external publisher.
public sealed class Recovery(AppDbContext db, IPublishEndpoint bus, TimeProvider clock)
{
    public async Task<int> Dispatch(CancellationToken ct)
    {
        var now = clock.GetUtcNow(); var stale = now.AddSeconds(-60);
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var jobs = db.Database.IsRelational()
            ? await db.PostPublications.FromSqlInterpolated($"""
                SELECT * FROM "PostPublication"
                WHERE "Status" IN ('Queued','Scheduled','Publishing','NeedsReview')
                  AND (("LeaseId" IS NOT NULL AND "LeaseUntil" <= {now})
                    OR ("LeaseId" IS NULL AND "NextRunAt" <= {now}))
                  AND ("LastDispatchedAt" IS NULL OR "LastDispatchedAt" <= {stale})
                ORDER BY "NextRunAt", "CreatedAt", "Id" LIMIT 50 FOR UPDATE SKIP LOCKED
                """).ToListAsync(ct)
            : await db.PostPublications.Where(x => (x.Status == "Queued" || x.Status == "Scheduled" || x.Status == "Publishing" || x.Status == "NeedsReview")
                && ((x.LeaseId != null && x.LeaseUntil <= now) || (x.LeaseId == null && x.NextRunAt <= now))
                && (x.LastDispatchedAt == null || x.LastDispatchedAt <= stale)).OrderBy(x => x.NextRunAt).ThenBy(x => x.CreatedAt).Take(50).ToListAsync(ct);
        foreach (var job in jobs)
        {
            job.LastDispatchedAt = now;
            await bus.Publish(new PublicationRequested(job.Id, job.Revision), ct);
        }
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return jobs.Count;
    }
}
public sealed class RecoveryHostedService(IServiceScopeFactory scopes, IOptions<PublishingOptions> settings,
    ILogger<RecoveryHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<Recovery>().Dispatch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception) { logger.LogError("PostGen publication recovery could not dispatch; it will retry on the next scan."); }
            await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(settings.Value.RecoveryIntervalSeconds, 1, 60)), stoppingToken);
        }
    }
}
