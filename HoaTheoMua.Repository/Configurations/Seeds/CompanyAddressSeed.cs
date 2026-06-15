using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CompanyAddressSeed
{
    public static void SeedCompanyAddress(this EntityTypeBuilder<CompanyAddress> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        builder.HasData(
            new CompanyAddress
            {
                Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                AddressLine = "123 Main St",
                Ward = "Ward 1",
                MapId = "111-1-1-1-1-1",
                City = "Ho Chi Minh City",
                Country = "Vietnam",
                Latitude = 10.7769m,
                Longitude = 106.7009m,
                CreatedAt = baseDate,
                UpdatedAt = null,
                IsDeleted = false
            }
        );
    }
}
