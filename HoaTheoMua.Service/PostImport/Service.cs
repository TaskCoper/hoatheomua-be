using System.Security.Cryptography;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostImport;

public sealed class Service(AppDbContext db, ICurrentUserService user, WorkbookReader reader, TimeProvider clock) : IService
{
    public async Task<Response.Settings> GetSettings(CancellationToken ct = default)
    {
        var actor = user.GetRequiredUserId();
        var accounts = await db.PostImportAccounts.AsNoTracking().Where(x => x.RequestedBy == actor).OrderBy(x => x.Platform).ToListAsync(ct);
        return new(accounts.Select(x => new Response.Account(Name(x.Platform), x.PlatformAccountId)).ToArray());
    }
    public async Task<Response.Settings> SaveSettings(Request.Settings request, CancellationToken ct = default)
    {
        var actor = user.GetRequiredUserId();
        if (request.Accounts == null || request.Accounts.Length > 3) throw new ImportException(400, "IMPORT_DEFAULTS_INVALID", "Chỉ chọn tối đa một tài khoản cho mỗi kênh.");
        var selected = request.Accounts.Select(x => (Platform: PlatformAccount.Service.Parse(x.Platform), x.AccountId)).ToArray();
        if (selected.Select(x => x.Platform).Distinct().Count() != selected.Length) throw new ImportException(400, "IMPORT_DEFAULTS_INVALID", "Mỗi kênh chỉ có một tài khoản mặc định.");
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        await Advisory(db, $"post-import-settings:{actor}", ct);
        foreach (var input in selected)
            if (!await db.PlatformAccounts.AnyAsync(x => x.Id == input.AccountId && !x.IsDeleted && x.Platform == input.Platform && x.Status == "Connected", ct))
                throw new ImportException(409, "IMPORT_DEFAULTS_INVALID", "Tài khoản chưa kết nối hoặc không thuộc kênh đã chọn.");
        var stored = await db.PostImportAccounts.Where(x => x.RequestedBy == actor).ToListAsync(ct);
        foreach (var old in stored.Where(x => selected.All(s => s.Platform != x.Platform))) db.PostImportAccounts.Remove(old);
        foreach (var input in selected)
        {
            var setting = stored.SingleOrDefault(x => x.Platform == input.Platform);
            if (setting == null) db.PostImportAccounts.Add(setting = new() { RequestedBy = actor, Platform = input.Platform });
            setting.PlatformAccountId = input.AccountId; setting.UpdatedAt = clock.GetUtcNow();
        }
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return await GetSettings(ct);
    }

    public async Task<Response.Summary> Import(Stream stream, string fileName, string requestKey, CancellationToken ct = default)
    {
        var actor = user.GetRequiredUserId();
        if (string.IsNullOrWhiteSpace(requestKey) || requestKey.Length > 128) throw new ImportException(400, "IMPORT_REQUEST_KEY_REQUIRED", "Thiếu mã yêu cầu nhập file.");
        if (!Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) throw new ImportException(400, "IMPORT_FILE_TYPE_INVALID", "Chỉ nhận file .xlsx theo mẫu v2.");
        using var bytes = new MemoryStream(); var buffer = new byte[16384]; int count;
        while ((count = await stream.ReadAsync(buffer, ct)) > 0)
        {
            if (bytes.Length + count > WorkbookReader.MaxFileBytes) throw new ImportException(413, "IMPORT_FILE_TOO_LARGE", "File Excel tối đa 10 MB.");
            bytes.Write(buffer, 0, count);
        }
        var hash = Convert.ToHexString(SHA256.HashData(bytes.GetBuffer().AsSpan(0, (int)bytes.Length)));
        var existing = await db.PostImportBatches.AsNoTracking().SingleOrDefaultAsync(x => x.RequestedBy == actor && x.RequestKey == requestKey, ct);
        if (existing != null) return await Existing(existing, hash, ct);
        bytes.Position = 0; var workbook = reader.Read(bytes, ct);
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        await Advisory(db, $"post-import-request:{actor}:{requestKey}", ct);
        existing = await db.PostImportBatches.AsNoTracking().SingleOrDefaultAsync(x => x.RequestedBy == actor && x.RequestKey == requestKey, ct);
        if (existing != null)
        {
            if (tx != null) await tx.CommitAsync(ct);
            return await Existing(existing, hash, ct);
        }
        var accounts = await db.PostImportAccounts.AsNoTracking().Where(x => x.RequestedBy == actor).ToDictionaryAsync(x => x.Platform, x => x.PlatformAccountId, ct);
        var batch = new PostImportBatch
        {
            Id = Guid.NewGuid(), RequestedBy = actor, RequestKey = requestKey, FileHash = hash,
            FileName = Path.GetFileName(fileName.Replace('\\', '/'))[..Math.Min(Path.GetFileName(fileName.Replace('\\', '/')).Length, 255)],
            CreatedAt = clock.GetUtcNow(), Issues = Json(workbook.Issues)
        };
        db.PostImportBatches.Add(batch);
        foreach (var input in workbook.Rows)
        {
            var row = new PostImportRow { Id = Guid.NewGuid(), BatchId = batch.Id, RowNumber = input.RowNumber,
                Code = input.Input.Code, Payload = Json(input.Input), Issues = Json(input.Issues), Status = input.Issues.Count > 0 ? "Invalid" : "Queued" };
            db.PostImportRows.Add(row);
            if (row.Status == "Invalid") continue;
            foreach (var platform in input.Input.Platforms)
                db.PostImportChannels.Add(new() { Id = Guid.NewGuid(), RowId = row.Id, Platform = platform,
                    PlatformAccountId = accounts.TryGetValue(platform, out var accountId) ? accountId : null });
        }
        if (workbook.Rows.All(x => x.Issues.Count > 0)) { batch.Status = "Processed"; batch.ProcessedAt = clock.GetUtcNow(); }
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return (await Load(batch, ct)).Batch;
    }
    private async Task<Response.Summary> Existing(PostImportBatch batch, string hash, CancellationToken ct)
    {
        if (batch.FileHash != hash) throw new ImportException(409, "IMPORT_IDEMPOTENCY_CONFLICT", "Mã yêu cầu đã dùng cho một file khác.");
        return (await Load(batch, ct)).Batch;
    }

