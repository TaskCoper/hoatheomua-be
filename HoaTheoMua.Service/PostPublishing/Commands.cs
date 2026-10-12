using HoaTheoMua.Repository.Entity;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostPublishing;

public sealed partial class Service
{
    public async Task<Response.Job> Retry(Guid publicationId, Request.Revision request, CancellationToken ct = default)
    {
        Validate(request); var actor = user.GetRequiredUserId(); settings.Value.Validate();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var (job, platform, account) = await store.LockJob(publicationId, ct); Expected(job, request.ExpectedRevision);
        if (job.Status != "Failed" || !job.CanRetry || job.PublishMayHaveOccurred || job.AttemptCount >= settings.Value.MaxAttempts)
            throw Error(409, "PUBLICATION_RETRY_NOT_ALLOWED", "Chỉ thử lại tác vụ được xác nhận chưa đăng và có lỗi cho phép thử lại.");
        if (job.ErrorCode is "META_RATE_LIMITED" or "ZALO_RATE_LIMITED" && job.NextRunAt > clock.GetUtcNow())
            throw Error(429, job.ErrorCode, "Nền tảng đang giới hạn yêu cầu; chờ hết thời gian backoff trước khi thử lại.");
        if (platform.IsDeleted || await db.BasePosts.AnyAsync(x => x.Id == job.BasePostId && x.IsDeleted, ct)) throw Error(409, "PUBLICATION_SOURCE_DELETED", "Bài nguồn đã bị xóa.");
        if (platform.Status == Repository.Enum.PlatformPostStatus.Generating
            || await db.PostPublications.AnyAsync(x => x.PlatformPostId == job.PlatformPostId && x.Id != job.Id
                && (ActiveStates.Contains(x.Status) || PreventRepostStates.Contains(x.Status)), ct))
            throw Error(409, "PUBLICATION_EXISTS", "Bài đang có tác vụ khác hoặc đã đăng.");
        var snapshot = Read<PublicationSnapshot>(job.Snapshot);
        if (!settings.Value.Enabled(snapshot.Platform) || !adapters.Any(x => x.Platform == snapshot.Platform)) throw Error(503, "PUBLISHING_DISABLED", "Kênh chưa sẵn sàng đăng.");
        if (AccountError(account, snapshot, clock.GetUtcNow()) is { } error) throw Error(409, error, "Tài khoản chưa sẵn sàng.");
        job.StepCount = 0; job.NextRunAt = job.LastDispatchedAt = clock.GetUtcNow();
        store.State(job, platform, "Queued"); AddReview(job.Id, actor, "Retry", "Retry giữ nguyên snapshot.");
        await bus.Publish(new PublicationRequested(job.Id, job.Revision), ct);
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return ToResponse(job);
    }
    public async Task<Response.Job> Cancel(Guid publicationId, Request.Revision request, CancellationToken ct = default)
    {
        Validate(request); var actor = user.GetRequiredUserId();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var (job, platform, _) = await store.LockJob(publicationId, ct); Expected(job, request.ExpectedRevision);
        if (job.Status != "Queued" || job.LeaseId != null || job.PublishMayHaveOccurred)
            throw Error(409, "PUBLICATION_CANCEL_NOT_ALLOWED", "Tác vụ đã bắt đầu; không thể xác nhận hủy.");
        store.State(job, platform, "Cancelled"); AddReview(job.Id, actor, "Cancel", "Hủy trước khi worker bắt đầu.");
        if (platform.OfficialGenerationId.HasValue && !await db.PlatformPostImages.AnyAsync(x => x.PlatformPostId == platform.Id && x.SelectedGeneratedPostImageId != null, ct))
            platform.Status = Repository.Enum.PlatformPostStatus.Generated;
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return ToResponse(job);
    }
    public Task<Response.Job> Recheck(Guid publicationId, Request.Revision request, CancellationToken ct = default)
        => Review(publicationId, request, "Recheck", "Yêu cầu đối soát lại.", null, ct);
    public Task<Response.Job> Resolve(Guid publicationId, Request.Resolve request, CancellationToken ct = default)
    {
        Validate(request);
        if (request.Action is not ("ConfirmPublished" or "ConfirmNotPublished" or "Close") || string.IsNullOrWhiteSpace(request.Note)
            || (request.Action == "ConfirmPublished" && string.IsNullOrWhiteSpace(request.ExternalPostId)))
            throw Error(400, "PUBLICATION_INVALID_REVIEW", "Cần action, ghi chú và ID bài khi xác nhận đã đăng.");
        return Review(publicationId, request, request.Action, request.Note, request.ExternalPostId, ct);
    }
    private async Task<Response.Job> Review(Guid id, Request.Revision request, string action, string note, string? candidate, CancellationToken ct)
    {
        Validate(request); var actor = user.GetRequiredUserId(); long revision;
        await using (var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            var (job, platform, _) = await store.LockJob(id, ct); Expected(job, request.ExpectedRevision);
            if (job.Status != "NeedsReview" || job.LeaseId != null)
                throw Error(409, "PUBLICATION_REVIEW_NOT_ALLOWED", "Tác vụ chưa cần đối soát hoặc đang được kiểm tra.");
            if (action == "Close") store.State(job, platform, "Closed", "PUBLICATION_CLOSED_UNRESOLVED");
            else { job.NextRunAt = clock.GetUtcNow(); store.Touch(job); }
            AddReview(id, actor, action, note, candidate); revision = job.Revision;
            await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
            if (action == "Close") return ToResponse(job);
        }
        // Admin input is evidence to check, not permission to assert provider success/absence.
        await worker.Process(id, revision, ct, candidate, manualReview: true);
        return ToResponse(await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == id, ct));
    }
    private void AddReview(Guid jobId, Guid actor, string action, string note, string? externalId = null)
        => db.PostPublicationReviews.Add(new() { Id = Guid.NewGuid(), PublicationId = jobId, ActorId = actor,
            Action = action, Note = note, ExternalPostId = externalId, CreatedAt = clock.GetUtcNow() });
}
