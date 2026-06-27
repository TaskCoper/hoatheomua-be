using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_two_fields_in_CompanyAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "CompanyAddresses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CompanyAddresses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "CompanyAddresses",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"),
                columns: new[] { "AddressLine", "City", "FullName", "Latitude", "Longitude", "MapId", "PhoneNumber", "Remarks", "Ward" },
                values: new object[] { "Số 13 Đào Duy Anh, Phường 9", "Hồ Chí Minh", "Hoa Theo Mua", 10.806088849000048m, 106.67715580100008m, "h4gORrahdlpw0Z8t4761TmmZ0TqpdZTffrY5FJFeuZJptWwspnit5C7RQkC8Z-TfMbtCEaZch_dwpGdal162dna1U3ihlJsAc7dBVRZdnc_6XjVIBoQFyl3ClWiqXAOzX", "84908765432", "Đến nơi lấy hàng đọc mã đơn để nhận hàng", "Phường Sài Gòn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "CompanyAddresses");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CompanyAddresses");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "CompanyAddresses");

            migrationBuilder.UpdateData(
                table: "CompanyAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"),
                columns: new[] { "AddressLine", "City", "Latitude", "Longitude", "MapId", "Ward" },
                values: new object[] { "123 Main St", "Ho Chi Minh City", 10.7769m, 106.7009m, "111-1-1-1-1-1", "Ward 1" });
        }
    }
}
