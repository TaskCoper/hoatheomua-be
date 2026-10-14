using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_filter_isDeletdfalse_orderNhanhVn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderNhanhVns_IdDonHangNhanh",
                table: "OrderNhanhVns");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_IdDonHangNhanh",
                table: "OrderNhanhVns",
                column: "IdDonHangNhanh",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderNhanhVns_IdDonHangNhanh",
                table: "OrderNhanhVns");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_IdDonHangNhanh",
                table: "OrderNhanhVns",
                column: "IdDonHangNhanh",
                unique: true);
        }
    }
}
