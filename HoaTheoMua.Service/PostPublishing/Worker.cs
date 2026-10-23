using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class Worker(AppDbContext db, Store store, IEnumerable<IPublishingAdapter> adapters,
    IOptions<PublishingOptions> settings, TokenProtector protector, TimeProvider clock, IPublicationTokenRefresher? tokenRefresher = null)
{
    public async Task Process(Guid id, long revision, CancellationToken ct, string? candidateExternalId = null, bool manualReview = false)
    {
        settings.Value.Validate();
        string? refreshError = null;
        if (tokenRefresher != null)
        {
            var pending = await db.PostPublications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.Revision == revision
                && (x.Status == "Queued" || x.Status == "Scheduled" || x.Status == "Publishing" || x.Status == "NeedsReview")
                && (x.NextRunAt <= clock.GetUtcNow() || manualReview) && (x.LeaseId == null || x.LeaseUntil <= clock.GetUtcNow()), ct);
            if (pending != null && settings.Value.ZaloEnabled && Read<PublicationSnapshot>(pending.Snapshot).Platform == Repository.Enum.PlatformType.Zalo)
            {
                try { await tokenRefresher.RefreshForPublication(pending.PlatformAccountId, ct); }
                catch (AccountException ex) when (ex.MessageCode == "PLATFORM_REFRESH_IN_PROGRESS") { return; }
                catch (AccountException ex) { refreshError = ex.MessageCode; }
            }
        }
        PublicationSnapshot snapshot; PublicationCheckpoint checkpoint; Credentials credentials;
        IPublishingAdapter adapter; PublicationOperation operation; Guid attemptId; long accountRevision;
        var lease = Guid.NewGuid();
        await using (var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            var (job, platform, account) = await store.LockJob(id, ct);
            if (job.Revision != revision || job.Status is not ("Queued" or "Scheduled" or "Publishing" or "NeedsReview")
                || job.LeaseUntil > clock.GetUtcNow() || (!manualReview && job.NextRunAt > clock.GetUtcNow())) return;
            if (job.Status == "Scheduled")
            {
                if (job.ScheduledAt > clock.GetUtcNow()) return; // Even an early/stale queue message cannot start a future schedule.
                if (job.ScheduledAt == null || clock.GetUtcNow() - job.ScheduledAt.Value > TimeSpan.FromSeconds(settings.Value.ScheduleLatenessSeconds))
                {
                    store.State(job, platform, "Failed", "PUBLICATION_SCHEDULE_MISSED"); job.NextRunAt = null;
                    await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct); return;
                }
            }
            if (job.LeaseId != null)
            {
                // An interrupted POST is never blindly repeated. Even if no send happened, intent is authoritative.
                var interrupted = await db.PostPublicationAttempts.Where(x => x.PublicationId == id && x.CompletedAt == null).ToListAsync(ct);
                foreach (var a in interrupted) { a.Status = "Interrupted"; a.CompletedAt = clock.GetUtcNow(); a.ErrorCode = "PUBLICATION_WORKER_INTERRUPTED"; }
                store.State(job, platform, job.PublishMayHaveOccurred ? "NeedsReview" : "Failed", "PUBLICATION_WORKER_INTERRUPTED");
                job.CanRetry = !job.PublishMayHaveOccurred;
                job.NextRunAt = clock.GetUtcNow().AddSeconds(30);
                job.LastDispatchedAt = null;
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
                return;
            }
            snapshot = Read<PublicationSnapshot>(job.Snapshot); checkpoint = Read<PublicationCheckpoint>(job.Checkpoint);
            var selected = adapters.SingleOrDefault(x => x.Platform == snapshot.Platform);
            var error = refreshError ?? (!settings.Value.Enabled(snapshot.Platform) ? "PUBLISHING_DISABLED" : selected == null ? "PUBLISHING_ADAPTER_UNAVAILABLE"
                : AccountError(account, snapshot, clock.GetUtcNow()));
            if (error == null && (job.AttemptCount >= settings.Value.MaxAttempts || (!manualReview && job.StepCount >= settings.Value.MaxSteps))) error = "PUBLICATION_BUDGET_EXCEEDED";
            if (error != null)
            {
                store.State(job, platform, job.PublishMayHaveOccurred ? "NeedsReview" : "Failed", error);
                job.CanRetry = !job.PublishMayHaveOccurred && error != "PUBLICATION_BUDGET_EXCEEDED";
                job.NextRunAt = null; // Explicit user action required after fixing configuration/account.
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct); return;
            }
            adapter = selected!;
            var cooldown = await db.PostPublications.Where(x => x.PlatformAccountId == account.Id && (x.ErrorCode == "META_RATE_LIMITED" || x.ErrorCode == "ZALO_RATE_LIMITED")
                && x.NextRunAt > clock.GetUtcNow()).Select(x => x.NextRunAt).MaxAsync(ct);
            if (cooldown.HasValue && !manualReview)
            {
                job.NextRunAt = cooldown; job.LastDispatchedAt = null; store.Touch(job);
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct); return;
            }
            try
            {
                credentials = System.Text.Json.JsonSerializer.Deserialize<Credentials>(protector.Unprotect(account.ProtectedCredentials!, PlatformAccount.Service.AccountPurpose(account.Id)))
                    ?? throw new System.Text.Json.JsonException();
                if (string.IsNullOrWhiteSpace(credentials.AccessToken)) throw new System.Text.Json.JsonException();
            }
            catch (Exception ex) when (ex is AccountException or System.Text.Json.JsonException)
            {
                store.State(job, platform, job.PublishMayHaveOccurred ? "NeedsReview" : "Failed", "PLATFORM_TOKEN_UNAVAILABLE");
                job.CanRetry = !job.PublishMayHaveOccurred; job.NextRunAt = null;
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct); return;
            }
            accountRevision = account.Revision;
            operation = job.PublishMayHaveOccurred || job.Status == "NeedsReview" ? PublicationOperation.Reconcile
                : checkpoint.ReadyToPublish ? PublicationOperation.Publish : PublicationOperation.Prepare;
            job.LeaseId = lease; job.LeaseUntil = clock.GetUtcNow().AddSeconds(settings.Value.LeaseSeconds);
            job.Status = operation == PublicationOperation.Reconcile ? "NeedsReview" : "Publishing";
            job.StepCount++; job.AttemptCount++; job.CanRetry = false;
            if (operation == PublicationOperation.Publish) job.PublishMayHaveOccurred = true;
            platform.Status = Repository.Enum.PlatformPostStatus.Publishing; platform.PublishAttemptCount = job.AttemptCount;
            store.Touch(job);
            attemptId = Guid.NewGuid();
            db.PostPublicationAttempts.Add(new() { Id = attemptId, PublicationId = id, Number = job.AttemptCount,
                Operation = operation.ToString(), StartedAt = clock.GetUtcNow() });
            await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        }
        // No database transaction is open across provider I/O. A separate durable intent precedes every call.
        PublicationResult result;
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(settings.Value.OperationTimeoutSeconds));
        var executeStarted = false;
        try
        {
            if (operation != PublicationOperation.Reconcile)
            {
                var reasons = adapter.Validate(snapshot);
                if (reasons.Count == 0) reasons = await adapter.Preflight(snapshot, credentials, budget.Token);
                if (reasons.Count > 0) result = new(PublicationOutcome.Rejected) { ErrorCode = reasons[0], Retryable = true, ConfirmedNotPublished = true };
                else { executeStarted = true; result = await adapter.Execute(operation, snapshot, checkpoint, credentials, candidateExternalId, budget.Token); }
            }
            else { executeStarted = true; result = await adapter.Execute(operation, snapshot, checkpoint, credentials, candidateExternalId, budget.Token); }
        }
        catch (MetaPublishingException ex)
        {
            result = ex.Result(executeStarted && operation != PublicationOperation.Prepare, operation == PublicationOperation.Reconcile);
            if (operation == PublicationOperation.Prepare && ex.ReadOnlyRequest && result.Retryable && !result.RequiresReauth)
                result = result with { Outcome = PublicationOutcome.Pending };
        }
        catch (ZaloPublishingException ex)
        {
            result = ex.Result(executeStarted && operation != PublicationOperation.Prepare, operation == PublicationOperation.Reconcile);
        }
        catch (Exception)
        {
            // Exception text can include request URLs/tokens. Persist only a stable error code.
            result = !executeStarted && operation != PublicationOperation.Reconcile
                ? new(PublicationOutcome.Rejected) { ErrorCode = "PUBLICATION_PREFLIGHT_INTERRUPTED", ConfirmedNotPublished = true, Retryable = true }
                : new(PublicationOutcome.Unknown) { ErrorCode = "PUBLICATION_PROVIDER_INTERRUPTED" };
        }
        if (!ValidResult(result, snapshot)) result = new(PublicationOutcome.Unknown) { ErrorCode = "PUBLICATION_INVALID_RESPONSE" };
        using var saveBudget = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await using var finish = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(saveBudget.Token) : null;
        var (current, target, latestAccount) = await store.LockJob(id, saveBudget.Token);
        if (current.LeaseId != lease) return; // Recovered/closed by another worker; stale response cannot overwrite it.
        var attempt = await db.PostPublicationAttempts.SingleAsync(x => x.Id == attemptId, saveBudget.Token);
        attempt.Status = result.Outcome.ToString(); attempt.CompletedAt = clock.GetUtcNow();
        attempt.ErrorCode = result.ErrorCode; attempt.ProviderCode = result.ProviderCode; attempt.ProviderRequestId = result.ProviderRequestId;
        attempt.ExternalPostId = result.ExternalPostId;
        if (result.Checkpoint != null) current.Checkpoint = Json(result.Checkpoint);
        if (result.PublishMayHaveOccurred) current.PublishMayHaveOccurred = true;
        current.NextRunAt = clock.GetUtcNow().AddSeconds(Math.Clamp(result.RetryAfterSeconds, 1, 3600));
        current.LastDispatchedAt = null;
        if (result.Outcome == PublicationOutcome.Succeeded)
        {
            current.ExternalPostId = result.ExternalPostId; current.ExternalPostUrl = result.ExternalPostUrl;
            current.PublishMayHaveOccurred = true; store.State(current, target, "Succeeded");
        }
        else if (result.Outcome == PublicationOutcome.Rejected && (!current.PublishMayHaveOccurred || result.ConfirmedNotPublished))
        {
            current.PublishMayHaveOccurred = false; store.State(current, target, "Failed", result.ErrorCode ?? "PUBLICATION_REJECTED");
            current.CanRetry = result.Retryable && current.AttemptCount < settings.Value.MaxAttempts;
        }
        else if (operation == PublicationOperation.Prepare && result.Outcome is PublicationOutcome.Continue or PublicationOutcome.Pending)
            store.State(current, target, "Publishing", result.ErrorCode);
        else if (snapshot.Platform == Repository.Enum.PlatformType.Zalo && result.Outcome == PublicationOutcome.Pending
            && result.PublishMayHaveOccurred && result.Checkpoint?.ProtectedProviderReference != null)
            store.State(current, target, "Publishing", result.ErrorCode);
        else
        {
            store.State(current, target, current.PublishMayHaveOccurred ? "NeedsReview" : "Failed", result.ErrorCode ?? "PUBLICATION_RESULT_UNKNOWN");
            current.CanRetry = !current.PublishMayHaveOccurred;
        }
        // A missing Zalo process token cannot be recovered by polling. Keep the create error
        // visible and wait for explicit review instead of repeatedly replacing it with "missing reference".
        if (snapshot.Platform == Repository.Enum.PlatformType.Zalo && current.Status == "NeedsReview"
            && Read<PublicationCheckpoint>(current.Checkpoint).ProtectedProviderReference == null)
            current.NextRunAt = null;
        // Only mark the credential revision actually used. Preserve a newer reconnect/refresh.
        if (result.RequiresReauth && latestAccount.Revision == accountRevision)
        {
            latestAccount.Status = "ReauthRequired"; latestAccount.LastErrorCode = result.ErrorCode;
            latestAccount.UpdatedAt = clock.GetUtcNow(); latestAccount.Revision++;
        }
        if (latestAccount.Revision != accountRevision && current.Status == "Publishing")
        {
            store.State(current, target, current.PublishMayHaveOccurred ? "NeedsReview" : "Failed", "PLATFORM_ACCOUNT_CHANGED"); current.CanRetry = !current.PublishMayHaveOccurred;
        }
        if (current.StepCount >= settings.Value.MaxSteps || current.AttemptCount >= settings.Value.MaxAttempts)
        {
            if (current.Status == "Publishing") store.State(current, target, current.PublishMayHaveOccurred ? "NeedsReview" : "Failed", "PUBLICATION_BUDGET_EXCEEDED");
            current.NextRunAt = null;
        }
        await db.SaveChangesAsync(saveBudget.Token); if (finish != null) await finish.CommitAsync(saveBudget.Token);
    }
    private static bool ValidResult(PublicationResult? r, PublicationSnapshot snapshot)
    {
        if (r == null || !Enum.IsDefined(r.Outcome) || r.ErrorCode?.Length > 100 || r.ProviderCode?.Length > 100
            || r.ProviderRequestId?.Length > 200 || r.ExternalPostId?.Length > 200 || r.ExternalPostUrl?.Length > 2048) return false;
        if (r.Outcome == PublicationOutcome.Succeeded && string.IsNullOrWhiteSpace(r.ExternalPostId)) return false;
        if (r.ExternalPostUrl != null && !Service.PublicUrl(r.ExternalPostUrl)) return false;
        return r.Checkpoint == null || (r.Checkpoint.Media != null && r.Checkpoint.Media.Length <= snapshot.Images.Length
            && r.Checkpoint.Media.Select(x => x.Order).Distinct().Count() == r.Checkpoint.Media.Length
            && r.Checkpoint.Media.All(x => snapshot.Images.Any(i => i.Order == x.Order) && !string.IsNullOrWhiteSpace(x.ProviderId) && x.ProviderId.Length <= 200)
            && r.Checkpoint.ContainerId?.Length is not > 200 && r.Checkpoint.ProtectedProviderReference?.Length is not > 32768);
    }
}
