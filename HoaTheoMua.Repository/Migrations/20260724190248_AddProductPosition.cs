using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Position",
                table: "Products",
                type: "numeric(28,10)",
                precision: 28,
                scale: 10,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Products");
        }
    }
}
