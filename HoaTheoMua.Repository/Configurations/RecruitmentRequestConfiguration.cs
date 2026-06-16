using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class RecruitmentRequestConfiguration : IEntityTypeConfiguration<RecruitmentRequest>
{
    public void Configure(EntityTypeBuilder<RecruitmentRequest> builder)
    {
        builder.ToTable("RecruitmentRequest");

        builder.Property(x => x.title).IsRequired();
        builder.Property(x => x.coverLetter).IsRequired();
        builder.Property(x => x.attachments).IsRequired();
        builder.Property(x => x.isRead).HasDefaultValue(false);
    }
}