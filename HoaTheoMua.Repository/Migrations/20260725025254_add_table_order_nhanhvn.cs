using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_table_order_nhanhvn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderNhanhVns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRowNumber = table.Column<int>(type: "integer", nullable: false),
                    IdDonHangNhanh = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdDonHangApi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LoaiDon = table.Column<string>(type: "text", nullable: true),
                    ThoiGianDonHang = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NguoiTaoDon = table.Column<string>(type: "text", nullable: true),
                    CuaHang = table.Column<string>(type: "text", nullable: true),
                    Nguon = table.Column<string>(type: "text", nullable: true),
                    KhoTaoDon = table.Column<string>(type: "text", nullable: true),
                    ThanhPhoKhoHang = table.Column<string>(type: "text", nullable: true),
                    QuanHuyenKhoHang = table.Column<string>(type: "text", nullable: true),
                    KhoLayHang = table.Column<string>(type: "text", nullable: true),
                    ThanhPhoLayHang = table.Column<string>(type: "text", nullable: true),
                    QuanHuyenLayHang = table.Column<string>(type: "text", nullable: true),
                    TenKhachHang = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MaKhachHang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SoDienThoaiKhachHang = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    EmailKhachHang = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CapDoKhachHang = table.Column<string>(type: "text", nullable: true),
                    NgaySinhKhachHang = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ThanhPhoKhachHang = table.Column<string>(type: "text", nullable: true),
                    QuanHuyenKhachHang = table.Column<string>(type: "text", nullable: true),
                    PhuongXaKhachHang = table.Column<string>(type: "text", nullable: true),
                    DiaChiKhachHang = table.Column<string>(type: "text", nullable: true),
                    TenSanPham = table.Column<string>(type: "text", nullable: true),
                    QuaTangKem = table.Column<string>(type: "text", nullable: true),
                    Imei = table.Column<string>(type: "text", nullable: true),
                    LoHang = table.Column<string>(type: "text", nullable: true),
                    MaSanPhamCha = table.Column<string>(type: "text", nullable: true),
                    TenSanPhamCha = table.Column<string>(type: "text", nullable: true),
                    MaSanPham = table.Column<string>(type: "text", nullable: true),
                    MaVach = table.Column<string>(type: "text", nullable: true),
                    DanhMucSanPham = table.Column<string>(type: "text", nullable: true),
                    KhoiLuongSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaBan = table.Column<decimal>(type: "numeric", nullable: true),
                    LoaiGiaSanPham = table.Column<string>(type: "text", nullable: true),
                    GiaBanTruocVat = table.Column<decimal>(type: "numeric", nullable: true),
                    TienVatSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    SoLuong = table.Column<int>(type: "integer", nullable: true),
                    ChietKhauMotSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    ChietKhauSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaSanPhamSauChietKhau = table.Column<decimal>(type: "numeric", nullable: true),
                    GhiChuSanPham = table.Column<string>(type: "text", nullable: true),
                    TongKhoiLuong = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaTriDonHang = table.Column<decimal>(type: "numeric", nullable: true),
                    PhiVanChuyen = table.Column<decimal>(type: "numeric", nullable: true),
                    PhiVuotCan = table.Column<decimal>(type: "numeric", nullable: true),
                    PhiKhaiGia = table.Column<decimal>(type: "numeric", nullable: true),
                    PhiChuyenHoan = table.Column<decimal>(type: "numeric", nullable: true),
                    PhiThuTienHo = table.Column<decimal>(type: "numeric", nullable: true),
                    PhiShipBaoKhach = table.Column<decimal>(type: "numeric", nullable: true),
                    CongVanChuyen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HangVanChuyen = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DichVuVanChuyen = table.Column<string>(type: "text", nullable: true),
                    SoNgayChuyenHang = table.Column<int>(type: "integer", nullable: true),
                    MaVanDon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BienBan = table.Column<string>(type: "text", nullable: true),
                    TienChuyenKhoan = table.Column<decimal>(type: "numeric", nullable: true),
                    TaiKhoanChuyenKhoan = table.Column<string>(type: "text", nullable: true),
                    TienChietKhau = table.Column<decimal>(type: "numeric", nullable: true),
                    TienDatCoc = table.Column<decimal>(type: "numeric", nullable: true),
                    TaiKhoanDatCoc = table.Column<string>(type: "text", nullable: true),
                    TongThu = table.Column<decimal>(type: "numeric", nullable: true),
                    TaiKhoanQuetThe = table.Column<string>(type: "text", nullable: true),
                    TienQuetThe = table.Column<decimal>(type: "numeric", nullable: true),
                    MaGiaoDichQuetThe = table.Column<string>(type: "text", nullable: true),
                    GiaVon = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaVonDonHang = table.Column<decimal>(type: "numeric", nullable: true),
                    DonViTinh = table.Column<string>(type: "text", nullable: true),
                    GhiChuNoiBo = table.Column<string>(type: "text", nullable: true),
                    GhiChuCuaKhach = table.Column<string>(type: "text", nullable: true),
                    TrangThaiHoaDonDienTu = table.Column<string>(type: "text", nullable: true),
                    SoHoaDonDienTu = table.Column<string>(type: "text", nullable: true),
                    NgayXuatHoaDonDienTu = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaCoQuanThueHoaDonDienTu = table.Column<string>(type: "text", nullable: true),
                    TrangThaiDonHang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LyDoHuy = table.Column<string>(type: "text", nullable: true),
                    NhanVienChamSocKhachHang = table.Column<string>(type: "text", nullable: true),
                    NgayXacNhan = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NgayGuiHangVanChuyen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NgayBanGiao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NgayThanhCong = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GioThanhCong = table.Column<string>(type: "text", nullable: true),
                    NguonDonHang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GhiChuHangVanChuyen = table.Column<string>(type: "text", nullable: true),
                    CanNangHangVanChuyen = table.Column<decimal>(type: "numeric", nullable: true),
                    MaGiamGia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdDonTra = table.Column<string>(type: "text", nullable: true),
                    IdDonGoc = table.Column<string>(type: "text", nullable: true),
                    SoThuTuTheoNgay = table.Column<int>(type: "integer", nullable: true),
                    BienBanBanGiao = table.Column<string>(type: "text", nullable: true),
                    MaAffiliate = table.Column<string>(type: "text", nullable: true),
                    NhomKhachHang = table.Column<string>(type: "text", nullable: true),
                    TienVat = table.Column<decimal>(type: "numeric", nullable: true),
                    TyLeVat = table.Column<string>(type: "text", nullable: true),
                    TienTieuDiem = table.Column<decimal>(type: "numeric", nullable: true),
                    Nhan = table.Column<string>(type: "text", nullable: true),
                    NgayHenThanhToan = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TrangThaiDoiSoat = table.Column<string>(type: "text", nullable: true),
                    NgayDoiSoat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TrangThaiThanhToanDoanhNghiep = table.Column<string>(type: "text", nullable: true),
                    NgayThanhToanDoanhNghiep = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NhanVienBanHang = table.Column<string>(type: "text", nullable: true),
                    LoiNhuan = table.Column<decimal>(type: "numeric", nullable: true),
                    TyLeLoiNhuan = table.Column<string>(type: "text", nullable: true),
                    FacebookPsid = table.Column<string>(type: "text", nullable: true),
                    FacebookPageUserId = table.Column<string>(type: "text", nullable: true),
                    SoDienThoaiFacebook = table.Column<string>(type: "text", nullable: true),
                    GiaTriFacebook = table.Column<string>(type: "text", nullable: true),
                    SoLuongDonViNhoNhat = table.Column<int>(type: "integer", nullable: true),
                    NhanVienDongGoi = table.Column<string>(type: "text", nullable: true),
                    IdPhapNhan = table.Column<string>(type: "text", nullable: true),
                    MaSoThue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmailNguoiLienHe = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EmailNguoiLienHeThuHai = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SoDienThoaiNguoiLienHe = table.Column<string>(type: "text", nullable: true),
                    TenNguoiLienHe = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MaThamChieu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DiaChiKhachHangTrenHoaDonDienTu = table.Column<string>(type: "text", nullable: true),
                    TenKhachHangTrenHoaDonDienTu = table.Column<string>(type: "text", nullable: true),
                    LoaiKhachHangHoaDon = table.Column<string>(type: "text", nullable: true),
                    CoLayHoaDon = table.Column<string>(type: "text", nullable: true),
                    MaCoQuanThueHoaDon = table.Column<string>(type: "text", nullable: true),
                    SoHoaDon = table.Column<string>(type: "text", nullable: true),
                    NgayPhatHanhHoaDon = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNhanhVns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_CreatedAt",
                table: "OrderNhanhVns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_IdDonHangApi",
                table: "OrderNhanhVns",
                column: "IdDonHangApi");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_IdDonHangNhanh",
                table: "OrderNhanhVns",
                column: "IdDonHangNhanh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_MaKhachHang",
                table: "OrderNhanhVns",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_MaVanDon",
                table: "OrderNhanhVns",
                column: "MaVanDon");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_NguonDonHang",
                table: "OrderNhanhVns",
                column: "NguonDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_SoDienThoaiKhachHang",
                table: "OrderNhanhVns",
                column: "SoDienThoaiKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_TenKhachHang",
                table: "OrderNhanhVns",
                column: "TenKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_TrangThaiDonHang",
                table: "OrderNhanhVns",
                column: "TrangThaiDonHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderNhanhVns");
        }
    }
}
