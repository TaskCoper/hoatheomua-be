using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class seedata_Orders_And_Transactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrackNumber",
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Metadata", "Status", "TrackNumber", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444441"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Pending", "TRK1001", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("44444444-4444-4444-4444-444444444442"), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Paid", "TRK1002", null, new Guid("33333333-3333-3333-3333-333333333334") },
                    { new Guid("44444444-4444-4444-4444-444444444443"), new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Shipped", "TRK1003", null, new Guid("33333333-3333-3333-3333-333333333335") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Delivered", "TRK1004", null, new Guid("33333333-3333-3333-3333-333333333336") },
                    { new Guid("44444444-4444-4444-4444-444444444445"), new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Cancelled", "TRK1005", null, new Guid("33333333-3333-3333-3333-333333333337") },
                    { new Guid("44444444-4444-4444-4444-444444444446"), new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Pending", "TRK1006", null, new Guid("33333333-3333-3333-3333-333333333338") },
                    { new Guid("44444444-4444-4444-4444-444444444447"), new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Paid", "TRK1007", null, new Guid("33333333-3333-3333-3333-333333333339") }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountNumber", "Accumulated", "Code", "Content", "CreatedAt", "Description", "Gateway", "IsDeleted", "OrderId", "ReferenceCode", "SePayId", "SubAccount", "TransactionDate", "TransferAmount", "TransferType", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555551"), "123456789", 100000m, "SEP1001", "Payment for TRK1001", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444441"), "REF1001", 1001L, "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555552"), "123456789", 250000m, "SEP1002", "Payment for TRK1002", new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444442"), "REF1002", 1002L, "", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 150000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555553"), "123456789", 450000m, "SEP1003", "Payment for TRK1003", new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444443"), "REF1003", 1003L, "", new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 200000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555554"), "123456789", 700000m, "SEP1004", "Payment for TRK1004", new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444444"), "REF1004", 1004L, "", new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 250000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "123456789", 650000m, "SEP1005", "Refund for TRK1005", new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444445"), "REF1005", 1005L, "", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, "OUT", null },
                    { new Guid("55555555-5555-5555-5555-555555555556"), "123456789", 750000m, "SEP1006", "Payment for TRK1006", new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444446"), "REF1006", 1006L, "", new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), 100000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555557"), "123456789", 1050000m, "SEP1007", "Payment for TRK1007", new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444447"), "REF1007", 1007L, "", new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), 300000m, "IN", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555556"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555557"));

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

            migrationBuilder.AlterColumn<decimal>(
                name: "TrackNumber",
                table: "Orders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
