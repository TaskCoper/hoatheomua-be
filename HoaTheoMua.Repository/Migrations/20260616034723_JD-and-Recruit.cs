using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class JDandRecruit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobDescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    avatarUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    coverImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    minExperienceYears = table.Column<float>(type: "real", nullable: true),
                    maxExperienceYears = table.Column<float>(type: "real", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    employmentType = table.Column<int>(type: "integer", nullable: false),
                    salaryMin = table.Column<float>(type: "real", nullable: true),
                    salaryMax = table.Column<float>(type: "real", nullable: true),
                    salaryNegotiable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    openings = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecruitmentRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    coverLetter = table.Column<string>(type: "text", nullable: false),
                    attachments = table.Column<string>(type: "text", nullable: false),
                    isRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobDescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequest_JobDescriptions_JobDescriptionId",
                        column: x => x.JobDescriptionId,
                        principalTable: "JobDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequest_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptions_slug",
                table: "JobDescriptions",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequest_JobDescriptionId",
                table: "RecruitmentRequest",
                column: "JobDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequest_UserId",
                table: "RecruitmentRequest",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecruitmentRequest");

            migrationBuilder.DropTable(
                name: "JobDescriptions");
        }
    }
}
