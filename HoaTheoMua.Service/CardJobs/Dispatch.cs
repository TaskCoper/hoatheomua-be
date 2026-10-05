using HoaTheoMua.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.CardJobs;

public sealed class CardGenerationConsumer(Worker worker) : IConsumer<CardGenerationRequested>
{
    public Task Consume(ConsumeContext<CardGenerationRequested> context) => worker.Process(context.Message.JobId, context.CancellationToken);
}

public sealed class CardGenerationConsumerDefinition : ConsumerDefinition<CardGenerationConsumer>
{
    public const string QueueName = "card-generation";
    public CardGenerationConsumerDefinition(IOptions<CardJobOptions> options)
    {
        EndpointName = QueueName;
        ConcurrentMessageLimit = options.Value.Concurrency;
    }
}

public sealed class Recovery(AppDbContext db, IPublishEndpoint bus, Store store, TimeProvider clock)
{
    public async Task<int> Scan(CancellationToken ct)
    {
        var now = clock.GetUtcNow(); var stale = now.AddSeconds(-60);
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var jobs = db.Database.IsRelational()
            ? await db.CardGenerationJobs.FromSqlInterpolated($"""
                SELECT * FROM "CardGenerationJob"
                WHERE ("Status" = 'Running' AND "LeaseUntil" <= {now})
                   OR ("Status" = 'Queued' AND ("LastDispatchedAt" IS NULL OR "LastDispatchedAt" <= {stale}))
                ORDER BY "CreatedAt" LIMIT 50 FOR UPDATE SKIP LOCKED
                """).ToListAsync(ct)
            : await db.CardGenerationJobs.Where(x => (x.Status == "Running" && x.LeaseUntil <= now)
                || (x.Status == "Queued" && (x.LastDispatchedAt == null || x.LastDispatchedAt <= stale))).Take(50).ToListAsync(ct);
        foreach (var job in jobs)
        {
            if (job.Status == "Running")
            {
                // Checkpoints survive, but an interrupted provider call is not safe to replay automatically.
                store.Finish(job, job.CancelRequestedAt != null ? "Canceled" : "Failed",
                    job.CancelRequestedAt != null ? null : "CARD_JOB_WORKER_INTERRUPTED");
            }
            else
            {
                job.LastDispatchedAt = now; store.Touch(job);
                await bus.Publish(new CardGenerationRequested(job.Id), ct);
            }
        }
        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
        return jobs.Count;
    }
}

public sealed class RecoveryHostedService(IServiceScopeFactory scopes, IOptions<CardJobOptions> options,
    ILogger<RecoveryHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<Recovery>().Scan(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception) { logger.LogError("Card job recovery failed; the next scan will retry."); }
            try { await Task.Delay(TimeSpan.FromSeconds(options.Value.RecoveryIntervalSeconds), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
