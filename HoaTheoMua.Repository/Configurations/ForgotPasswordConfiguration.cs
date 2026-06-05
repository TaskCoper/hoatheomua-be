using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class ForgotPasswordConfiguration: IEntityTypeConfiguration<ForgotPassword>
{
    public void Configure(EntityTypeBuilder<ForgotPassword> builder)
    {
        builder.Property(rp => rp.Otp)
            .IsRequired();
        builder.Property(rp => rp.IsUsed)
            .HasDefaultValue(false);
        builder.Property(rp => rp.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(rp => rp.Otp)
            .IsUnique();
    }
}