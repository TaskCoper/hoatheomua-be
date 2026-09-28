using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CardJobs;

public sealed class Store(AppDbContext db, TimeProvider clock)
{
    // Every mutation runs in a short transaction; never hold a lock during provider I/O.
    public async Task<CardGenerationJob?> Lock(Guid id, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        return db.Database.IsRelational()
            ? await db.CardGenerationJobs.FromSqlInterpolated($"SELECT * FROM \"CardGenerationJob\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct)
            : await db.CardGenerationJobs.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public void Touch(CardGenerationJob job)
    {
        job.Revision++;
        job.UpdatedAt = clock.GetUtcNow();
    }

    public void Finish(CardGenerationJob job, string status, string? error = null)
    {
        job.Status = status; job.ErrorCode = error; job.CompletedAt = clock.GetUtcNow();
        job.LeaseId = null; job.LeaseUntil = null;
        Touch(job);
    }
}
