using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CategoryNewsDetailSeed
{
    public static void SeedCategoryNewsDetail(this EntityTypeBuilder<CategoryNewsDetail> builder)
    {
        builder.HasData(
           
        );
    }
}