    public async Task<IReadOnlyList<Response.Summary>> List(CancellationToken ct = default)
    {
        var actor = user.GetRequiredUserId();
        var batches = await db.PostImportBatches.AsNoTracking().Where(x => x.RequestedBy == actor).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(10).ToListAsync(ct);
        var result = new List<Response.Summary>();
        foreach (var batch in batches) result.Add((await Load(batch, ct)).Batch);
        return result;
    }
    public async Task<Response.Detail> Get(Guid id, int pageIndex = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var detail = await Load(await Owned(id, ct), ct);
        pageIndex = Math.Clamp(pageIndex, 1, 100000); pageSize = Math.Clamp(pageSize, 1, 100);
        return detail with { Rows = detail.Rows.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray(), PageIndex = pageIndex, PageSize = pageSize };
    }
    private async Task<PostImportBatch> Owned(Guid id, CancellationToken ct)
    {
        var actor = user.GetRequiredUserId();
        return await db.PostImportBatches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.RequestedBy == actor, ct)
            ?? throw new ImportException(404, "IMPORT_NOT_FOUND", "Không tìm thấy đợt nhập file.");
    }
    private async Task<Response.Detail> Load(PostImportBatch batch, CancellationToken ct)
    {
        var rows = await db.PostImportRows.AsNoTracking().Where(x => x.BatchId == batch.Id).OrderBy(x => x.RowNumber).ToListAsync(ct);
        var channels = await (from c in db.PostImportChannels.AsNoTracking()
            join r in db.PostImportRows.AsNoTracking() on c.RowId equals r.Id
            join p in db.PostPublications.AsNoTracking() on c.PublicationId equals p.Id into jobs
            from job in jobs.DefaultIfEmpty()
            where r.BatchId == batch.Id
            select new { c.RowId, c.Platform, c.Status, c.ErrorCode, c.PublicationId,
                JobStatus = job == null ? null : job.Status, JobError = job == null ? null : job.ErrorCode,
                ExternalPostId = job == null ? null : job.ExternalPostId, ExternalPostUrl = job == null ? null : job.ExternalPostUrl }).ToListAsync(ct);
        var grouped = channels.ToLookup(x => x.RowId);
        var result = rows.Select(r =>
        {
            var items = grouped[r.Id].Select(c => new Response.Channel(Name(c.Platform), c.JobStatus ?? (c.Status == "Rejected" ? "Failed" : "Pending"),
                c.JobError ?? c.ErrorCode, c.PublicationId, c.ExternalPostId, c.ExternalPostUrl, c.Status == "Existing")).ToArray();
            var state = r.Status == "Invalid" ? "Invalid" : r.Status == "Queued" ? "Queued"
                : items.Any(x => x.Status is "Pending" or "Queued" or "Publishing") ? "Publishing"
                : items.Any(x => x.Status is "NeedsReview" or "Closed") ? "NeedsReview"
                : items.Any(x => x.Status == "Scheduled") ? "Scheduled"
                : items.Any(x => x.Status is "Failed" or "Cancelled") ? "Failed" : "Succeeded";
            return new Response.Row(r.RowNumber, r.Code, Read<ImportInput>(r.Payload).Title, state, r.BasePostId, Read<ImportIssue[]>(r.Issues), items);
        }).ToArray();
        var all = result.SelectMany(x => x.Channels).ToArray();
        var invalid = rows.Count(x => x.Status == "Invalid");
        var pending = all.Count(x => x.Status is "Pending" or "Queued" or "Publishing");
        var failed = all.Count(x => x.Status is "Failed" or "Cancelled");
        var scheduled = all.Count(x => x.Status == "Scheduled");
        var review = all.Count(x => x.Status is "NeedsReview" or "Closed");
        var issues = Read<ImportIssue[]>(batch.Issues);
        var status = batch.Status != "Processed" || pending > 0 ? "Processing" : scheduled > 0 ? "Scheduled" : review > 0 ? "NeedsReview"
            : failed > 0 || invalid > 0 || issues.Length > 0 ? "CompletedWithErrors" : "Completed";
        var summary = new Response.Summary(batch.Id, batch.FileName, batch.CreatedAt, status, rows.Count, rows.Count(x => x.Status != "Queued"),
            invalid, all.Length, all.Count(x => x.Status == "Succeeded"), failed, pending, scheduled, review, all.Count(x => x.Existing));
        return new(summary, issues, result, 1, rows.Count, rows.Count);
    }
    public async Task<byte[]> Export(Guid id, CancellationToken ct = default)
    {
        var detail = await Load(await Owned(id, ct), ct);
        var records = new List<Dictionary<string, object?>>();
        Dictionary<string, object?> Record(string sheet, int row, string code, string platform, string status, string? error, string? message, Guid? postId = null, string? externalId = null, string? url = null) => new()
        {
            ["Sheet"] = sheet, ["Dòng"] = row, ["Mã bài"] = code, ["Kênh"] = platform, ["Trạng thái"] = status,
            ["Mã lỗi"] = error ?? "", ["Chi tiết"] = message ?? "", ["ID bài hệ thống"] = postId?.ToString() ?? "",
            ["ID bài nền tảng"] = externalId ?? "", ["Đường dẫn bài"] = url ?? ""
        };
        foreach (var row in detail.Rows)
        {
            foreach (var issue in row.Issues) records.Add(Record(issue.Sheet, issue.RowNumber, row.Code, "", "Invalid", issue.Code, issue.Message, row.BasePostId));
            foreach (var channel in row.Channels) records.Add(Record("Bai_dang", row.RowNumber, row.Code, channel.Platform, channel.Status,
                channel.ErrorCode, channel.Existing ? "Đã có tác vụ cho bài/kênh này; không tạo lại." : ErrorMessage(channel.ErrorCode), row.BasePostId, channel.ExternalPostId, channel.ExternalPostUrl));
        }
        foreach (var issue in detail.Issues) records.Add(Record(issue.Sheet, issue.RowNumber, "", "", "Invalid", issue.Code, issue.Message));
        using var stream = new MemoryStream();
        await MiniExcel.SaveAsAsync(stream, records, sheetName: "Ket_qua", excelType: ExcelType.XLSX);
        return stream.ToArray();
    }
    internal static string Name(PlatformType type) => type.ToString().ToLowerInvariant();
    public static string? ErrorMessage(string? code) => code switch
    {
        null => null,
        "IMPORT_ACCOUNT_REQUIRED" => "Chưa cấu hình tài khoản mặc định cho kênh này. Lưu tài khoản rồi nhập lại file.",
        "IMPORT_CONTENT_CHANGED" => "Mã bài đã có nội dung khác và có tác vụ đang chạy/đã đăng. Sửa bài trong hệ thống hoặc dùng mã mới cho bài mới.",
        "IMPORT_SOURCE_DELETED" => "Bài của mã này đã bị xóa; dùng mã mới để tạo bài mới.",
        "IMPORT_PROCESSING_FAILED" => "Chưa xử lý được dòng sau nhiều lần thử. Kiểm tra dữ liệu và nhập lại file.",
        "PUBLICATION_IMAGE_COUNT_INVALID" => "Số ảnh vượt giới hạn của kênh. Chọn bớt ảnh hoặc tách thành nhiều bài; hệ thống không tự bỏ ảnh.",
        "PUBLICATION_SCHEDULE_OUT_OF_RANGE" => "Lịch đăng đã quá gần, quá hạn hoặc vượt khoảng thời gian cho phép. Sửa ngày giờ và nhập lại.",
        "PUBLISHING_DISABLED" => "Kênh đăng chưa được bật trong môi trường này.",
        _ => "Kiểm tra mã lỗi và chi tiết bài đăng để xử lý."
    };
    internal static async Task Advisory(AppDbContext db, string key, CancellationToken ct)
    {
        if (db.Database.IsRelational())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))", ct);
    }
}
