using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_field_vat_Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductFree",
                table: "Orders",
                newName: "ProductFee");

            migrationBuilder.AddColumn<decimal>(
                name: "Vat",
                table: "Orders",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vat",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ProductFee",
                table: "Orders",
                newName: "ProductFree");
        }
    }
}
