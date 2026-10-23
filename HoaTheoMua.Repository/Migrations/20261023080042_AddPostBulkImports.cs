using System;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPostBulkImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportedPostIdentity",
                columns: table => new
                {
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BasePostId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedPostIdentity", x => new { x.RequestedBy, x.Code });
                    table.ForeignKey(
                        name: "FK_ImportedPostIdentity_BasePost_BasePostId",
                        column: x => x.BasePostId,
                        principalTable: "BasePost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostImportAccount",
                columns: table => new
                {
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<PlatformType>(type: "\"PlatformType\"", nullable: false),
                    PlatformAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImportAccount", x => new { x.RequestedBy, x.Platform });
                    table.ForeignKey(
                        name: "FK_PostImportAccount_PlatformAccount_PlatformAccountId",
                        column: x => x.PlatformAccountId,
                        principalTable: "PlatformAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostImportBatch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Issues = table.Column<string>(type: "jsonb", nullable: false),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImportBatch", x => x.Id);
                    table.CheckConstraint("CK_PostImportBatch_Lease", "(\"LeaseId\" IS NULL) = (\"LeaseUntil\" IS NULL)");
                    table.CheckConstraint("CK_PostImportBatch_Status", "\"Status\" IN ('Queued','Processing','Processed')");
                });

            migrationBuilder.CreateTable(
                name: "PostImportRow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Issues = table.Column<string>(type: "jsonb", nullable: false),
                    BasePostId = table.Column<Guid>(type: "uuid", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImportRow", x => x.Id);
                    table.CheckConstraint("CK_PostImportRow_Range", "\"RowNumber\" >= 2 AND \"Attempts\" >= 0");
                    table.CheckConstraint("CK_PostImportRow_Status", "\"Status\" IN ('Queued','Processed','Invalid')");
                    table.ForeignKey(
                        name: "FK_PostImportRow_BasePost_BasePostId",
                        column: x => x.BasePostId,
                        principalTable: "BasePost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostImportRow_PostImportBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "PostImportBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostImportChannel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<PlatformType>(type: "\"PlatformType\"", nullable: false),
                    PlatformAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImportChannel", x => x.Id);
                    table.CheckConstraint("CK_PostImportChannel_Status", "\"Status\" IN ('Pending','Submitted','Existing','Rejected')");
                    table.ForeignKey(
                        name: "FK_PostImportChannel_PlatformAccount_PlatformAccountId",
                        column: x => x.PlatformAccountId,
                        principalTable: "PlatformAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostImportChannel_PostImportRow_RowId",
                        column: x => x.RowId,
                        principalTable: "PostImportRow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostImportChannel_PostPublication_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "PostPublication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedPostIdentity_BasePostId",
                table: "ImportedPostIdentity",
                column: "BasePostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostImportAccount_PlatformAccountId",
                table: "PostImportAccount",
                column: "PlatformAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PostImportBatch_RequestedBy_CreatedAt",
                table: "PostImportBatch",
                columns: new[] { "RequestedBy", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PostImportBatch_RequestedBy_RequestKey",
                table: "PostImportBatch",
                columns: new[] { "RequestedBy", "RequestKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostImportBatch_Status_LeaseUntil",
                table: "PostImportBatch",
                columns: new[] { "Status", "LeaseUntil" },
                filter: "\"Status\" <> 'Processed'");

            migrationBuilder.CreateIndex(
                name: "IX_PostImportChannel_PlatformAccountId",
                table: "PostImportChannel",
                column: "PlatformAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PostImportChannel_PublicationId",
                table: "PostImportChannel",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PostImportChannel_RowId_Platform",
                table: "PostImportChannel",
                columns: new[] { "RowId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostImportRow_BasePostId",
                table: "PostImportRow",
                column: "BasePostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostImportRow_BatchId_RowNumber",
                table: "PostImportRow",
                columns: new[] { "BatchId", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostImportRow_BatchId_Status_RowNumber",
                table: "PostImportRow",
                columns: new[] { "BatchId", "Status", "RowNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM "PostImportBatch") OR EXISTS (SELECT 1 FROM "ImportedPostIdentity") THEN
                        RAISE EXCEPTION 'Cannot remove bulk import history; retain the schema and roll forward.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "ImportedPostIdentity");

            migrationBuilder.DropTable(
                name: "PostImportAccount");

            migrationBuilder.DropTable(
                name: "PostImportChannel");

            migrationBuilder.DropTable(
                name: "PostImportRow");

            migrationBuilder.DropTable(
                name: "PostImportBatch");

        }
    }
}
