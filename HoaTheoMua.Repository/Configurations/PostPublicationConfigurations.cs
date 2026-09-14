using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PostPublicationConfiguration : IEntityTypeConfiguration<PostPublication>
{
    public void Configure(EntityTypeBuilder<PostPublication> b)
    {
        b.ToTable("PostPublication", t =>
        {
            t.HasCheckConstraint("CK_PostPublication_Status", "\"Status\" IN ('Queued','Scheduled','Publishing','Succeeded','Failed','Cancelled','NeedsReview','Closed')");
            t.HasCheckConstraint("CK_PostPublication_Counters", "\"Revision\" > 0 AND \"AttemptCount\" >= 0 AND \"StepCount\" >= 0");
            t.HasCheckConstraint("CK_PostPublication_Lease", "(\"LeaseId\" IS NULL) = (\"LeaseUntil\" IS NULL)");
            t.HasCheckConstraint("CK_PostPublication_Schedule", "\"Status\" <> 'Scheduled' OR (\"ScheduledAt\" IS NOT NULL AND \"NextRunAt\" IS NOT NULL AND \"NextRunAt\" >= \"ScheduledAt\" AND \"LeaseId\" IS NULL AND \"AttemptCount\" = 0 AND NOT \"PublishMayHaveOccurred\")");
            t.HasCheckConstraint("CK_PostPublication_Retry", "NOT \"CanRetry\" OR (\"Status\" = 'Failed' AND NOT \"PublishMayHaveOccurred\")");
            t.HasCheckConstraint("CK_PostPublication_Success", "\"Status\" <> 'Succeeded' OR (\"ExternalPostId\" IS NOT NULL AND length(\"ExternalPostId\") > 0)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.IdempotencyKey).HasMaxLength(128);
        b.Property(x => x.RequestHash).HasMaxLength(64);
        b.Property(x => x.Snapshot).HasColumnType("jsonb");
        b.Property(x => x.Checkpoint).HasColumnType("jsonb");
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.ExternalPostId).HasMaxLength(200);
        b.Property(x => x.ExternalPostUrl).HasMaxLength(2048);
        b.Property(x => x.ErrorCode).HasMaxLength(100);
        b.HasOne<BasePost>().WithMany().HasForeignKey(x => x.BasePostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PlatformPost>().WithMany().HasForeignKey(x => x.PlatformPostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PlatformAccount>().WithMany().HasForeignKey(x => x.PlatformAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PlatformPostId, x.IdempotencyKey }).IsUnique();
        b.HasIndex(x => x.PlatformPostId).IsUnique().HasDatabaseName("UX_PostPublication_Active")
            .HasFilter("\"Status\" IN ('Queued','Scheduled','Publishing','NeedsReview')");
        b.HasIndex(x => new { x.PlatformPostId, x.CreatedAt, x.Id });
        b.HasIndex(x => new { x.Status, x.NextRunAt, x.LeaseUntil });
    }
}

public class PostPublicationAttemptConfiguration : IEntityTypeConfiguration<PostPublicationAttempt>
{
    public void Configure(EntityTypeBuilder<PostPublicationAttempt> b)
    {
        b.ToTable("PostPublicationAttempt", t => t.HasCheckConstraint("CK_PostPublicationAttempt_Number", "\"Number\" > 0"));
        b.HasKey(x => x.Id);
        b.HasOne<PostPublication>().WithMany().HasForeignKey(x => x.PublicationId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PublicationId, x.Number }).IsUnique();
        b.Property(x => x.Operation).HasMaxLength(40); b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.ErrorCode).HasMaxLength(100); b.Property(x => x.ProviderCode).HasMaxLength(100);
        b.Property(x => x.ProviderRequestId).HasMaxLength(200); b.Property(x => x.ExternalPostId).HasMaxLength(200);
    }
}

public class PostPublicationReviewConfiguration : IEntityTypeConfiguration<PostPublicationReview>
{
    public void Configure(EntityTypeBuilder<PostPublicationReview> b)
    {
        b.ToTable("PostPublicationReview"); b.HasKey(x => x.Id);
        b.HasOne<PostPublication>().WithMany().HasForeignKey(x => x.PublicationId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Action).HasMaxLength(40); b.Property(x => x.Note).HasMaxLength(2000);
        b.Property(x => x.ExternalPostId).HasMaxLength(200);
        b.HasIndex(x => new { x.PublicationId, x.CreatedAt });
    }
}
