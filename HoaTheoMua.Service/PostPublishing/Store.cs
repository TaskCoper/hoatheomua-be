using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Account = HoaTheoMua.Repository.Entity.PlatformAccount;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class Store(AppDbContext db, TimeProvider clock)
{
    public static readonly string[] ActiveStates = ["Queued", "Scheduled", "Publishing", "NeedsReview"];
    public static readonly string[] PreventRepostStates = ["Succeeded", "Closed", "NeedsReview"];
    public static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    public static string Json<T>(T value) => JsonSerializer.Serialize(value);
    public static T Read<T>(string value) => JsonSerializer.Deserialize<T>(value) ?? throw new InvalidOperationException("Missing publication data");

    public async Task<PlatformPost> LockPlatform(Guid id, PlatformType platform, CancellationToken ct, bool allowDeleted = false)
    {
        var post = await LockBase(id, ct);
        if (post.IsDeleted && !allowDeleted) throw new NotFoundException("Post not found");
        var target = await db.PlatformPosts.AsNoTracking().SingleOrDefaultAsync(x => x.BasePostId == id && x.Platform == platform && (allowDeleted || !x.IsDeleted), ct)
            ?? throw new NotFoundException("Platform post not found");
        return await LockPlatformId(target.Id, ct);
    }
    private async Task<BasePost> LockBase(Guid id, CancellationToken ct)
    {
        Detach<BasePost>(x => x.Id == id);
        return (db.Database.IsRelational()
            ? (await db.BasePosts.FromSqlInterpolated($"SELECT * FROM \"BasePost\" WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
            : await db.BasePosts.SingleOrDefaultAsync(x => x.Id == id, ct)) ?? throw new NotFoundException("Post not found");
    }
    private async Task<PlatformPost> LockPlatformId(Guid id, CancellationToken ct)
    {
        Detach<PlatformPost>(x => x.Id == id);
        return (db.Database.IsRelational()
            ? (await db.PlatformPosts.FromSqlInterpolated($"SELECT * FROM \"PlatformPost\" WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(ct)).Single()
            : await db.PlatformPosts.SingleAsync(x => x.Id == id, ct));
    }
    public async Task<Account> LockAccount(Guid id, CancellationToken ct)
    {
        Detach<Account>(x => x.Id == id);
        return (db.Database.IsRelational()
            ? (await db.PlatformAccounts.FromSqlInterpolated($"SELECT * FROM \"PlatformAccount\" WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
            : await db.PlatformAccounts.SingleOrDefaultAsync(x => x.Id == id, ct)) ?? throw new NotFoundException("Platform account not found");
    }
    public async Task<(PostPublication Job, PlatformPost Platform, Account Account)> LockJob(Guid id, CancellationToken ct)
    {
        var meta = await db.PostPublications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Publication not found");
        await LockBase(meta.BasePostId, ct);
        var platform = await LockPlatformId(meta.PlatformPostId, ct);
        var account = await LockAccount(meta.PlatformAccountId, ct);
        Detach<PostPublication>(x => x.Id == id);
        var job = db.Database.IsRelational()
            ? (await db.PostPublications.FromSqlInterpolated($"SELECT * FROM \"PostPublication\" WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(ct)).Single()
            : await db.PostPublications.SingleAsync(x => x.Id == id, ct);
        return (job, platform, account);
    }
    private void Detach<T>(Func<T, bool> match) where T : class
    {
        foreach (var entry in db.ChangeTracker.Entries<T>().Where(x => match(x.Entity)).ToArray()) entry.State = EntityState.Detached;
    }
    public static void Expected(PostPublication job, long revision)
    {
        if (revision < 1 || job.Revision != revision) throw Error(409, "PUBLICATION_CHANGED", "Tác vụ đã thay đổi; tải lại trước khi thao tác.");
    }
    public static PublicationException Error(int status, string code, string message) => new(status, code, message);
    public void Touch(PostPublication job) { job.Revision++; job.UpdatedAt = clock.GetUtcNow(); }
    public void State(PostPublication job, PlatformPost platform, string state, string? error = null)
    {
        job.Status = state; job.ErrorCode = error; job.CanRetry = false;
        job.LeaseId = null; job.LeaseUntil = null;
        job.CompletedAt = state is "Succeeded" or "Failed" or "Cancelled" or "Closed" ? clock.GetUtcNow() : null;
        platform.Status = state switch
        {
            "Scheduled" => PlatformPostStatus.Scheduled,
            "Queued" or "Publishing" or "NeedsReview" => PlatformPostStatus.Publishing,
            "Succeeded" => PlatformPostStatus.Published,
            "Failed" or "Closed" => PlatformPostStatus.PublishFailed,
            _ => platform.OfficialGenerationId.HasValue ? PlatformPostStatus.Ready : PlatformPostStatus.Draft
        };
        platform.PlatformAccountId = job.PlatformAccountId;
        platform.ScheduledAt = state == "Cancelled" ? null : job.ScheduledAt;
        platform.PublishError = error; platform.PublishAttemptCount = job.AttemptCount;
        platform.UpdatedAt = clock.GetUtcNow();
        if (state == "Succeeded") { platform.ExternalPostId = job.ExternalPostId; platform.PublishedAt = job.CompletedAt; }
        Touch(job);
    }
    public static Response.Job ToResponse(PostPublication x) => new(x.Id, x.Status, x.Revision, x.RequestedBy,
        x.CreatedAt, x.UpdatedAt, x.CompletedAt, x.AttemptCount, x.CanRetry, x.ExternalPostId,
        x.ExternalPostUrl, x.ErrorCode, x.PublishMayHaveOccurred, x.ScheduledAt);

    public static string? AccountError(Account a, PublicationSnapshot s, DateTimeOffset now)
    {
        if (a.IsDeleted || a.Status != "Connected" || string.IsNullOrWhiteSpace(a.ProtectedCredentials)
            || a.AccessTokenExpiresAt <= now) return "PLATFORM_REAUTH_REQUIRED";
        if (a.Platform != s.Platform || a.ExternalAccountId != s.ExternalAccountId || a.PageId != s.PageId) return "PLATFORM_ACCOUNT_MISMATCH";
        if (a.RefreshLeaseId != null) return "PLATFORM_REFRESH_IN_PROGRESS";
        var scopes = s.Platform == PlatformType.Facebook ? new[] { "pages_read_engagement", "pages_manage_posts" }
            : new[] { "pages_read_engagement", "instagram_basic", "instagram_content_publish" };
        return scopes.Except(a.GrantedScopes).Any() ? "PLATFORM_PERMISSION_REQUIRED" : null;
    }
}
