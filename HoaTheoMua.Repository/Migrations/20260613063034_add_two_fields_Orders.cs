using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_two_fields_Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AhahaMoveOrderId",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AhahaMoveTrackingUrl",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NhanhOrderId",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NhanhTrackingUrl",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM1011", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM104311", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM865", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM5543", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444445"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM6444", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444446"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM1234t1", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444447"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM10234", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444448"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM1435", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444449"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM1015", "https://ahahovetrackingUrl", "NO14", "https://123" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444450"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { "AHM1013", "https://ahahovetrackingUrl", "NO14", "https://123 " });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AhahaMoveOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AhahaMoveTrackingUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NhanhOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NhanhTrackingUrl",
                table: "Orders");
        }
    }
}
