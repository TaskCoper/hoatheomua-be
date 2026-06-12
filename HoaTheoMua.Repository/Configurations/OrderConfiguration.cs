using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.TrackNumber)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();
        
        builder.Property(o => o.Metadata)
            .IsRequired();
        
        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasMany(o => o.Transactions)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
