using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations;

public partial class AddNhanhCompletedShippingOrderId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.AddColumn<string>(
            name: "NhanhCompletedShippingOrderId", table: "Orders", type: "text", nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropColumn(name: "NhanhCompletedShippingOrderId", table: "Orders");
}
