using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PostOutConfiguration : IEntityTypeConfiguration<PostOut>
{
    public void Configure(EntityTypeBuilder<PostOut> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .IsRequired();

        builder.HasOne(x => x.History)
            .WithOne(x => x.PostOut)
            .HasForeignKey<History>(x => x.PostOutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
