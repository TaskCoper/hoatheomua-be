using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostPublishing;

public sealed partial class Service
{
    public Task<Response.Job> Schedule(Guid id, string platform, Request.Schedule request, CancellationToken ct = default)
        => CreatePublication(id, platform, request, ParseScheduleTime(request.ScheduledAt), ct);

    private static DateTimeOffset ParseScheduleTime(string value)
    {
        if (value == null || !Regex.IsMatch(value, @"\A\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|[+-]\d{2}:\d{2})\z")
            || !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            throw Error(400, "PUBLICATION_SCHEDULE_OFFSET_REQUIRED", "scheduledAt phải là ISO 8601 có Z hoặc offset, ví dụ 2026-09-20T09:00:00+07:00.");
        // PostgreSQL timestamps have microsecond precision. Normalize before hashing/persisting.
        var utc = time.ToUniversalTime();
        return new DateTimeOffset(utc.Ticks - utc.Ticks % 10, TimeSpan.Zero);
    }
    private void ValidateScheduleTime(DateTimeOffset scheduledAt)
    {
        settings.Value.Validate(); var now = clock.GetUtcNow();
        if (scheduledAt < now.AddSeconds(settings.Value.MinScheduleLeadSeconds) || scheduledAt > now.AddDays(settings.Value.MaxScheduleDays))
            throw Error(400, "PUBLICATION_SCHEDULE_OUT_OF_RANGE", $"Lịch phải cách hiện tại ít nhất {settings.Value.MinScheduleLeadSeconds} giây và không quá {settings.Value.MaxScheduleDays} ngày.");
    }
    public async Task<Response.Job> Reschedule(Guid id, string platform, Request.Reschedule request, CancellationToken ct = default)
    {
        Validate(request); var actor = user.GetRequiredUserId(); var time = ParseScheduleTime(request.ScheduledAt);
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var (job, target, _) = await store.LockJob(request.PublicationId, ct);
        CheckScheduleTarget(job, id, platform); Expected(job, request.ExpectedRevision); RequireUnclaimedSchedule(job);
        ValidateScheduleTime(time);
        var previous = job.ScheduledAt;
        job.ScheduledAt = job.NextRunAt = time; job.LastDispatchedAt = null;
        store.State(job, target, "Scheduled");
        AddReview(job.Id, actor, "Reschedule", Json(new { previous, scheduledAt = time }));
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return ToResponse(job);
    }
    public async Task<Response.Job> CancelSchedule(Guid id, string platform, Request.CancelSchedule request, CancellationToken ct = default)
    {
        Validate(request); var actor = user.GetRequiredUserId();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var (job, target, _) = await store.LockJob(request.PublicationId, ct);
        CheckScheduleTarget(job, id, platform); Expected(job, request.ExpectedRevision); RequireUnclaimedSchedule(job);
        store.State(job, target, "Cancelled"); job.NextRunAt = job.LastDispatchedAt = null;
        AddReview(job.Id, actor, "CancelSchedule", Json(new { scheduledAt = job.ScheduledAt }));
        await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        return ToResponse(job);
    }
    private static void CheckScheduleTarget(Repository.Entity.PostPublication job, Guid id, string platform)
    {
        if (job.BasePostId != id || Read<PublicationSnapshot>(job.Snapshot).Platform != PlatformAccount.Service.Parse(platform))
            throw Error(404, "PUBLICATION_SCHEDULE_NOT_FOUND", "Không tìm thấy lịch cho bài/nền tảng này.");
    }
    private static void RequireUnclaimedSchedule(Repository.Entity.PostPublication job)
    {
        if (job.Status != "Scheduled" || job.LeaseId != null || job.AttemptCount != 0 || job.PublishMayHaveOccurred)
            throw Error(409, "PUBLICATION_SCHEDULE_NOT_EDITABLE", "Lịch đã được worker nhận hoặc đã kết thúc; không thể đổi/hủy lịch.");
    }
}
