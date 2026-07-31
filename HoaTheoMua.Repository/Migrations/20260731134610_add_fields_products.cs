using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_fields_products : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NhanhBrandId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NhanhCategoryId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NhanhTypeId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NhanhUpdatedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "OtherImages",
                table: "Products",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NhanhBrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NhanhCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NhanhTypeId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NhanhUpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OtherImages",
                table: "Products");
        }
    }
}
