using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_order_details_nhanhvn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChietKhauMotSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "ChietKhauSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "DanhMucSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "DonViTinh",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "GhiChuSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "GiaBan",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "GiaBanTruocVat",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "GiaSanPhamSauChietKhau",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "GiaVon",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "GiaVonDonHang",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "Imei",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "KhoiLuongSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "LoHang",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "LoaiGiaSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "MaSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "MaSanPhamCha",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "MaVach",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "NhanVienDongGoi",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "QuaTangKem",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "SoLuong",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "SoLuongDonViNhoNhat",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "SourceRowNumber",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "TenSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "TenSanPhamCha",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "TienVatSanPham",
                table: "OrderNhanhVns");

            migrationBuilder.CreateTable(
                name: "OrderNhanhVnDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNhanhVnId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenSanPham = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    QuaTangKem = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Imei = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LoHang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaSanPhamCha = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TenSanPhamCha = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MaSanPham = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaVach = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DanhMucSanPham = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    KhoiLuongSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaBan = table.Column<decimal>(type: "numeric", nullable: true),
                    LoaiGiaSanPham = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GiaBanTruocVat = table.Column<decimal>(type: "numeric", nullable: true),
                    TienVatSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    SoLuong = table.Column<int>(type: "integer", nullable: true),
                    ChietKhauMotSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    ChietKhauSanPham = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaSanPhamSauChietKhau = table.Column<decimal>(type: "numeric", nullable: true),
                    GhiChuSanPham = table.Column<string>(type: "text", nullable: true),
                    GiaVon = table.Column<decimal>(type: "numeric", nullable: true),
                    GiaVonDonHang = table.Column<decimal>(type: "numeric", nullable: true),
                    DonViTinh = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SoLuongDonViNhoNhat = table.Column<int>(type: "integer", nullable: true),
                    NhanVienDongGoi = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNhanhVnDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderNhanhVnDetails_OrderNhanhVns_OrderNhanhVnId",
                        column: x => x.OrderNhanhVnId,
                        principalTable: "OrderNhanhVns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVnDetails_CreatedAt",
                table: "OrderNhanhVnDetails",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVnDetails_MaSanPham",
                table: "OrderNhanhVnDetails",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVnDetails_OrderNhanhVnId",
                table: "OrderNhanhVnDetails",
                column: "OrderNhanhVnId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVnDetails_TenSanPham",
                table: "OrderNhanhVnDetails",
                column: "TenSanPham");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderNhanhVnDetails");

            migrationBuilder.AddColumn<decimal>(
                name: "ChietKhauMotSanPham",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ChietKhauSanPham",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucSanPham",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViTinh",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChuSanPham",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaBan",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaBanTruocVat",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaSanPhamSauChietKhau",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaVon",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaVonDonHang",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imei",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KhoiLuongSanPham",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoHang",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiGiaSanPham",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaSanPham",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaSanPhamCha",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaVach",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanVienDongGoi",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuaTangKem",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuong",
                table: "OrderNhanhVns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongDonViNhoNhat",
                table: "OrderNhanhVns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceRowNumber",
                table: "OrderNhanhVns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TenSanPham",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenSanPhamCha",
                table: "OrderNhanhVns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TienVatSanPham",
                table: "OrderNhanhVns",
                type: "numeric",
                nullable: true);
        }
    }
}
