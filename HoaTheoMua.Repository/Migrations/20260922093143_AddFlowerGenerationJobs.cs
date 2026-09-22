using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowerGenerationJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowerGenerationJob",
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
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductImageUrl = table.Column<string>(type: "text", nullable: true),
                    MockupImageUrl = table.Column<string>(type: "text", nullable: true),
                    SourceFlowerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    Checkpoint = table.Column<string>(type: "jsonb", nullable: false),
                    Result = table.Column<string>(type: "jsonb", nullable: true),
                    GeneratedFlowerId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FlowerGenerationJob", x => x.Id);
                    table.CheckConstraint("CK_FlowerJob_Lease", "(\"LeaseId\" IS NULL) = (\"LeaseUntil\" IS NULL)");
                    table.CheckConstraint("CK_FlowerJob_Status", "\"Status\" IN ('Queued','Running','Succeeded','Failed','Canceled')");
                    table.CheckConstraint("CK_FlowerJob_Success", "\"Status\" <> 'Succeeded' OR (\"GeneratedFlowerId\" IS NOT NULL AND \"Result\" IS NOT NULL)");
                    table.CheckConstraint("CK_FlowerJob_Type", "\"Type\" IN ('Create','Regenerate')");
                    table.ForeignKey(
                        name: "FK_FlowerGenerationJob_GeneratedFlower_GeneratedFlowerId",
                        column: x => x.GeneratedFlowerId,
                        principalTable: "GeneratedFlower",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowerGenerationJob_GeneratedFlowerId",
                table: "FlowerGenerationJob",
                column: "GeneratedFlowerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerGenerationJob_Status_LeaseUntil_LastDispatchedAt",
                table: "FlowerGenerationJob",
                columns: new[] { "Status", "LeaseUntil", "LastDispatchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FlowerGenerationJob_UserId_CreatedAt_Id",
                table: "FlowerGenerationJob",
                columns: new[] { "UserId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_FlowerGenerationJob_UserId_IdempotencyKey",
                table: "FlowerGenerationJob",
                columns: new[] { "UserId", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowerGenerationJob");
        }
    }
}
