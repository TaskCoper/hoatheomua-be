using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fix_relational_news_and_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "NewsList",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777771"),
                column: "UserId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777772"),
                column: "UserId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777773"),
                column: "UserId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777774"),
                column: "UserId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777775"),
                column: "UserId",
                value: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777776"),
                column: "UserId",
                value: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "UserId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777778"),
                column: "UserId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777779"),
                column: "UserId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "NewsList",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777780"),
                column: "UserId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.CreateIndex(
                name: "IX_NewsList_UserId",
                table: "NewsList",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsList_Users_UserId",
                table: "NewsList",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsList_Users_UserId",
                table: "NewsList");

            migrationBuilder.DropIndex(
                name: "IX_NewsList_UserId",
                table: "NewsList");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "NewsList");
        }
    }
}
