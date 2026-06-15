using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddMapIdIntoComapnyAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111112"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111113"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111114"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111115"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111116"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111117"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111118"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111119"));

            migrationBuilder.DeleteData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111120"));

            migrationBuilder.RenameColumn(
                name: "District",
                table: "CompanyAddresses",
                newName: "MapId");

            migrationBuilder.UpdateData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"),
                columns: new[] { "Latitude", "Longitude", "MapId" },
                values: new object[] { 10.7769m, 106.7009m, "111-1-1-1-1-1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MapId",
                table: "CompanyAddresses",
                newName: "District");

            migrationBuilder.UpdateData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"),
                columns: new[] { "District", "Latitude", "Longitude" },
                values: new object[] { "District 1", null, null });

            migrationBuilder.InsertData(
                table: "CompanyAddresses",
                columns: new[] { "Id", "AddressLine", "City", "Country", "CreatedAt", "District", "IsDeleted", "Latitude", "Longitude", "UpdatedAt", "Ward" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-111111111112"), "456 Elm St", "Ho Chi Minh City", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 2", false, null, null, null, "Ward 2" },
                    { new Guid("e1111111-1111-1111-1111-111111111113"), "789 Oak St", "Hanoi", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 3", false, null, null, null, "Ward 3" },
                    { new Guid("e1111111-1111-1111-1111-111111111114"), "321 Pine St", "Danang", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 4", false, null, null, null, "Ward 4" },
                    { new Guid("e1111111-1111-1111-1111-111111111115"), "654 Maple St", "Hue", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 5", false, null, null, null, "Ward 5" },
                    { new Guid("e1111111-1111-1111-1111-111111111116"), "987 Cedar St", "Can Tho", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 6", false, null, null, null, "Ward 6" },
                    { new Guid("e1111111-1111-1111-1111-111111111117"), "159 Birch St", "Hai Phong", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 7", false, null, null, null, "Ward 7" },
                    { new Guid("e1111111-1111-1111-1111-111111111118"), "753 Spruce St", "Nha Trang", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 8", false, null, null, null, "Ward 8" },
                    { new Guid("e1111111-1111-1111-1111-111111111119"), "852 Fir St", "Da Lat", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 9", false, null, null, null, "Ward 9" },
                    { new Guid("e1111111-1111-1111-1111-111111111120"), "951 Ash St", "Vung Tau", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 10", false, null, null, null, "Ward 10" }
                });
        }
    }
}
