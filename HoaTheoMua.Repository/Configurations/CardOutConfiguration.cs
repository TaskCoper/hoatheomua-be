using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CardOutConfiguration : IEntityTypeConfiguration<CardOut>
{
    public void Configure(EntityTypeBuilder<CardOut> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .IsRequired();

        builder.HasOne(x => x.History)
            .WithOne(x => x.CardOut)
            .HasForeignKey<History>(x => x.CardOutId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.HasOne(x => x.Order)
        //     .WithMany()
        //     .HasForeignKey(x => x.OrderId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
