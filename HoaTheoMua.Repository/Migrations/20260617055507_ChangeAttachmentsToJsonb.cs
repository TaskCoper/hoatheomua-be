using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAttachmentsToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"RecruitmentRequest\" ALTER COLUMN \"attachments\" TYPE jsonb USING \"attachments\"::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"RecruitmentRequest\" ALTER COLUMN \"attachments\" TYPE text USING \"attachments\"::text;");
        }
    }
}
