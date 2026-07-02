using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class add_fields_TagIds_Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<long>>(
                name: "TagIds",
                table: "Orders",
                type: "bigint[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagIds",
                table: "Orders");
        }
    }
}
