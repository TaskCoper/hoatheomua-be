using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetails>
{
    public void Configure(EntityTypeBuilder<OrderDetails> builder)
    {
        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");
        builder.Property(x => x.SnapshotCard)
            .HasColumnType("jsonb");
        builder.Property(x => x.GeneratedFlowerId)
            .HasColumnName("GeneratedFlower")
            .HasColumnType("uuid[]");
        builder.Property(x => x.GeneratedCardId)
            .HasColumnName("GeneratedCard")
            .HasColumnType("uuid[]");
    }
}
