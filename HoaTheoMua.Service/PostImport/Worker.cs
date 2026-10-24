using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostImport;

public sealed partial class Worker(AppDbContext db, PostPublishing.Service publishing, TimeProvider clock,
    AiService.IService? ai = null)
{
    private static readonly string[] BlockingStates = ["Queued", "Scheduled", "Publishing", "NeedsReview", "Succeeded", "Closed"];

    // One durable row per lease. The publication outbox owns actual post sends and its
    // existing duplicate/crash/unknown-result protections. Credentials may refresh before queuing.
    public async Task<bool> ProcessOne(Guid batchId, CancellationToken ct = default)
    {
        var lease = Guid.NewGuid(); Guid rowId; Guid actor;
        await using (var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            var batch = await LockBatch(batchId, ct);
            if (batch == null || batch.Status == "Processed" || batch.LeaseUntil > clock.GetUtcNow()) return false;
            var next = await db.PostImportRows.Where(x => x.BatchId == batchId && x.Status == "Queued").OrderBy(x => x.RowNumber).FirstOrDefaultAsync(ct);
            if (next == null)
            {
                batch.Status = "Processed"; batch.ProcessedAt = clock.GetUtcNow(); batch.LeaseId = null; batch.LeaseUntil = null;
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct); return false;
            }
            batch.Status = "Processing"; batch.LeaseId = lease; batch.LeaseUntil = clock.GetUtcNow().AddMinutes(2);
            next.Attempts++; rowId = next.Id; actor = batch.RequestedBy;
            await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        }
        try
        {
            var postId = await EnsurePost(batchId, rowId, actor, lease, ct);
            await PrepareAi(batchId, rowId, actor, lease, postId, ct);
            var row = await db.PostImportRows.AsNoTracking().SingleAsync(x => x.Id == rowId, ct);
            var input = Read<ImportInput>(row.Payload);
            var channels = await db.PostImportChannels.AsNoTracking().Where(x => x.RowId == rowId && x.Status == "Pending").OrderBy(x => x.Platform).ToListAsync(ct);
            foreach (var channel in channels)
            {
                if (!await HasLease(batchId, lease, ct)) return false;
                PostPublication? known = await Known(postId, channel.Platform, ct);
                Guid? publicationId = known?.Id; var disposition = known == null ? "Submitted" : "Existing"; string? error = null;
                if (known == null && input.ScheduledAt == null)
                {
                    if (!await SaveChannel(batchId, lease, channel.Id, null, "Waiting", null, ct)) return false;
                    continue;
                }
                if (known == null)
                {
                    if (channel.PlatformAccountId == null) { disposition = "Rejected"; error = "IMPORT_ACCOUNT_REQUIRED"; }
                    else
                    {
                        try
                        {
                            var job = await publishing.PublishImported(postId, Service.Name(channel.Platform), channel.PlatformAccountId.Value,
                                input.ScheduledAt, actor, "bulk:" + channel.Id.ToString("N"), Read<Preparation>(row.Preparation).Fingerprints![Service.Name(channel.Platform)], ct);
                            publicationId = job.PublicationId;
                        }
                        catch (AppException ex)
                        {
                            if (ex.MessageCode == "PLATFORM_REFRESH_IN_PROGRESS") throw;
                            // Another batch may have submitted the same code/channel while this one
                            // was preparing. Attach its exact durable job rather than creating another.
                            known = await Known(postId, channel.Platform, ct);
                            if (known != null) { publicationId = known.Id; disposition = "Existing"; }
                            else { disposition = "Rejected"; error = ex.MessageCode; }
                        }
                    }
                }
                if (!await SaveChannel(batchId, lease, channel.Id, publicationId, disposition, error, ct)) return false;
            }
            await Finish(batchId, rowId, lease, null, retry: false, ct);
            return true;
        }
        catch (HoaTheoMua.Service.PlatformAccount.AccountException ex) when (ex.MessageCode == "PLATFORM_REFRESH_IN_PROGRESS")
        {
            await Finish(batchId, rowId, lease, null, retry: true, ct, defer: true);
            return false;
        }
        catch (ImportException ex)
        {
            await Finish(batchId, rowId, lease, new("Bai_dang", 0, ex.MessageCode, ex.Message), retry: false, ct);
            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { return false; }
        catch (Exception)
        {
            // Do not persist exception text; it may include SQL, media URLs or credentials.
            await Finish(batchId, rowId, lease, new("Bai_dang", 0, "IMPORT_PROCESSING_FAILED", Service.ErrorMessage("IMPORT_PROCESSING_FAILED")!), retry: true, ct);
            return false;
        }
    }
    private async Task<PostImportBatch?> LockBatch(Guid id, CancellationToken ct)
    {
        foreach (var tracked in db.ChangeTracker.Entries<PostImportBatch>().Where(x => x.Entity.Id == id).ToArray()) tracked.State = EntityState.Detached;
        return db.Database.IsRelational()
            ? (await db.PostImportBatches.FromSqlInterpolated($"SELECT * FROM \"PostImportBatch\" WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
            : await db.PostImportBatches.SingleOrDefaultAsync(x => x.Id == id, ct);
    }
    private Task<bool> HasLease(Guid id, Guid lease, CancellationToken ct) =>
        db.PostImportBatches.AsNoTracking().AnyAsync(x => x.Id == id && x.LeaseId == lease && x.LeaseUntil > clock.GetUtcNow(), ct);
    private async Task<bool> SaveChannel(Guid id, Guid lease, Guid channelId, Guid? publicationId, string status, string? error, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var batch = await LockBatch(id, ct);
        if (batch?.LeaseId != lease) return false;
        var channel = await db.PostImportChannels.SingleAsync(x => x.Id == channelId, ct);
        if (channel.Status == "Pending") { channel.PublicationId = publicationId; channel.Status = status; channel.ErrorCode = error; }
        batch.LeaseUntil = clock.GetUtcNow().AddMinutes(2);
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return true;
    }
    private async Task Finish(Guid id, Guid rowId, Guid lease, ImportIssue? issue, bool retry, CancellationToken ct, bool defer = false)
    {
        db.ChangeTracker.Clear();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var batch = await LockBatch(id, ct); if (batch?.LeaseId != lease) return;
        var row = await db.PostImportRows.SingleAsync(x => x.Id == rowId, ct);
        if (defer) row.Attempts = Math.Max(0, row.Attempts - 1);
        if (!defer && (!retry || row.Attempts >= 3))
        {
            row.Status = issue == null ? "Processed" : "Invalid";
            if (issue != null)
            {
                var ownedTargets = Read<Preparation>(row.Preparation).OwnedTargets ?? [];
                if (row.BasePostId.HasValue)
                    foreach (var target in await db.PlatformPosts.Where(x => ownedTargets.Contains(x.Id) && x.Status == PlatformPostStatus.Generating).ToListAsync(ct))
                    { target.Status = PlatformPostStatus.Failed; target.LastError = issue.Message; }
                row.Issues = Json(new[] { issue with { RowNumber = row.RowNumber } });
                foreach (var pending in await db.PostImportChannels.Where(x => x.RowId == rowId && x.Status == "Pending").ToListAsync(ct))
                { pending.Status = "Rejected"; pending.ErrorCode = issue.Code; }
            }
        }
        var more = await db.PostImportRows.AnyAsync(x => x.BatchId == id && x.Id != rowId && x.Status == "Queued", ct)
            || row.Status == "Queued";
        batch.Status = more ? "Queued" : "Processed"; batch.ProcessedAt = more ? null : clock.GetUtcNow();
        batch.LeaseId = null; batch.LeaseUntil = null;
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
    }
    private async Task<PostPublication?> Known(Guid postId, PlatformType platform, CancellationToken ct)
    {
        return await (from job in db.PostPublications.AsNoTracking()
            join target in db.PlatformPosts.AsNoTracking() on job.PlatformPostId equals target.Id
            where job.BasePostId == postId && target.Platform == platform && BlockingStates.Contains(job.Status)
            orderby job.CreatedAt descending, job.Id descending select job).FirstOrDefaultAsync(ct);
    }
    internal static string Fingerprint(string title, string content, string? hashtag, IEnumerable<string> images) =>
        ContentHash(title, content, hashtag, images);
    private async Task<Guid> EnsurePost(Guid batchId, Guid rowId, Guid actor, Guid lease, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        if ((await LockBatch(batchId, ct))?.LeaseId != lease) throw new ImportException(409, "IMPORT_LEASE_CHANGED", "Tác vụ đang được xử lý ở tiến trình khác.");
        var row = await db.PostImportRows.SingleAsync(x => x.Id == rowId, ct);
        var input = Read<ImportInput>(row.Payload);
        if (row.BasePostId.HasValue)
        {
            if (Read<Preparation>(row.Preparation).Fingerprints == null)
            {
                row.Preparation = Json(new Preparation(Fingerprints: input.Platforms.ToDictionary(Service.Name,
                    platform => Fingerprint(input.Title, input.Content, input.Hashtag,
                        input.Images.Where(x => x.Platform == null || x.Platform == platform).OrderBy(x => x.Order).Select(x => x.Url)))));
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
            }
            return row.BasePostId.Value;
        }
        await Service.Advisory(db, $"post-import-code:{actor}:{input.Code}", ct);
        var identity = await db.ImportedPostIdentities.SingleOrDefaultAsync(x => x.RequestedBy == actor && x.Code == input.Code, ct);
        var legacy = !input.UseAiContent && input.Images.All(x => x.Platform == null && x.Ratio == null && !x.UseAi && string.IsNullOrEmpty(x.Instruction));
        var wantedHash = Fingerprint(input.Title, input.Content, input.Hashtag, legacy
            ? input.Images.Select(x => x.Url) : input.Images.Select(x => Json(x)).Append(input.UseAiContent.ToString()));
        BasePost post; var changed = identity == null;
        if (identity == null)
        {
            post = new BasePost { Id = Guid.NewGuid(), Title = input.Title, Content = input.Content, Hashtag = input.Hashtag, CreatedAt = clock.GetUtcNow() };
            db.BasePosts.Add(post);
            db.ImportedPostIdentities.Add(identity = new() { RequestedBy = actor, Code = input.Code, BasePostId = post.Id, ContentHash = wantedHash, CreatedAt = clock.GetUtcNow() });
            await db.SaveChangesAsync(ct);
        }
        else
        {
            post = db.Database.IsRelational()
                ? (await db.BasePosts.FromSqlInterpolated($"SELECT * FROM \"BasePost\" WHERE \"Id\" = {identity.BasePostId} FOR UPDATE").ToListAsync(ct)).Single()
                : await db.BasePosts.SingleAsync(x => x.Id == identity.BasePostId, ct);
            if (post.IsDeleted) throw new ImportException(409, "IMPORT_SOURCE_DELETED", Service.ErrorMessage("IMPORT_SOURCE_DELETED")!);
            var urls = await db.BasePostImages.Where(x => x.BasePostId == post.Id && !x.IsDeleted).OrderBy(x => x.SortOrder).Select(x => x.ImageUrl).ToListAsync(ct);
            changed = identity.ContentHash != wantedHash || Fingerprint(post.Title, post.Content, post.Hashtag, urls) != Fingerprint(input.Title, input.Content, input.Hashtag, input.Images.Select(x => x.Url));
            if (await db.PlatformPosts.AnyAsync(x => x.BasePostId == post.Id && !x.IsDeleted && x.Status == PlatformPostStatus.Generating, ct)
                || changed && await db.PostPublications.AnyAsync(x => x.BasePostId == post.Id && BlockingStates.Contains(x.Status), ct))
                throw new ImportException(409, "IMPORT_CONTENT_CHANGED", Service.ErrorMessage("IMPORT_CONTENT_CHANGED")!);
        }
        var source = await db.BasePostImages.Where(x => x.BasePostId == post.Id && !x.IsDeleted).OrderBy(x => x.SortOrder).ToListAsync(ct);
        if (changed)
        {
            foreach (var old in source) old.IsDeleted = true;
            // Release filtered unique sort positions before inserting replacement source rows.
            await db.SaveChangesAsync(ct);
            source = input.Images.Select((x, index) => new BasePostImage { Id = Guid.NewGuid(), BasePostId = post.Id, ImageUrl = x.Url,
                SortOrder = index + 1, CreatedAt = clock.GetUtcNow() }).ToList();
            db.BasePostImages.AddRange(source);
            post.Title = input.Title; post.Content = input.Content; post.Hashtag = input.Hashtag; post.UpdatedAt = clock.GetUtcNow();
            identity.ContentHash = wantedHash;
            await db.SaveChangesAsync(ct);
        }
        var platforms = await db.PlatformPosts.Where(x => x.BasePostId == post.Id && !x.IsDeleted).ToListAsync(ct);
        var requested = changed ? platforms.Select(x => x.Platform).Union(input.Platforms).ToArray() : input.Platforms;
        foreach (var platform in requested)
        {
            if (await Known(post.Id, platform, ct) != null) continue;
            var target = platforms.SingleOrDefault(x => x.Platform == platform);
            if (target != null && !changed) continue;
            if (target == null)
            {
                target = new() { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = platform, Status = PlatformPostStatus.Ready, UserId = actor, CreatedAt = clock.GetUtcNow() };
                db.PlatformPosts.Add(target); await db.SaveChangesAsync(ct);
            }
            var content = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = target.Id, Source = ContentSource.Manual,
                Title = input.Title, Content = input.Content, Hashtag = input.Hashtag, UserId = actor, CreatedAt = clock.GetUtcNow(), ParentGeneratedPostId = target.OfficialGenerationId };
            db.GeneratedPosts.Add(content);
            db.PlatformPostImages.RemoveRange(await db.PlatformPostImages.Where(x => x.PlatformPostId == target.Id).ToListAsync(ct));
            await db.SaveChangesAsync(ct);
            foreach (var selection in input.Images.Select((item, index) => (item, index)).Where(x => x.item.Platform == null || x.item.Platform == platform))
            {
                var image = source[selection.index];
                var ratio = selection.item.Ratio ?? AspectRatioType.Square1x1;
                var original = await db.GeneratedPostImages.Where(x => x.SourceBasePostImageId == image.Id && x.StorageProvider == "ImportedUrl" && x.RatioCode == ratio && x.Status == ImageStatus.Succeeded)
                    .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
                if (original == null)
                {
                    original = new() { Id = Guid.NewGuid(), BasePostId = post.Id, SourceBasePostImageId = image.Id,
                        SourceImageUrl = image.ImageUrl, ImageUrl = image.ImageUrl, RatioCode = ratio,
                        StorageProvider = "ImportedUrl", Status = ImageStatus.Succeeded, CreatedAt = clock.GetUtcNow() };
                    db.GeneratedPostImages.Add(original);
                }
                db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = target.Id, SourceBasePostImageId = image.Id,
                    RatioCode = ratio, SelectedGeneratedPostImageId = original.Id, SortOrder = selection.item.Order, CreatedAt = clock.GetUtcNow() });
            }
            target.OfficialGenerationId = content.Id; target.SelectedAt = clock.GetUtcNow(); target.UpdatedAt = clock.GetUtcNow();
            target.Status = PlatformPostStatus.Ready; target.LastError = null; target.PublishError = null;
            await db.SaveChangesAsync(ct);
        }
        row.BasePostId = post.Id;
        var fingerprints = input.Platforms.ToDictionary(Service.Name, platform => Fingerprint(input.Title, input.Content, input.Hashtag,
            input.Images.Where(x => x.Platform == null || x.Platform == platform).OrderBy(x => x.Order).Select(x => x.Url)));
        row.Preparation = Json(new Preparation(Fingerprints: fingerprints));
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return post.Id;
    }
}

public sealed class HostedService(IServiceScopeFactory scopes, ILogger<HostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Guid[] ids;
                await using (var scope = scopes.CreateAsyncScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
                    ids = await db.PostImportBatches.AsNoTracking().Where(x => x.Status != "Processed" && (x.LeaseUntil == null || x.LeaseUntil <= now))
                        .OrderBy(x => x.CreatedAt).Take(10).Select(x => x.Id).ToArrayAsync(stoppingToken);
                }
                foreach (var id in ids)
                    for (var i = 0; i < 20 && !stoppingToken.IsCancellationRequested; i++)
                    {
                        await using var scope = scopes.CreateAsyncScope();
                        if (!await scope.ServiceProvider.GetRequiredService<Worker>().ProcessOne(id, stoppingToken)) break;
                    }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception) { logger.LogError("Post import recovery is unavailable; durable rows will be retried."); }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
