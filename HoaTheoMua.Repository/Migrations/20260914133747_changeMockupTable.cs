using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changeMockupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReceiverName", table: "GeneratedCard");
            migrationBuilder.DropColumn(name: "SenderName", table: "GeneratedCard");

            migrationBuilder.RenameColumn(
                name: "RootID",
                table: "ClientHistory",
                newName: "RootId");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotCard",
                table: "OrderDetails",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql("CREATE TYPE \"FlowerReferenceType\" AS ENUM ('flower', 'bundle');");

            migrationBuilder.DropPrimaryKey(name: "PK_Mockup", table: "Mockup");
            migrationBuilder.RenameTable(name: "Mockup", newName: "FlowerReference");
            migrationBuilder.RenameIndex(
                name: "IX_Mockup_IsActive_CreatedAt",
                table: "FlowerReference",
                newName: "IX_FlowerReference_IsActive_CreatedAt");
            migrationBuilder.AddPrimaryKey(name: "PK_FlowerReference", table: "FlowerReference", column: "Id");

            migrationBuilder.AddColumn<global::HoaTheoMua.Repository.Enum.FlowerReferenceType>(
                name: "Type",
                table: "FlowerReference",
                type: "\"FlowerReferenceType\"",
                nullable: true);
            migrationBuilder.Sql("UPDATE \"FlowerReference\" SET \"Type\" = 'flower'::\"FlowerReferenceType\";");
            migrationBuilder.AlterColumn<global::HoaTheoMua.Repository.Enum.FlowerReferenceType>(
                name: "Type",
                table: "FlowerReference",
                type: "\"FlowerReferenceType\"",
                nullable: false,
                oldClrType: typeof(global::HoaTheoMua.Repository.Enum.FlowerReferenceType),
                oldType: "\"FlowerReferenceType\"",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Type", table: "FlowerReference");

            migrationBuilder.DropPrimaryKey(name: "PK_FlowerReference", table: "FlowerReference");
            migrationBuilder.RenameTable(name: "FlowerReference", newName: "Mockup");
            migrationBuilder.RenameIndex(
                name: "IX_FlowerReference_IsActive_CreatedAt",
                table: "Mockup",
                newName: "IX_Mockup_IsActive_CreatedAt");
            migrationBuilder.AddPrimaryKey(name: "PK_Mockup", table: "Mockup", column: "Id");

            migrationBuilder.Sql("DROP TYPE \"FlowerReferenceType\";");

            migrationBuilder.DropColumn(name: "SnapshotCard", table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "RootId",
                table: "ClientHistory",
                newName: "RootID");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "GeneratedCard",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "GeneratedCard",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
