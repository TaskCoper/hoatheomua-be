using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class GeneratedCardConfiguration : IEntityTypeConfiguration<GeneratedCard>
{
    public void Configure(EntityTypeBuilder<GeneratedCard> builder)
    {
        builder.ToTable("GeneratedCards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.ImageUrl).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
