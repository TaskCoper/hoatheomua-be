using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_fields_Group_table_Config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "level",
                table: "JobDescriptions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "Configs",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Configs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configs_Group",
                table: "Configs",
                column: "Group");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Configs_Group",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Configs");

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "JobDescriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "Configs",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
