using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class OrderNhanhVnDetailConfiguration : IEntityTypeConfiguration<OrderNhanhVnDetail>
{
    public void Configure(EntityTypeBuilder<OrderNhanhVnDetail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenSanPham).HasMaxLength(250);
        builder.Property(x => x.MaSanPham).HasMaxLength(100);
        builder.Property(x => x.MaVach).HasMaxLength(100);
        builder.Property(x => x.DanhMucSanPham).HasMaxLength(250);
        builder.Property(x => x.DonViTinh).HasMaxLength(50);
        builder.Property(x => x.LoaiGiaSanPham).HasMaxLength(50);
        builder.Property(x => x.MaSanPhamCha).HasMaxLength(100);
        builder.Property(x => x.TenSanPhamCha).HasMaxLength(250);
        builder.Property(x => x.LoHang).HasMaxLength(100);
        builder.Property(x => x.QuaTangKem).HasMaxLength(250);
        builder.Property(x => x.Imei).HasMaxLength(100);
        builder.Property(x => x.NhanVienDongGoi).HasMaxLength(250);

        builder.HasIndex(x => x.OrderNhanhVnId);
        builder.HasIndex(x => x.TenSanPham);
        builder.HasIndex(x => x.MaSanPham);
        builder.HasIndex(x => x.CreatedAt);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(x => x.OrderNhanhVn)
            .WithMany(x => x.OrderNhanhVnDetails)
            .HasForeignKey(x => x.OrderNhanhVnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
    