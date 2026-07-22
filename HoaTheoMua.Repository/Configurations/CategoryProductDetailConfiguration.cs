using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CategoryProductDetailConfiguration : IEntityTypeConfiguration<CategoryProductDetail>
{
    public void Configure(EntityTypeBuilder<CategoryProductDetail> builder)
    {
        builder.HasIndex(cnd => new { cnd.CategoryProductId, cnd.ProductId }).IsUnique();

        builder.Property(cnd => cnd.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(cnd => cnd.CategoryProduct)
            .WithMany(c => c.CategoryProductDetails)
            .HasForeignKey(cnd => cnd.CategoryProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cnd => cnd.Product)
            .WithMany(p => p.CategoryProductDetails)
            .HasForeignKey(cnd => cnd.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
