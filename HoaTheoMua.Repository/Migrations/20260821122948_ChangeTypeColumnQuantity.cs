using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeColumnQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ComboSpecifications_SubId_MatId",
                table: "ComboSpecifications");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ComboSpecifications",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_ComboSpecifications_SubId_MatId",
                table: "ComboSpecifications",
                columns: new[] { "SubId", "MatId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ComboSpecifications_SubId_MatId",
                table: "ComboSpecifications");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "ComboSpecifications",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboSpecifications_SubId_MatId",
                table: "ComboSpecifications",
                columns: new[] { "SubId", "MatId" },
                unique: true);
        }
    }
}
