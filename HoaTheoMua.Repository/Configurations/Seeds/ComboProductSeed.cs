using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class ComboProductSeed
{
    private static readonly DateTimeOffset SeededAt =
        new(2026, 8, 24, 0, 0, 0, TimeSpan.Zero);

    public static void SeedComboProducts(this EntityTypeBuilder<Product> builder)
    {
        builder.HasData(
            Create("5d7c48f1-2e63-4a9b-8f14-cb7291d63805", "SEED-COMBO-001",
                "CB-ROSE-001", "Bó hoa hồng Julibee", "Combo", null, 0),
            Create("a31f762c-9b48-4e05-bd67-285cf194a730", "SEED-COMBO-002",
                "CB-LILY-001", "Giỏ hoa Lily thanh lịch", "Combo", null, 0),
            Create("e84b15d9-6c27-4fa3-91de-73b5082ac649", "SEED-COMBO-003",
                "CB-SUNFLOWER-001", "Bó hoa hướng dương", "Combo", null, 0),

            Create("0c629e7a-4d13-48b5-a967-f21d80537c94", "SEED-MAT-001",
                "MAT-ROSE-001", "Hoa hồng Julibee", "Thành phần", "Cành", 50),
            Create("b7523a9d-81e6-457c-93f0-6ad4182ce705", "SEED-MAT-002",
                "MAT-PAPER-001", "Giấy gói hoa Hàn Quốc", "Phụ liệu", "Tờ", 30),
            Create("d80416c3-27a9-4fb1-85e6-39c7502ad184", "SEED-MAT-003",
                "MAT-RIBBON-001", "Ruy băng satin", "Phụ liệu", "Mét", 100),
            Create("46fa8b21-c953-41de-a607-128d7e3c95b4", "SEED-MAT-004",
                "MAT-FOAM-001", "Xốp cắm hoa", "Phụ liệu", "Miếng", 60),
            Create("6e19d4b7-52c8-469a-b031-f8472d5e16c3", "SEED-MAT-005",
                "MAT-LILY-001", "Hoa Lily trắng", "Thành phần", "Cành", 48),
            Create("83c5f920-1a74-4d6e-98b3-2f617c4a5d80", "SEED-MAT-006",
                "MAT-CARNATION-001", "Hoa lá phụ cẩm chướng", "Thành phần", "Cành", 40),
            Create("f24a7d18-93b6-40ce-a852-7c105e4d9b63", "SEED-MAT-007",
                "MAT-BASKET-001", "Giỏ mây cắm hoa", "Phụ liệu", "Cái", 20),
            Create("a9682f4c-75d1-438b-90e6-3c517d8a2f49", "SEED-MAT-008",
                "MAT-SUNFLOWER-001", "Hoa hướng dương", "Thành phần", "Cành", 64)
        );
    }

    private static object Create(
        string id,
        string nhanhProductId,
        string productCode,
        string productName,
        string productLabel,
        string? unit,
        decimal availableForSale)
    {
        return new
        {
            Id = Guid.Parse(id),
            SourceRowNumber = 0,
            NhanhProductId = nhanhProductId,
            ProductCode = productCode,
            ProductName = productName,
            ProductType = productLabel,
            ProductLabel = new List<string> { productLabel },
            Unit = unit,
            AvailableForSale = (decimal?)availableForSale,
            IsPin = false,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeededAt
        };
    }
}
