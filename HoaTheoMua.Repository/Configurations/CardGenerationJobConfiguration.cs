using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CardGenerationJobConfiguration : IEntityTypeConfiguration<CardGenerationJob>
{
    public void Configure(EntityTypeBuilder<CardGenerationJob> b)
    {
        b.ToTable("CardGenerationJob", t =>
        {
            t.HasCheckConstraint("CK_CardJob_Status", "\"Status\" IN ('Queued','Running','Succeeded','Failed','Canceled')");
            t.HasCheckConstraint("CK_CardJob_Type", "\"Type\" IN ('Create','Regenerate')");
            t.HasCheckConstraint("CK_CardJob_Lease", "(\"LeaseId\" IS NULL) = (\"LeaseUntil\" IS NULL)");
            t.HasCheckConstraint("CK_CardJob_Success", "\"Status\" <> 'Succeeded' OR (\"GeneratedCardId\" IS NOT NULL AND \"Result\" IS NOT NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.IdempotencyKey).HasMaxLength(128);
        b.Property(x => x.RequestHash).HasMaxLength(64);
        b.Property(x => x.Type).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.Stage).HasMaxLength(40);
        b.Property(x => x.DisplayName).HasMaxLength(250);
        b.Property(x => x.ErrorCode).HasMaxLength(100);
        b.Property(x => x.Snapshot).HasColumnType("jsonb");
        b.Property(x => x.Checkpoint).HasColumnType("jsonb");
        b.Property(x => x.Result).HasColumnType("jsonb");
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasIndex(x => new { x.UserId, x.IdempotencyKey }).IsUnique();
        b.HasIndex(x => new { x.OrderDetailId, x.VirtualId }).IsUnique()
            .HasFilter("\"Type\" = 'Create' AND \"Status\" IN ('Queued','Running')");
        b.HasIndex(x => new { x.UserId, x.CreatedAt, x.Id });
        b.HasIndex(x => new { x.Status, x.LeaseUntil, x.LastDispatchedAt });
        b.HasOne<GeneratedCard>().WithMany().HasForeignKey(x => x.GeneratedCardId).OnDelete(DeleteBehavior.Restrict);
    }
}
