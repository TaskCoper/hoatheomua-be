using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class FlowerOutConfiguration : IEntityTypeConfiguration<FlowerOut>
{
    public void Configure(EntityTypeBuilder<FlowerOut> builder)
    {

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ImageUrl)
            .IsRequired();
        
        builder.HasOne(x => x.History)
            .WithOne(x => x.FlowerOut)
            .HasForeignKey<History>(x => x.FlowerOutId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.HasOne(x => x.Order)
        //     .WithMany()
        //     .HasForeignKey(x => x.OrderId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
