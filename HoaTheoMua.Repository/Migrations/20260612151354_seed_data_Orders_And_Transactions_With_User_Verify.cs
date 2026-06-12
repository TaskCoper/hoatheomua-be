using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class seed_data_Orders_And_Transactions_With_User_Verify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Metadata", "Status", "TrackNumber", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444448"), new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Pending", "TRK1008", null, new Guid("e5c95b37-6dd1-4bcf-b560-e80d9dc60996") },
                    { new Guid("44444444-4444-4444-4444-444444444449"), new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Paid", "TRK1009", null, new Guid("e5c95b37-6dd1-4bcf-b560-e80d9dc60996") },
                    { new Guid("44444444-4444-4444-4444-444444444450"), new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "{}", "Shipped", "TRK1010", null, new Guid("e5c95b37-6dd1-4bcf-b560-e80d9dc60996") }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountNumber", "Accumulated", "Code", "Content", "CreatedAt", "Description", "Gateway", "IsDeleted", "OrderId", "ReferenceCode", "SePayId", "SubAccount", "TransactionDate", "TransferAmount", "TransferType", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555558"), "123456789", 1400000m, "SEP1008", "Payment for TRK1008", new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444448"), "REF1008", 1008L, "", new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), 350000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555559"), "123456789", 1800000m, "SEP1009", "Payment for TRK1009", new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444449"), "REF1009", 1009L, "", new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), 400000m, "IN", null },
                    { new Guid("55555555-5555-5555-5555-555555555560"), "123456789", 2250000m, "SEP1010", "Payment for TRK1010", new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test", "SePay", false, new Guid("44444444-4444-4444-4444-444444444450"), "REF1010", 1010L, "", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 450000m, "IN", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555558"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555559"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555560"));

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
        }
    }
}
