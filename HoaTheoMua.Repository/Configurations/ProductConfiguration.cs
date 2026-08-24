using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HoaTheoMua.Repository.Configurations.Seeds;

namespace HoaTheoMua.Repository.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.NhanhProductId)
            .HasMaxLength(100);

        builder.HasIndex(p => p.NhanhProductId)
            .IsUnique()
            .HasFilter("\"NhanhProductId\" IS NOT NULL");

        builder.Property(p => p.ParentProductId)
            .HasMaxLength(100);

        builder.Property(p => p.ParentProductCode)
            .HasMaxLength(200);

        builder.Property(p => p.ParentProductName)
            .HasMaxLength(500);

        builder.Property(p => p.Barcode)
            .HasMaxLength(200);

        builder.Property(p => p.ProductType)
            .HasMaxLength(100);

        builder.Property(p => p.ProductCode)
            .HasMaxLength(200);

        builder.Property(p => p.ProductName)
            .HasMaxLength(500);

        builder.Property(p => p.OtherName)
            .HasMaxLength(500);

        builder.Property(p => p.Unit)
            .HasMaxLength(50);

        builder.Property(p => p.CoverImage)
            .HasMaxLength(2000);

        builder.Property(p => p.ImportVatMode)
            .HasMaxLength(100);

        builder.Property(p => p.SaleVatMode)
            .HasMaxLength(100);

        builder.Property(p => p.Status)
            .HasMaxLength(100);

        builder.Property(p => p.CategoryCode)
            .HasMaxLength(100);

        builder.Property(p => p.CategoryName)
            .HasMaxLength(300);

        builder.Property(p => p.InternalCategoryCode)
            .HasMaxLength(100);

        builder.Property(p => p.Brand)
            .HasMaxLength(200);

        builder.Property(p => p.Supplier)
            .HasMaxLength(500);

        builder.Property(p => p.WarrantyAddress)
            .HasMaxLength(1000);

        builder.Property(p => p.Slug)
            .HasMaxLength(500);

        builder.Property(p => p.ProductLabel)
            .HasMaxLength(300);

        builder.Property(p => p.PriceLabel)
            .HasMaxLength(100);

        builder.Property(p => p.Size)
            .HasMaxLength(100);

        builder.Property(p => p.Color)
            .HasMaxLength(100);

        builder.Property(p => p.Origin)
            .HasMaxLength(200);
        
        builder.Property(x => x.IsPin)
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.SeedComboProducts();
    }
}
