using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DiscountType).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.DiscountValue).HasPrecision(18, 2);
        builder.Property(x => x.MaxDiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.MinOrderAmount).HasPrecision(18, 2);
        builder.Property(x => x.MaxOrderAmount).HasPrecision(18, 2);
        builder.Property(x => x.UsedCount).HasDefaultValue(0);
        
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        // Unique across all rows, soft-deleted included: an issued code is never reused.
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ProgramId).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(x => new { x.Visibility, x.IsActive, x.IsDeleted });
    }
}