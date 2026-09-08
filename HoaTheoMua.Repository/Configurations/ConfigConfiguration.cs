using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class ConfigConfiguration: IEntityTypeConfiguration<Config>
{
    public void Configure(EntityTypeBuilder<Config> builder)
    {
        builder.Property(x => x.Value)
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(x => x.Key)
            .IsRequired();
        builder.Property(x => x.IsPublic)
            .HasDefaultValue(false);
        builder.HasIndex(x => x.Group);
        builder.HasIndex(x => new { x.Group, x.Key })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"Kind\" = 'Setting' AND \"Group\" = 'card_config' AND \"Key\" IN ('card_config.sizes', 'card_config.prices')");
        builder.Property(x => x.Group)
            .IsRequired();

    }
}
