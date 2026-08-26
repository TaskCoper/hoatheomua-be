using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class GeneratedPostConfiguration : IEntityTypeConfiguration<GeneratedPost>
{
    public void Configure(EntityTypeBuilder<GeneratedPost> builder)
    {
        builder.ToTable("GeneratedPosts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.ImageUrl).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
