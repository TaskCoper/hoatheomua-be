using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class CategoryProductConfiguration : IEntityTypeConfiguration<CategoryProduct>
{
    public void Configure(EntityTypeBuilder<CategoryProduct> builder)
    {
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Name_En)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.Description_En)
            .HasMaxLength(1000);

        builder.Property(c => c.Position)
            .HasColumnType("double precision");

        builder.Property(c => c.MetaData)
            .HasColumnType("jsonb");

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(c => c.ParentCategoryProduct)
            .WithMany(c => c.ChildCategoryProducts)
            .HasForeignKey(c => c.ParentCategoryProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
