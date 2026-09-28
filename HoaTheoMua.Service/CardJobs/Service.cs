using System.Security.Cryptography;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Card;
using HoaTheoMua.Service.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static HoaTheoMua.Service.CardJobs.JobJson;

namespace HoaTheoMua.Service.CardJobs;

public sealed class Service(AppDbContext db, Card.Service cards, ICurrentUserService currentUser,
    IPublishEndpoint bus, Store store, TimeProvider clock, IOptions<CardJobOptions> settings)
{
    public async Task<JobResponse> Create(Request.CreateCardAiRequest request, string key, CancellationToken ct)
    {
        var type = request.SourceCardId.HasValue ? "Regenerate" : "Create";
        var source = request.SourceCardId;
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_')))
            throw new BadRequestException("Idempotency-Key phải có 1–128 ký tự chữ, số, dấu - hoặc _.");
        var user = currentUser.GetRequiredUserId();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Write(new { type, source, request }))));
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        // Serializes submissions for a user, including quota checks and first-use key races.
        if (db.Database.IsRelational())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({user.ToString()}, 0))", ct);
        var existing = await db.CardGenerationJobs.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == user && x.IdempotencyKey == key, ct);
        if (existing != null)
        {
            if (existing.RequestHash != hash) throw new ConflictException("CARD_JOB_KEY_REUSED", "Key đã được dùng cho yêu cầu khác.");
            return Response(existing);
        }
        if (await db.CardGenerationJobs.CountAsync(x => x.UserId == user && (x.Status == "Queued" || x.Status == "Running"), ct) >= settings.Value.MaxActivePerUser)
            throw new ConflictException("CARD_JOB_LIMIT", "Bạn đã có nhiều thiết kế đang xử lý. Vui lòng chờ hoàn thành.");
        var input = await cards.Prepare(request, ct);
        // Validate the complete immutable provider input before acknowledging the job.
        AiService.Service.ValidateCardInput(input.Input);
        if (type == "Create" && await db.CardGenerationJobs.AnyAsync(x => x.OrderDetailId == input.OrderDetailId
            && x.VirtualId == input.VirtualId && x.Type == "Create" && (x.Status == "Queued" || x.Status == "Running"), ct))
            throw new ConflictException("CARD_GENERATION_IN_PROGRESS", "Thiệp đang được tạo.");
        var now = clock.GetUtcNow();
        var job = new CardGenerationJob
        {
            Id = Guid.NewGuid(), UserId = user, Type = type, IdempotencyKey = key, RequestHash = hash,
            DisplayName = input.Snapshot.CardTemplate.TemplateName[..Math.Min(250, input.Snapshot.CardTemplate.TemplateName.Length)],
            OrderDetailId = input.OrderDetailId, VirtualId = input.VirtualId,
            BouquetImageUrl = input.Snapshot.Source.ImageUrl, TemplateImageUrl = input.Snapshot.CardTemplate.TemplateImageUrl,
            SourceCardId = source, Snapshot = Write(input),
            Checkpoint = Write(new Checkpoint()), CreatedAt = now, UpdatedAt = now, LastDispatchedAt = now
        };
        db.CardGenerationJobs.Add(job);
        await bus.Publish(new CardGenerationRequested(job.Id), ct);
        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
        return Response(job);
    }

    public async Task<JobResponse> Get(Guid id, CancellationToken ct)
    {
        var user = currentUser.GetRequiredUserId();
        var job = await db.CardGenerationJobs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.UserId == user, ct)
            ?? throw new NotFoundException("CARD_JOB_NOT_FOUND");
        return Response(job);
    }

    public async Task<BasePaginationResponse> List(bool active, int pageIndex, int pageSize, CancellationToken ct, Guid? orderDetailId = null, Guid? virtualId = null)
    {
        var user = currentUser.GetRequiredUserId();
        pageIndex = Math.Max(1, pageIndex); pageSize = Math.Clamp(pageSize, 1, 50);
        var query = db.CardGenerationJobs.AsNoTracking().Where(x => x.UserId == user);
        if (orderDetailId.HasValue) query = query.Where(x => x.OrderDetailId == orderDetailId);
        if (virtualId.HasValue) query = query.Where(x => x.VirtualId == virtualId);
        if (active) query = query.Where(x => x.Status == "Queued" || x.Status == "Running");
        var total = await query.CountAsync(ct);
        var jobs = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return ApiResponseFactory.BasePagination(jobs.Select(Response).ToList(), pageIndex, pageSize, total);
    }

    public async Task<JobResponse> Cancel(Guid id, CancellationToken ct)
    {
        var user = currentUser.GetRequiredUserId();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var job = await store.Lock(id, ct);
        if (job == null || job.UserId != user) throw new NotFoundException("CARD_JOB_NOT_FOUND");
        if (Active(job))
        {
            job.CancelRequestedAt ??= clock.GetUtcNow();
            if (job.Status == "Queued") store.Finish(job, "Canceled");
            else store.Touch(job);
            await db.SaveChangesAsync(ct);
        }
        if (tx != null) await tx.CommitAsync(ct);
        return Response(job);
    }
}
