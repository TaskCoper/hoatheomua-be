using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ExtendPostImportAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PostImportChannel_Status",
                table: "PostImportChannel");

            migrationBuilder.AddColumn<string>(
                name: "Preparation",
                table: "PostImportRow",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PostImportChannel_Status",
                table: "PostImportChannel",
                sql: "\"Status\" IN ('Pending','Submitted','Existing','Rejected','Waiting')");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"PostImportChannel\" SET \"Status\" = 'Rejected', \"ErrorCode\" = 'IMPORT_WAITING_DOWNGRADED' WHERE \"Status\" = 'Waiting'");
            migrationBuilder.DropCheckConstraint(
                name: "CK_PostImportChannel_Status",
                table: "PostImportChannel");

            migrationBuilder.DropColumn(
                name: "Preparation",
                table: "PostImportRow");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PostImportChannel_Status",
                table: "PostImportChannel",
                sql: "\"Status\" IN ('Pending','Submitted','Existing','Rejected')");

        }
    }
}
