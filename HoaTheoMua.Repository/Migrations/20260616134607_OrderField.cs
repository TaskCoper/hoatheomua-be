using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class OrderField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444445"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444446"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444447"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444448"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444449"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444450"));

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalFee",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShortAddress",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingFee",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProductFree",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Orders");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalFee",
                table: "Orders",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "ShortAddress",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingFee",
                table: "Orders",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProductFree",
                table: "Orders",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Orders",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Orders",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "Address", "AhahaMoveOrderId", "AhahaMoveTrackingUrl", "CreatedAt", "FullName", "IsDeleted", "Latitude", "Longitude", "NhanhOrderId", "NhanhTrackingUrl", "Phone", "ProductFree", "Remarks", "ShippingFee", "ShortAddress", "Status", "TotalFee", "TrackNumber", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444441"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Pending", null, "TRK1001", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("44444444-4444-4444-4444-444444444442"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Paid", null, "TRK1002", null, new Guid("33333333-3333-3333-3333-333333333334") },
                    { new Guid("44444444-4444-4444-4444-444444444443"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Shipped", null, "TRK1003", null, new Guid("33333333-3333-3333-3333-333333333335") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Delivered", null, "TRK1004", null, new Guid("33333333-3333-3333-3333-333333333336") },
                    { new Guid("44444444-4444-4444-4444-444444444445"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Cancelled", null, "TRK1005", null, new Guid("33333333-3333-3333-3333-333333333337") },
                    { new Guid("44444444-4444-4444-4444-444444444446"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Pending", null, "TRK1006", null, new Guid("33333333-3333-3333-3333-333333333338") },
                    { new Guid("44444444-4444-4444-4444-444444444447"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Paid", null, "TRK1007", null, new Guid("33333333-3333-3333-3333-333333333339") },
                    { new Guid("44444444-4444-4444-4444-444444444448"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Pending", null, "TRK1008", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("44444444-4444-4444-4444-444444444449"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Paid", null, "TRK1009", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("44444444-4444-4444-4444-444444444450"), null, null, null, new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null, null, null, null, null, null, null, null, null, "Shipped", null, "TRK1010", null, new Guid("33333333-3333-3333-3333-333333333333") }
                });
        }
    }
}
