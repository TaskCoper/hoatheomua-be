using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductExcelMappingComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_NhanhProductId",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<decimal>(
                name: "PendingImportStock",
                table: "Products",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NhanhProductId",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<decimal>(
                name: "BranchPrice",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BranchPriceDuplicate",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BranchPriceWithVat",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImportVat",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceRowNumber",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "StockError",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockTransferring",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Vat",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebOrder",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WholesaleBranchPrice",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WholesaleBranchPriceDuplicate",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WholesaleBranchPriceWithVat",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_NhanhProductId",
                table: "Products",
                column: "NhanhProductId",
                unique: true,
                filter: "\"NhanhProductId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_NhanhProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BranchPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BranchPriceDuplicate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BranchPriceWithVat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImportVat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SourceRowNumber",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockError",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockTransferring",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Vat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WebOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WholesaleBranchPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WholesaleBranchPriceDuplicate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WholesaleBranchPriceWithVat",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PendingImportStock",
                table: "Products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NhanhProductId",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_NhanhProductId",
                table: "Products",
                column: "NhanhProductId",
                unique: true);
        }
    }
}
