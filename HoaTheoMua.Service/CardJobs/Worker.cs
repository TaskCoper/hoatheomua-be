using HoaTheoMua.Repository;
using HoaTheoMua.Service.Card;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static HoaTheoMua.Service.CardJobs.JobJson;

namespace HoaTheoMua.Service.CardJobs;

public sealed class Worker(AppDbContext db, Store store, AiService.IService ai,
    Card.Service cards, IServiceScopeFactory scopes, IOptions<CardJobOptions> settings,
    TimeProvider clock, ILogger<Worker> logger)
{
    public async Task Process(Guid id, CancellationToken ct)
    {
        var lease = Guid.NewGuid();
        PreparedCard input;
        await using (var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            var job = await store.Lock(id, ct);
            if (job == null || job.Status != "Queued") return;
            input = Read<PreparedCard>(job.Snapshot);
            job.Status = "Running"; job.Stage = "GeneratingCard"; job.AttemptCount++;
            job.LeaseId = lease; job.LeaseUntil = clock.GetUtcNow().AddSeconds(settings.Value.LeaseSeconds);
            store.Touch(job);
            await db.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(settings.Value.BudgetSeconds));
        using var heartbeatStop = new CancellationTokenSource();
        var heartbeat = Heartbeat(id, lease, budget, heartbeatStop.Token);
        var checkpoint = new Checkpoint();
        try
        {
            cards.ValidateBranding();
            var image = await ai.GenerateCardImage(input.Input, budget.Token);
            checkpoint = new Checkpoint(image);
            await SaveCheckpoint(id, lease, "Branding", checkpoint, budget.Token);
            checkpoint = checkpoint with { Branding = await cards.Brand(input, image, budget.Token) };
            await SaveCheckpoint(id, lease, "Saving", checkpoint, budget.Token);
            await Complete(id, lease, input, checkpoint, budget.Token);
            logger.LogInformation("Card job {JobId} processing finished", id);
        }
        catch (Exception ex)
        {
            // Never automatically replay a paid provider call with an unknown outcome.
            // Keep exception messages/URLs out of public errors and operational logs.
            logger.LogWarning("Card job {JobId} stopped at checkpoint {Stage}; exception {ExceptionType}",
                id, checkpoint.Card != null ? "Saving" : "GeneratingCard", ex.GetType().Name);
            using var save = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await Fail(id, lease, ex is OperationCanceledException ? "CARD_JOB_INTERRUPTED" : "CARD_JOB_GENERATION_FAILED", save.Token);
        }
        finally
        {
            heartbeatStop.Cancel();
            await heartbeat;
        }
    }

    private async Task SaveCheckpoint(Guid id, Guid lease, string stage, Checkpoint checkpoint, CancellationToken ct)
    {
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var job = await store.Lock(id, ct);
        if (job == null || job.Status != "Running" || job.LeaseId != lease || job.LeaseUntil <= clock.GetUtcNow())
            throw new OperationCanceledException("Job lease lost");
        // Persist completed output even if cancellation arrived just after the provider returned.
        job.Checkpoint = Write(checkpoint);
        if (job.CancelRequestedAt != null) store.Finish(job, "Canceled");
        else { job.Stage = stage; store.Touch(job); }
        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
        if (job.Status == "Canceled") throw new OperationCanceledException("Job canceled");
    }

    private async Task Complete(Guid id, Guid lease, PreparedCard input, Checkpoint checkpoint, CancellationToken ct)
    {
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var job = await store.Lock(id, ct);
        if (job == null || job.Status != "Running" || job.LeaseId != lease || job.LeaseUntil <= clock.GetUtcNow()) return;
        if (job.CancelRequestedAt != null) store.Finish(job, "Canceled");
        else
        {
            var result = await cards.AddPreparedResult(input, checkpoint.Card!, ct, checkpoint.Branding);
            job.GeneratedCardId = result.Id; job.Result = Write(result); job.Stage = "Completed";
            store.Finish(job, "Succeeded");
        }

        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
    }

    private async Task Fail(Guid id, Guid lease, string code, CancellationToken ct)
    {
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var job = await store.Lock(id, ct);
        if (job == null || job.Status != "Running" || job.LeaseId != lease) return;
        store.Finish(job, job.CancelRequestedAt != null ? "Canceled" : "Failed", job.CancelRequestedAt != null ? null : code);
        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
    }

    private async Task Heartbeat(Guid id, Guid lease, CancellationTokenSource budget, CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), stop);
                using var scope = scopes.CreateScope();
                var heartbeatDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var heartbeatStore = scope.ServiceProvider.GetRequiredService<Store>();
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stop);
                timeout.CancelAfter(TimeSpan.FromSeconds(10));
                await using var tx = heartbeatDb.Database.IsRelational() ? await heartbeatDb.Database.BeginTransactionAsync(timeout.Token) : null;
                var job = await heartbeatStore.Lock(id, timeout.Token);
                if (job == null || job.Status != "Running" || job.LeaseId != lease || job.CancelRequestedAt != null
                    || job.LeaseUntil <= clock.GetUtcNow())
                {
                    budget.Cancel(); return;
                }
                job.LeaseUntil = clock.GetUtcNow().AddSeconds(settings.Value.LeaseSeconds);
                heartbeatStore.Touch(job);
                await heartbeatDb.SaveChangesAsync(timeout.Token);
                if (tx != null) await tx.CommitAsync(timeout.Token);
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception)
        {
            // Fail closed if the lease cannot be renewed; recovery handles an unavailable DB.
            budget.Cancel();
        }
    }
}
