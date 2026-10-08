using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations;

public partial class UniqueActiveVoucherCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Vouchers_Code", table: "Vouchers");
        migrationBuilder.CreateIndex(
            name: "IX_Vouchers_Code",
            table: "Vouchers",
            column: "Code",
            unique: true,
            filter: "\"IsActive\" = true");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Nếu đã có mã inactive trùng nhau, phải xử lý dữ liệu trước khi quay lại index cũ.
        // Không tự xóa hoặc đổi mã voucher vì có thể ảnh hưởng lịch sử sử dụng.
        migrationBuilder.DropIndex(name: "IX_Vouchers_Code", table: "Vouchers");
        migrationBuilder.CreateIndex(
            name: "IX_Vouchers_Code",
            table: "Vouchers",
            column: "Code",
            unique: true);
    }
}
