using System.ComponentModel.DataAnnotations;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostPublishing;

public sealed partial class Service(AppDbContext db, Store store, ICurrentUserService user, IPublishEndpoint bus,
    IEnumerable<IPublishingAdapter> adapters, IOptions<PublishingOptions> settings,
    IOptions<PlatformAccount.PlatformAccountOptions> accountSettings, TimeProvider clock, Worker worker) : IService
{
    public async Task<Response.Preview> Preview(Guid id, string platform, Guid accountId, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var target = await store.LockPlatform(id, PlatformAccount.Service.Parse(platform), ct);
        var account = await store.LockAccount(accountId, ct);
        var result = await MakePreview(target, account, ct);
        if (tx != null) await tx.CommitAsync(ct);
        return result;
    }
    private async Task<Response.Preview> MakePreview(PlatformPost target, Repository.Entity.PlatformAccount account, CancellationToken ct)
    {
        var content = await db.GeneratedPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == target.OfficialGenerationId && x.PlatformPostId == target.Id, ct);
        var slots = await db.PlatformPostImages.AsNoTracking().Where(x => x.PlatformPostId == target.Id).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        var ids = slots.Select(x => x.SelectedGeneratedPostImageId).ToArray();
        var images = await db.GeneratedPostImages.AsNoTracking().Where(x => ids.Contains(x.Id) && x.BasePostId == target.BasePostId).ToDictionaryAsync(x => x.Id, ct);
        var selected = new List<PublicationImage>(); var reasons = new List<string>();
        foreach (var slot in slots)
        {
            if (slot.SelectedGeneratedPostImageId is not Guid imageId || !images.TryGetValue(imageId, out var image)
                || image.Status != ImageStatus.Succeeded || image.RatioCode != slot.RatioCode || image.SourceBasePostImageId != slot.SourceBasePostImageId
                || !PublicUrl(image.ImageUrl)) { reasons.Add("INVALID_SELECTED_IMAGE"); continue; }
            selected.Add(new(image.Id, image.ImageUrl!, slot.SortOrder));
        }
        if (content == null || string.IsNullOrWhiteSpace(content.Content)) reasons.Add("MISSING_CONTENT");
        if (selected.Count == 0) reasons.Add("MISSING_IMAGES");
        var text = string.Join("\n\n", new[] { content?.Title, content?.Content, content?.Hashtag }.Where(x => !string.IsNullOrWhiteSpace(x)));
        var snapshot = new PublicationSnapshot(target.BasePostId, target.Id, account.Id, account.ExternalAccountId, account.PageId, target.Platform,
            target.Platform == PlatformType.Zalo ? "v2.0" : accountSettings.Value.Meta.GraphApiVersion, "postgen-text-v1", content?.Id ?? Guid.Empty, content?.Title, content?.Content ?? "", content?.Hashtag, text, selected.ToArray(),
            target.Platform == PlatformType.Zalo ? new PublicationArticle(settings.Value.ZaloAuthor, content?.Title ?? "") : null);
        if (target.Status == PlatformPostStatus.Generating) reasons.Add("GENERATION_IN_PROGRESS");
        if (await db.PostPublications.AnyAsync(x => x.PlatformPostId == target.Id && (ActiveStates.Contains(x.Status) || PreventRepostStates.Contains(x.Status)), ct)
            || target.Status is PlatformPostStatus.Scheduled or PlatformPostStatus.Publishing or PlatformPostStatus.Published) reasons.Add("PUBLICATION_EXISTS");
        if (!settings.Value.Enabled(target.Platform)) reasons.Add("PUBLISHING_DISABLED");
        if (!System.Text.RegularExpressions.Regex.IsMatch(snapshot.ApiVersion, @"^v\d+\.\d+$")) reasons.Add("PLATFORM_API_VERSION_REQUIRED");
        if (AccountError(account, snapshot, clock.GetUtcNow()) is { } error) reasons.Add(error);
        var adapter = adapters.SingleOrDefault(x => x.Platform == target.Platform);
        if (adapter == null) reasons.Add("PUBLISHING_ADAPTER_UNAVAILABLE"); else reasons.AddRange(adapter.Validate(snapshot));
        // Revision includes selected values, formatter/version and account connection revision, not transient job state.
        var revision = Hash(Json(new { snapshot, accountRevision = account.Revision, target.SelectedAt }));
        return new(revision, snapshot, reasons.Count == 0, reasons.Distinct().ToArray());
    }
    internal static bool PublicUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var u)
        && (u.Scheme == "https" || u.Scheme == "http") && !u.IsLoopback && u.UserInfo.Length == 0 && value!.Length <= 2048;
    private static void Validate(object input)
    {
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, true)) throw Error(400, "PUBLICATION_INVALID_REQUEST", errors[0].ErrorMessage!);
    }
    public Task<Response.Job> Publish(Guid id, string platform, Request.Publish request, CancellationToken ct = default)
        => CreatePublication(id, platform, request, null, ct);
    private async Task<Response.Job> CreatePublication(Guid id, string platform, Request.Publish request, DateTimeOffset? scheduledAt, CancellationToken ct)
    {
        Validate(request); var actor = user.GetRequiredUserId(); settings.Value.Validate();
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.AccountId == Guid.Empty) throw Error(400, "PUBLICATION_INVALID_REQUEST", "Thiếu tài khoản hoặc idempotency key.");
        var hash = scheduledAt.HasValue ? Hash(Json(new { operation = "Schedule", request.AccountId, request.ExpectedRevision, scheduledAt }))
            : Hash(Json(new { request.AccountId, request.ExpectedRevision }));
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var target = await store.LockPlatform(id, PlatformAccount.Service.Parse(platform), ct);
        var existing = await db.PostPublications.AsNoTracking().SingleOrDefaultAsync(x => x.PlatformPostId == target.Id && x.IdempotencyKey == request.IdempotencyKey, ct);
        if (existing != null)
        {
            if (existing.RequestHash != hash) throw Error(409, "IDEMPOTENCY_CONFLICT", "Key đã được dùng với yêu cầu khác.");
            if (tx != null) await tx.CommitAsync(ct);
            return ToResponse(existing);
        }
        var account = await store.LockAccount(request.AccountId, ct);
        if (scheduledAt.HasValue) ValidateScheduleTime(scheduledAt.Value);
        var preview = await MakePreview(target, account, ct);
        if (preview.Revision != request.ExpectedRevision) throw Error(409, "PUBLICATION_PREVIEW_CHANGED", "Nội dung, ảnh, tài khoản hoặc formatter đã đổi; xem preview lại.");
        if (!preview.CanPublish) throw Error(preview.Reasons.Any(x => x is "PUBLISHING_DISABLED" or "PUBLISHING_ADAPTER_UNAVAILABLE") ? 503 : 409,
            preview.Reasons[0], "Bài chưa đủ điều kiện tạo tác vụ: " + string.Join(", ", preview.Reasons));
        var now = clock.GetUtcNow();
        var job = new PostPublication { Id = Guid.NewGuid(), BasePostId = id, PlatformPostId = target.Id, PlatformAccountId = account.Id,
            RequestedBy = actor, IdempotencyKey = request.IdempotencyKey, RequestHash = hash, Snapshot = Json(preview.Snapshot),
            CreatedAt = now, UpdatedAt = now, ScheduledAt = scheduledAt, NextRunAt = scheduledAt ?? now,
            LastDispatchedAt = scheduledAt.HasValue ? null : now };
        store.State(job, target, scheduledAt.HasValue ? "Scheduled" : "Queued"); db.PostPublications.Add(job);
        if (scheduledAt.HasValue) AddReview(job.Id, actor, "Schedule", Json(new { scheduledAt }));
        else await bus.Publish(new PublicationRequested(job.Id, job.Revision), ct);
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return ToResponse(job);
    }
    public async Task<BasePaginationResponse> List(Guid id, string platform, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        user.GetRequiredUserId(); var type = PlatformAccount.Service.Parse(platform);
        var target = await db.PlatformPosts.AsNoTracking().SingleOrDefaultAsync(x => x.BasePostId == id && x.Platform == type, ct)
            ?? throw new Exceptions.NotFoundException("Platform post not found");
        var query = db.PostPublications.AsNoTracking().Where(x => x.PlatformPostId == target.Id);
        pageIndex = Math.Clamp(pageIndex, 1, 1_000_000); pageSize = Math.Clamp(pageSize, 1, 100);
        var count = await query.CountAsync(ct);
        var jobs = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return ApiResponseFactory.BasePagination(jobs.Select(ToResponse).ToList(), pageIndex, pageSize, count);
    }
    public async Task<Response.Detail> Get(Guid publicationId, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct) : null;
        var job = await db.PostPublications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == publicationId, ct) ?? throw new Exceptions.NotFoundException("Publication not found");
        var attempts = await db.PostPublicationAttempts.AsNoTracking().Where(x => x.PublicationId == publicationId).OrderBy(x => x.Number)
            .Select(x => new Response.Attempt(x.Number, x.Operation, x.Status, x.StartedAt, x.CompletedAt, x.ErrorCode, x.ProviderCode, x.ProviderRequestId, x.ExternalPostId)).ToListAsync(ct);
        var reviews = await db.PostPublicationReviews.AsNoTracking().Where(x => x.PublicationId == publicationId).OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Select(x => new Response.Review(x.ActorId, x.Action, x.Note, x.ExternalPostId, x.CreatedAt)).ToListAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
        return new(ToResponse(job), Read<PublicationSnapshot>(job.Snapshot), attempts, reviews);
    }
}
