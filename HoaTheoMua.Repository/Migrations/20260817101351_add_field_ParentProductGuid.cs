using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_field_ParentProductGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentProductGuid",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ParentProductGuid",
                table: "Products",
                column: "ParentProductGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Products_ParentProductGuid",
                table: "Products",
                column: "ParentProductGuid",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Products_ParentProductGuid",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ParentProductGuid",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ParentProductGuid",
                table: "Products");
        }
    }
}
