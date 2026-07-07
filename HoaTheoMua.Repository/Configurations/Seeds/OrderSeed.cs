using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class OrderSeed
{
    public static void SeedOrder(this EntityTypeBuilder<Order> builder)
    {
        builder.HasData(

        );
    }
}
