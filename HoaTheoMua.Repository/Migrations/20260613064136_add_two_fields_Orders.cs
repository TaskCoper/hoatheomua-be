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
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AhahaMoveTrackingUrl",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanhOrderId",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanhTrackingUrl",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444445"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444446"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444447"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444448"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444449"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444450"),
                columns: new[] { "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "NhanhOrderId", "NhanhTrackingUrl" },
                values: new object[] { null, null, null, null });
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
