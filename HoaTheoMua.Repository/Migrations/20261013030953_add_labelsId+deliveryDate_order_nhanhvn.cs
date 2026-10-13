using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_labelsIddeliveryDate_order_nhanhvn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DeliveryDate",
                table: "OrderNhanhVns",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<List<long>>(
                name: "TagIds",
                table: "OrderNhanhVns",
                type: "bigint[]",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "Status", "CreatedAt" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_OrderTime",
                table: "Orders",
                columns: new[] { "Status", "OrderTime" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNhanhVns_DeliveryDate",
                table: "OrderNhanhVns",
                column: "DeliveryDate",
                filter: "\"IsDeleted\" = false\r\nAND (\"TrangThaiDonHang\" IS NULL\r\n     OR \"TrangThaiDonHang\" NOT IN ('63', 'Khách hủy'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_OrderTime",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderNhanhVns_DeliveryDate",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "OrderNhanhVns");

            migrationBuilder.DropColumn(
                name: "TagIds",
                table: "OrderNhanhVns");
        }
    }
}
