using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PlatformStandardConfiguration : IEntityTypeConfiguration<PlatformStandard>
{
    public void Configure(EntityTypeBuilder<PlatformStandard> builder)
    {
        builder.ToTable("PlatformStandards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Platform).IsRequired();
    }
}
