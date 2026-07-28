using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FilterCategoryProductDetailUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CategoryProductDetails_CategoryProductId_ProductId",
                table: "CategoryProductDetails");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProductDetails_CategoryProductId_ProductId",
                table: "CategoryProductDetails",
                columns: new[] { "CategoryProductId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CategoryProductDetails_CategoryProductId_ProductId",
                table: "CategoryProductDetails");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProductDetails_CategoryProductId_ProductId",
                table: "CategoryProductDetails",
                columns: new[] { "CategoryProductId", "ProductId" },
                unique: true);
        }
    }
}
