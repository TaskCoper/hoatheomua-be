using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Configurations.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace HoaTheoMua.Repository.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.TrackNumber)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();
        
        builder.Property(o => o.Address)
            .IsRequired();
        
        builder.Property(o => o.ShortAddress)
            .IsRequired();
        
        builder.Property(o => o.FullName)
            .IsRequired();
        
        builder.Property(o => o.Phone)
            .IsRequired();
        
        builder.Property(o => o.Remarks)
            .IsRequired();

        builder.Property(o => o.TrackNumber)
            .IsRequired();
        
        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasMany(o => o.Transactions)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.OrderDetails)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.SeedOrder();
    }
}
