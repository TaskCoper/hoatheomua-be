using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CategoryNewsSeed
{
    public static void SeedCategoryNews( this EntityTypeBuilder<CategoryNews> builder)
    {
        
        builder.HasData(
            
        );
    }
}
