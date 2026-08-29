using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CompanyAddressSeed
{
    public static void SeedCompanyAddress(this EntityTypeBuilder<CompanyAddress> builder)
    {
        builder.HasData(

        );
    }
}
