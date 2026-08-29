using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(x => x.ReceiverName)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.Note)
            .HasMaxLength(500);
        builder.Property(x => x.Ward)
            .HasMaxLength(100);
        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.MapId)
            .HasMaxLength(300);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        // Địa chỉ luôn được truy vấn theo user → đánh index cho nhanh.
        builder.HasIndex(x => x.UserId);
    }
}
