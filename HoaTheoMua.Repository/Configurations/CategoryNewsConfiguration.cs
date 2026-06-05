using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CategoryNewsConfiguration: IEntityTypeConfiguration<CategoryNews>
{
    public void Configure(EntityTypeBuilder<CategoryNews> builder)
    {
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.HasOne(c => c.ParentCategoryNews)
            .WithMany(c => c.ChildCategoryNews)
            .HasForeignKey(c => c.ParentCategoryNewsId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}