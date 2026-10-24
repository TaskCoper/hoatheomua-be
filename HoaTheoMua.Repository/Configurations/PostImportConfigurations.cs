using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PostImportBatchConfiguration : IEntityTypeConfiguration<PostImportBatch>
{
    public void Configure(EntityTypeBuilder<PostImportBatch> b)
    {
        b.ToTable("PostImportBatch", t =>
        {
            t.HasCheckConstraint("CK_PostImportBatch_Status", "\"Status\" IN ('Queued','Processing','Processed')");
            t.HasCheckConstraint("CK_PostImportBatch_Lease", "(\"LeaseId\" IS NULL) = (\"LeaseUntil\" IS NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.RequestKey).HasMaxLength(128).IsRequired();
        b.Property(x => x.FileHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Issues).HasColumnType("jsonb").IsRequired();
        b.HasIndex(x => new { x.RequestedBy, x.RequestKey }).IsUnique();
        b.HasIndex(x => new { x.RequestedBy, x.CreatedAt }).IsDescending(false, true);
        b.HasIndex(x => new { x.Status, x.LeaseUntil }).HasFilter("\"Status\" <> 'Processed'");
    }
}

public class PostImportRowConfiguration : IEntityTypeConfiguration<PostImportRow>
{
    public void Configure(EntityTypeBuilder<PostImportRow> b)
    {
        b.ToTable("PostImportRow", t =>
        {
            t.HasCheckConstraint("CK_PostImportRow_Status", "\"Status\" IN ('Queued','Processed','Invalid')");
            t.HasCheckConstraint("CK_PostImportRow_Range", "\"RowNumber\" >= 2 AND \"Attempts\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Preparation).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Issues).HasColumnType("jsonb").IsRequired();
        b.HasOne<PostImportBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<BasePost>().WithMany().HasForeignKey(x => x.BasePostId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.BatchId, x.RowNumber }).IsUnique();
        b.HasIndex(x => new { x.BatchId, x.Status, x.RowNumber });
        b.HasIndex(x => x.BasePostId);
    }
}

public class PostImportChannelConfiguration : IEntityTypeConfiguration<PostImportChannel>
{
    public void Configure(EntityTypeBuilder<PostImportChannel> b)
    {
        b.ToTable("PostImportChannel", t => t.HasCheckConstraint("CK_PostImportChannel_Status", "\"Status\" IN ('Pending','Submitted','Existing','Rejected','Waiting')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Platform).HasColumnType("\"PlatformType\"");
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.ErrorCode).HasMaxLength(100);
        b.HasOne<PostImportRow>().WithMany().HasForeignKey(x => x.RowId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<PlatformAccount>().WithMany().HasForeignKey(x => x.PlatformAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PostPublication>().WithMany().HasForeignKey(x => x.PublicationId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.RowId, x.Platform }).IsUnique();
        b.HasIndex(x => x.PublicationId);
        b.HasIndex(x => x.PlatformAccountId);
    }
}

public class ImportedPostIdentityConfiguration : IEntityTypeConfiguration<ImportedPostIdentity>
{
    public void Configure(EntityTypeBuilder<ImportedPostIdentity> b)
    {
        b.ToTable("ImportedPostIdentity");
        b.HasKey(x => new { x.RequestedBy, x.Code });
        b.Property(x => x.Code).HasMaxLength(100);
        b.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
        b.HasOne<BasePost>().WithMany().HasForeignKey(x => x.BasePostId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.BasePostId).IsUnique();
    }
}

public class PostImportAccountConfiguration : IEntityTypeConfiguration<PostImportAccount>
{
    public void Configure(EntityTypeBuilder<PostImportAccount> b)
    {
        b.ToTable("PostImportAccount");
        b.HasKey(x => new { x.RequestedBy, x.Platform });
        b.Property(x => x.Platform).HasColumnType("\"PlatformType\"");
        b.HasOne<PlatformAccount>().WithMany().HasForeignKey(x => x.PlatformAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PlatformAccountId);
    }
}
