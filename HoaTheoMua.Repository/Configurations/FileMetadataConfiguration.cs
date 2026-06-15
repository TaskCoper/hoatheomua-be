using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.StorageKey)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.BucketName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.UploadStatus)
            .HasDefaultValue(FileUploadStatus.Pending);

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(f => f.StorageKey)
            .IsUnique();

        builder.HasIndex(f => f.UploadStatus);
        builder.HasIndex(f => f.UploadedByUserId);
        builder.HasIndex(f => f.CreatedAt);

        builder.HasOne(f => f.UploadedByUser)
            .WithMany()
            .HasForeignKey(f => f.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
