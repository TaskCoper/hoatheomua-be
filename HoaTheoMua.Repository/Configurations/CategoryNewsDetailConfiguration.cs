using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CategoryNewsDetailConfiguration: IEntityTypeConfiguration<CategoryNewsDetail>
{

    public void Configure(EntityTypeBuilder<CategoryNewsDetail> builder)
    {
        builder.HasIndex(cnd => new { cnd.CategoryNewsId, cnd.NewsId }).IsUnique();

        builder.HasOne(cnd => cnd.CategoryNews)
            .WithMany(c => c.CategoryNewsDetail)
            .HasForeignKey(cnd => cnd.CategoryNewsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cnd => cnd.News)
            .WithMany(n => n.CategoryNewsDetail)
            .HasForeignKey(cnd => cnd.NewsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}