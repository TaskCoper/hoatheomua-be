using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class TransactionSeed
{
    public static void SeedTransaction(this EntityTypeBuilder<Transaction> builder)
    {
        // Transaction seed removed: it referenced OrderIds (44444444-…441 .. 450) that are no
        // longer seeded (OrderSeed.HasData is empty), which violated FK_Transactions_Orders_OrderId.
        // Re-add Order seeds first if you want demo transactions back.
        builder.HasData(

        );
    }
}
