using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addColumnFactorInProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboSpecification_Products_MatId",
                table: "ComboSpecification");

            migrationBuilder.DropForeignKey(
                name: "FK_ComboSpecification_Products_SubId",
                table: "ComboSpecification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComboSpecification",
                table: "ComboSpecification");

            migrationBuilder.RenameTable(
                name: "ComboSpecification",
                newName: "ComboSpecifications");

            migrationBuilder.RenameIndex(
                name: "IX_ComboSpecification_SubId_MatId",
                table: "ComboSpecifications",
                newName: "IX_ComboSpecifications_SubId_MatId");

            migrationBuilder.RenameIndex(
                name: "IX_ComboSpecification_MatId",
                table: "ComboSpecifications",
                newName: "IX_ComboSpecifications_MatId");

            migrationBuilder.AddColumn<decimal>(
                name: "Factor",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComboSpecifications",
                table: "ComboSpecifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComboSpecifications_Products_MatId",
                table: "ComboSpecifications",
                column: "MatId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComboSpecifications_Products_SubId",
                table: "ComboSpecifications",
                column: "SubId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboSpecifications_Products_MatId",
                table: "ComboSpecifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ComboSpecifications_Products_SubId",
                table: "ComboSpecifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComboSpecifications",
                table: "ComboSpecifications");

            migrationBuilder.DropColumn(
                name: "Factor",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "ComboSpecifications",
                newName: "ComboSpecification");

            migrationBuilder.RenameIndex(
                name: "IX_ComboSpecifications_SubId_MatId",
                table: "ComboSpecification",
                newName: "IX_ComboSpecification_SubId_MatId");

            migrationBuilder.RenameIndex(
                name: "IX_ComboSpecifications_MatId",
                table: "ComboSpecification",
                newName: "IX_ComboSpecification_MatId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComboSpecification",
                table: "ComboSpecification",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComboSpecification_Products_MatId",
                table: "ComboSpecification",
                column: "MatId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComboSpecification_Products_SubId",
                table: "ComboSpecification",
                column: "SubId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
