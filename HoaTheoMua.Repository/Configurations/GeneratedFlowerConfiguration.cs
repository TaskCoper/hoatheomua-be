using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class GeneratedFlowerConfiguration : IEntityTypeConfiguration<GeneratedFlower>
{
    public void Configure(EntityTypeBuilder<GeneratedFlower> builder)
    {
        builder.ToTable("GeneratedFlowers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
