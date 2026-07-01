using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_fields_in_table_Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExportInvoice",
                table: "Orders",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptAddress",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptEmail",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptFullName",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptPhoneNumber",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsExportInvoice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiptAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiptEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiptFullName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiptPhoneNumber",
                table: "Orders");
        }
    }
}
