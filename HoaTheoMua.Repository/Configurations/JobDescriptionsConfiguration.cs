using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class JobDescriptionsConfiguration : IEntityTypeConfiguration<JobDescriptions>
{
    public void Configure(EntityTypeBuilder<JobDescriptions> builder)
    {
        builder.Property(x => x.title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.avatarUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.coverImageUrl)
            .IsRequired()
            .HasMaxLength(2048);
        
        builder.Property(x => x.description)
            .IsRequired();

        builder.Property(x => x.slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(x => x.slug)
            .IsUnique();

        builder.Property(x => x.minExperienceYears)
            .IsRequired(false);

        builder.Property(x => x.maxExperienceYears)
            .IsRequired(false);

        builder.Property(x => x.salaryMin)
            .IsRequired(false);

        builder.Property(x => x.salaryMax)
            .IsRequired(false);

        builder.Property(x => x.level)
            .IsRequired();

        builder.Property(x => x.employmentType)
            .IsRequired();

        builder.Property(x => x.status)
            .IsRequired();

        builder.Property(x => x.salaryNegotiable)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.openings)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.recruitmentRequests)
            .WithOne(a => a.JobDescription)
            .HasForeignKey(a => a.JobDescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}