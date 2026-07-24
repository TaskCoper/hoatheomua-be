using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddNhanhVnProductEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NhanhProductId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentProductId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProductCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProductName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CoverImage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WarrantyMonths = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: true),
                    ImportPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    ImportPriceWithVat = table.Column<decimal>(type: "numeric", nullable: true),
                    ImportVatMode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    PriceWithVat = table.Column<decimal>(type: "numeric", nullable: true),
                    SaleVatMode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProfitPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    CostPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    WholesalePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    WholesalePriceWithVat = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalStock = table.Column<decimal>(type: "numeric", nullable: true),
                    Stock = table.Column<decimal>(type: "numeric", nullable: true),
                    WarehouseStock = table.Column<decimal>(type: "numeric", nullable: true),
                    Held = table.Column<decimal>(type: "numeric", nullable: true),
                    AvailableForSale = table.Column<decimal>(type: "numeric", nullable: true),
                    PreOrder = table.Column<decimal>(type: "numeric", nullable: true),
                    DeliveringStock = table.Column<decimal>(type: "numeric", nullable: true),
                    PendingImportStock = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CategoryCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CategoryName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Supplier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NhanhCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PriceLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Size = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_NhanhProductId",
                table: "Products",
                column: "NhanhProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
