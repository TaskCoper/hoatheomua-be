using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CompanyAddressConfiguration: IEntityTypeConfiguration<CompanyAddress>
{
    public void Configure(EntityTypeBuilder<CompanyAddress> builder)
    {
        builder.Property(ca => ca.AddressLine)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(ca => ca.Ward)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(ca => ca.MapId)
            .HasMaxLength(100);
        builder.Property(ca => ca.City)
            .HasMaxLength(100);
        builder.Property(ca => ca.Country)
            .HasMaxLength(100);
        builder.Property(ca => ca.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.SeedCompanyAddress();
    }
    
}