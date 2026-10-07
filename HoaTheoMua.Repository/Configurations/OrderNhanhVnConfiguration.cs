using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class OrderNhanhVnConfiguration : IEntityTypeConfiguration<OrderNhanhVn>
{
    public void Configure(EntityTypeBuilder<OrderNhanhVn> builder)
    {

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.IdDonHangNhanh)
           // .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.IdDonHangNhanh)
                .IsUnique();
        builder.HasIndex(x => x.IdDonHangApi);
        builder.HasIndex(x => x.MaVanDon);
        builder.HasIndex(x => x.MaKhachHang);
        builder.HasIndex(x => x.TenKhachHang);
        builder.HasIndex(x => x.SoDienThoaiKhachHang);
        builder.HasIndex(x => x.TrangThaiDonHang);
        builder.HasIndex(x => x.NguonDonHang);
        builder.HasIndex(x => x.CreatedAt);

        builder.Property(x => x.TenKhachHang).HasMaxLength(250);
        builder.Property(x => x.EmailKhachHang).HasMaxLength(250);
        builder.Property(x => x.TenNguoiLienHe).HasMaxLength(250);
        builder.Property(x => x.EmailNguoiLienHe).HasMaxLength(250);
        builder.Property(x => x.EmailNguoiLienHeThuHai).HasMaxLength(250);

        builder.Property(x => x.MaVanDon).HasMaxLength(100);
        builder.Property(x => x.MaKhachHang).HasMaxLength(100);
        builder.Property(x => x.IdDonHangApi).HasMaxLength(100);
        builder.Property(x => x.NguonDonHang).HasMaxLength(100);
        builder.Property(x => x.TrangThaiDonHang).HasMaxLength(100);
        builder.Property(x => x.CongVanChuyen).HasMaxLength(100);
        builder.Property(x => x.MaGiamGia).HasMaxLength(100);
        builder.Property(x => x.MaThamChieu).HasMaxLength(100);
        builder.Property(x => x.HangVanChuyen).HasMaxLength(150);

        builder.Property(x => x.SoDienThoaiKhachHang).HasMaxLength(30);
        builder.Property(x => x.MaSoThue).HasMaxLength(50);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()");
        
        builder.HasIndex(x => x.ThoiGianDonHang)
            .HasFilter("""
                       "IsDeleted" = false
                       AND ("TrangThaiDonHang" IS NULL
                            OR "TrangThaiDonHang" NOT IN ('63', 'Khách hủy'))
                       """);


    }
}