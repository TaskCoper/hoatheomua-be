using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CompanyAddressConfiguration: IEntityTypeConfiguration<CompanyAddress>
{
    public void Configure(EntityTypeBuilder<CompanyAddress> builder)
    {
        builder.Property(ca => ca.AddressLine)
            .HasMaxLength(500);
        builder.Property(ca => ca.Ward)
            .HasMaxLength(100);
        builder.Property(ca => ca.District)
            .HasMaxLength(100);
        builder.Property(ca => ca.Province)
            .HasMaxLength(100);
        builder.Property(ca => ca.City)
            .HasMaxLength(100);
        builder.Property(ca => ca.Country)
            .HasMaxLength(100);
        builder.Property(ca => ca.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}