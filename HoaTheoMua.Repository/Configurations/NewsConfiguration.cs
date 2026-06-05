using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class NewsConfiguration: IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(n => n.Description)
            .IsRequired()
            .HasColumnType("text");
        builder.Property(n => n.CoverImage)
            .HasMaxLength(500);
        builder.Property(n => n.ViewCount)
            .HasDefaultValue(0);
        builder.Property(n => n.Status)
            .HasMaxLength(50)
            .HasDefaultValue(NewsStatus.Draft);
        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.SeedNews();
    }
}