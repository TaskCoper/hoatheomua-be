using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class RefreshTokenConfiguration: IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(500);
        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500);
        builder.Property(rt => rt.DeviceLabel)
            .HasMaxLength(255);
        builder.Property(rt => rt.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(rt => rt.IsActive)
            .HasDefaultValue(true)
            .IsRequired();
        builder.HasIndex(rt => rt.RefreshTokenHash)
            .IsUnique();
        builder.SeedRefreshToken();
    }
}