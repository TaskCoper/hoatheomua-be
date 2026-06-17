using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ConfigValueField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Orders");

            // PostgreSQL cannot implicitly cast text -> integer, so emit an explicit
            // USING clause. NULLIF maps empty strings to NULL before the cast.
            migrationBuilder.Sql(
                "ALTER TABLE \"Orders\" ALTER COLUMN \"NhanhOrderId\" TYPE integer USING NULLIF(\"NhanhOrderId\", '')::integer;");

            // PostgreSQL cannot implicitly cast text -> jsonb, so emit an explicit USING clause.
            migrationBuilder.Sql(
                "ALTER TABLE \"Configs\" ALTER COLUMN \"Value\" TYPE jsonb USING \"Value\"::jsonb;");

            migrationBuilder.AlterColumn<string>(
                name: "MapId",
                table: "CompanyAddresses",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NhanhOrderId",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Configs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "MapId",
                table: "CompanyAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
