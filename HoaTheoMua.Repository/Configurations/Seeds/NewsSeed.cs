using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class NewsSeed
{
    public static void SeedNews(this EntityTypeBuilder<News> builder)
    {
        // Author* are the profile snapshot; in real writes they come from the JWT claims.
        // Seeded UserIds map to the legacy dev accounts: 1111=Admin, 2222=Staff, 3333=Normal.
        builder.HasData(
         
        );
    }
}
