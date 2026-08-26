using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_product_listing_indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_ParentProductId_ProductType",
                table: "Products",
                columns: new[] { "ParentProductId", "ProductType" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Position_CreatedAt",
                table: "Products",
                columns: new[] { "Position", "CreatedAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ParentProductId_ProductType",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Position_CreatedAt",
                table: "Products");
        }
    }
}
