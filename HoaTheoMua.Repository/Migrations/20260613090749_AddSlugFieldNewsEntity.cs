using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugFieldNewsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "NewsList",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777771"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777772"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777773"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777774"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777775"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777776"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777778"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777779"),
                column: "slug",
                value: null);

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777780"),
                column: "slug",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "slug",
                table: "NewsList");
        }
    }
}
