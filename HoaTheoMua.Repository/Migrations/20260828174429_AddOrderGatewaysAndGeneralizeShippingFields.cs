using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderGatewaysAndGeneralizeShippingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AhahaMoveTrackingUrl",
                table: "Orders",
                newName: "ShippingTrackingUrl");

            migrationBuilder.RenameColumn(
                name: "AhahaMoveOrderId",
                table: "Orders",
                newName: "ShippingPartyOrderId");

            migrationBuilder.AddColumn<int>(
                name: "PaymentGateway",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingGateway",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Orders"
                SET "PaymentGateway" = CASE
                    WHEN "QrUrl" IS NOT NULL AND "QrUrl" <> '' THEN 2
                    ELSE 1
                END,
                "ShippingGateway" = 1;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentGateway",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ShippingGateway",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_PaymentGateway",
                table: "Orders",
                sql: "\"PaymentGateway\" IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_ShippingGateway",
                table: "Orders",
                sql: "\"ShippingGateway\" IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_PaymentGateway",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_ShippingGateway",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentGateway",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingGateway",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShippingTrackingUrl",
                table: "Orders",
                newName: "AhahaMoveTrackingUrl");

            migrationBuilder.RenameColumn(
                name: "ShippingPartyOrderId",
                table: "Orders",
                newName: "AhahaMoveOrderId");
        }
    }
}
