using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class OrderNhanhVnConfiguration : IEntityTypeConfiguration<OrderNhanhVn>
{
    public void Configure(EntityTypeBuilder<OrderNhanhVn> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UpdatedAt);

        // Id đơn hàng của Nhanh.vn
        builder.Property(x => x.IdDonHangNhanh)
            .IsRequired();

        builder.HasIndex(x => x.IdDonHangNhanh)
            .IsUnique();
        
        builder.HasIndex(x => x.MaVanDon);

        builder.HasIndex(x => x.SoDienThoaiKhachHang);

        builder.HasIndex(x => x.TenKhachHang);

        builder.HasIndex(x => x.TrangThaiDonHang);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasIndex(x => x.NguonDonHang);

        builder.HasIndex(x => x.MaKhachHang);

        builder.HasIndex(x => x.IdDonHangApi);
        

        builder.Property(x => x.TenKhachHang)
            .HasMaxLength(250);

        builder.Property(x => x.SoDienThoaiKhachHang)
            .HasMaxLength(30);

        builder.Property(x => x.EmailKhachHang)
            .HasMaxLength(250);

        builder.Property(x => x.MaVanDon)
            .HasMaxLength(100);

        builder.Property(x => x.MaKhachHang)
            .HasMaxLength(100);

        builder.Property(x => x.IdDonHangApi)
            .HasMaxLength(100);

        builder.Property(x => x.NguonDonHang)
            .HasMaxLength(100);

        builder.Property(x => x.TrangThaiDonHang)
            .HasMaxLength(100);

        builder.Property(x => x.HangVanChuyen)
            .HasMaxLength(150);

        builder.Property(x => x.CongVanChuyen)
            .HasMaxLength(100);

        builder.Property(x => x.MaGiamGia)
            .HasMaxLength(100);

        builder.Property(x => x.MaThamChieu)
            .HasMaxLength(100);

        builder.Property(x => x.MaSoThue)
            .HasMaxLength(50);

        builder.Property(x => x.EmailNguoiLienHe)
            .HasMaxLength(250);

        builder.Property(x => x.EmailNguoiLienHeThuHai)
            .HasMaxLength(250);

        builder.Property(x => x.TenNguoiLienHe)
            .HasMaxLength(250);

        builder.Property(x => x.GiaTriDonHang)
            .HasPrecision(18, 2);

        builder.Property(x => x.GiaVonDonHang)
            .HasPrecision(18, 2);

        builder.Property(x => x.LoiNhuan)
            .HasPrecision(18, 2);

        builder.Property(x => x.PhiVanChuyen)
            .HasPrecision(18, 2);

        builder.Property(x => x.TongThu)
            .HasPrecision(18, 2);

        builder.Property(x => x.GiaBan)
            .HasPrecision(18, 2);

        builder.Property(x => x.GiaBanTruocVat)
            .HasPrecision(18, 2);

        builder.Property(x => x.GiaSanPhamSauChietKhau)
            .HasPrecision(18, 2);

        builder.Property(x => x.GiaVon)
            .HasPrecision(18, 2);
        

        builder.Property(x => x.DiaChiKhachHang);

        builder.Property(x => x.GhiChuCuaKhach);

        builder.Property(x => x.GhiChuNoiBo);

        builder.Property(x => x.GhiChuSanPham);

        builder.Property(x => x.GhiChuHangVanChuyen);
    }
}