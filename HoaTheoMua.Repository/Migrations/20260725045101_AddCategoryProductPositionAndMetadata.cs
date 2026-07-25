using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryProductPositionAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetaData",
                table: "CategoryProducts",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "CategoryProducts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaData",
                table: "CategoryProducts");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "CategoryProducts");
        }
    }
}
