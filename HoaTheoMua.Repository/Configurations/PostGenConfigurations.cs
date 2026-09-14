using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

internal static class PostGenConstraints
{
    public static string EnumCheck<T>(string column) where T : struct, System.Enum =>
        $"\"{column}\" IN ({string.Join(", ", System.Enum.GetNames<T>().Select(x => $"'{x}'"))})";
}

public class BasePostConfiguration : IEntityTypeConfiguration<BasePost>
{
    public void Configure(EntityTypeBuilder<BasePost> b)
    {
        b.ToTable("BasePost");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(500).IsRequired();
        b.Property(x => x.Content).HasColumnType("text").IsRequired();
        b.Property(x => x.Hashtag).HasMaxLength(1000);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.CreatedAt).IsDescending().HasFilter("\"IsDeleted\" = false");
    }
}

public class BasePostImageConfiguration : IEntityTypeConfiguration<BasePostImage>
{
    public void Configure(EntityTypeBuilder<BasePostImage> b)
    {
        b.ToTable("BasePostImage");
        b.HasKey(x => x.Id);
        b.Property(x => x.ImageUrl).HasMaxLength(2048).IsRequired();
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasOne<BasePost>().WithMany().HasForeignKey(x => x.BasePostId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.BasePostId, x.SortOrder }).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}

public class PlatformPostConfiguration : IEntityTypeConfiguration<PlatformPost>
{
    public void Configure(EntityTypeBuilder<PlatformPost> b)
    {
        b.ToTable("PlatformPost", t => t.HasCheckConstraint("CK_PlatformPost_Status", PostGenConstraints.EnumCheck<PlatformPostStatus>("Status")));
        b.HasKey(x => x.Id);
        b.Property(x => x.Platform).HasColumnType("\"PlatformType\"");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.LastError).HasColumnType("text");
        b.Property(x => x.PublishError).HasColumnType("text");
        b.Property(x => x.ExternalPostId).HasMaxLength(200);
        b.Property(x => x.PublishAttemptCount).HasDefaultValue(0);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasOne<BasePost>().WithMany().HasForeignKey(x => x.BasePostId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<GeneratedPost>().WithMany().HasForeignKey(x => x.OfficialGenerationId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.BasePostId, x.Platform }).IsUnique().HasFilter("\"IsDeleted\" = false");
        b.HasIndex(x => new { x.Status, x.ScheduledAt }).HasFilter("\"Status\" = 'Scheduled'");
    }
}

public class GeneratedPostConfiguration : IEntityTypeConfiguration<GeneratedPost>
{
    public void Configure(EntityTypeBuilder<GeneratedPost> b)
    {
        b.ToTable("GeneratedPost", t => t.HasCheckConstraint("CK_GeneratedPost_Source", PostGenConstraints.EnumCheck<ContentSource>("Source")));
        b.HasKey(x => x.Id);
        b.Property(x => x.Source).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Content).HasColumnType("text").IsRequired();
        b.Property(x => x.Hashtag).HasMaxLength(1000);
        b.Property(x => x.AiModel).HasMaxLength(100);
        b.Property(x => x.AiRequestId).HasMaxLength(200);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasOne<PlatformPost>().WithMany().HasForeignKey(x => x.PlatformPostId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<GeneratedPost>().WithMany().HasForeignKey(x => x.ParentGeneratedPostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SystemPrompt>().WithMany().HasForeignKey(x => x.SystemPromptId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Config>().WithMany().HasForeignKey(x => x.SystemPromptConfigId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PlatformPostId, x.CreatedAt, x.Id });
        b.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true);
    }
}

public class GeneratedPostImageConfiguration : IEntityTypeConfiguration<GeneratedPostImage>
{
    public void Configure(EntityTypeBuilder<GeneratedPostImage> b)
    {
        b.ToTable("GeneratedPostImage", t =>
        {
            t.HasCheckConstraint("CK_GeneratedPostImage_Status", PostGenConstraints.EnumCheck<ImageStatus>("Status"));
            t.HasCheckConstraint("CK_GeneratedPostImage_RatioCode", PostGenConstraints.EnumCheck<AspectRatioType>("RatioCode"));
            t.HasCheckConstraint("CK_GeneratedPostImage_Succeeded", "\"Status\" <> 'Succeeded' OR \"ImageUrl\" IS NOT NULL");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RatioCode).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.SourceImageUrl).HasMaxLength(2048).IsRequired();
        b.Property(x => x.ImageUrl).HasMaxLength(2048);
        b.Property(x => x.StorageProvider).HasMaxLength(50);
        b.Property(x => x.AiModel).HasMaxLength(100);
        b.Property(x => x.AiRequestId).HasMaxLength(200);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasOne<BasePost>().WithMany().HasForeignKey(x => x.BasePostId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<BasePostImage>().WithMany().HasForeignKey(x => x.SourceBasePostImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<GeneratedPostImage>().WithMany().HasForeignKey(x => x.ParentGeneratedPostImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.SourceBasePostImageId, x.RatioCode, x.CreatedAt, x.Id });
        b.HasIndex(x => new { x.BasePostId, x.CreatedAt }).IsDescending(false, true);
    }
}

public class PlatformPostImageConfiguration : IEntityTypeConfiguration<PlatformPostImage>
{
    public void Configure(EntityTypeBuilder<PlatformPostImage> b)
    {
        b.ToTable("PlatformPostImage", t => t.HasCheckConstraint("CK_PlatformPostImage_RatioCode", PostGenConstraints.EnumCheck<AspectRatioType>("RatioCode")));
        b.HasKey(x => x.Id);
        b.Property(x => x.RatioCode).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasOne<PlatformPost>().WithMany().HasForeignKey(x => x.PlatformPostId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<BasePostImage>().WithMany().HasForeignKey(x => x.SourceBasePostImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<GeneratedPostImage>().WithMany().HasForeignKey(x => x.SelectedGeneratedPostImageId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PlatformPostId, x.SourceBasePostImageId, x.RatioCode }).IsUnique();
        b.HasIndex(x => new { x.PlatformPostId, x.SortOrder });
    }
}
