using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCardGenerationJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardGenerationJob",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Stage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OrderDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    VirtualId = table.Column<Guid>(type: "uuid", nullable: false),
                    BouquetImageUrl = table.Column<string>(type: "text", nullable: true),
                    TemplateImageUrl = table.Column<string>(type: "text", nullable: true),
                    SourceCardId = table.Column<Guid>(type: "uuid", nullable: true),
                    Snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    Checkpoint = table.Column<string>(type: "jsonb", nullable: false),
                    Result = table.Column<string>(type: "jsonb", nullable: true),
                    GeneratedCardId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastDispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardGenerationJob", x => x.Id);
                    table.CheckConstraint("CK_CardJob_Lease", "(\"LeaseId\" IS NULL) = (\"LeaseUntil\" IS NULL)");
                    table.CheckConstraint("CK_CardJob_Status", "\"Status\" IN ('Queued','Running','Succeeded','Failed','Canceled')");
                    table.CheckConstraint("CK_CardJob_Success", "\"Status\" <> 'Succeeded' OR (\"GeneratedCardId\" IS NOT NULL AND \"Result\" IS NOT NULL)");
                    table.CheckConstraint("CK_CardJob_Type", "\"Type\" IN ('Create','Regenerate')");
                    table.ForeignKey(
                        name: "FK_CardGenerationJob_GeneratedCard_GeneratedCardId",
                        column: x => x.GeneratedCardId,
                        principalTable: "GeneratedCard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardGenerationJob_GeneratedCardId",
                table: "CardGenerationJob",
                column: "GeneratedCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardGenerationJob_OrderDetailId_VirtualId",
                table: "CardGenerationJob",
                columns: new[] { "OrderDetailId", "VirtualId" },
                unique: true,
                filter: "\"Type\" = 'Create' AND \"Status\" IN ('Queued','Running')");

            migrationBuilder.CreateIndex(
                name: "IX_CardGenerationJob_Status_LeaseUntil_LastDispatchedAt",
                table: "CardGenerationJob",
                columns: new[] { "Status", "LeaseUntil", "LastDispatchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CardGenerationJob_UserId_CreatedAt_Id",
                table: "CardGenerationJob",
                columns: new[] { "UserId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CardGenerationJob_UserId_IdempotencyKey",
                table: "CardGenerationJob",
                columns: new[] { "UserId", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardGenerationJob");
        }
    }
}
