using System;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPostGenCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy post tables were unused when PostGen was designed. Refuse to discard
            // unexpected data; a deployment with rows needs a deliberate backfill first.
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM "BasePost") OR EXISTS (SELECT 1 FROM "GeneratedPost") THEN
                        RAISE EXCEPTION 'PostGen migration requires empty legacy BasePost/GeneratedPost tables; backfill existing data first';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_PlatformStandard_Platform",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "BasePost");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PlatformStandard",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PlatformStandard",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PlatformStandard",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxContentLength",
                table: "PlatformStandard",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxHashtagCount",
                table: "PlatformStandard",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxImageCount",
                table: "PlatformStandard",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PlatformStandard",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiModel",
                table: "GeneratedPost",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiRequestId",
                table: "GeneratedPost",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMs",
                table: "GeneratedPost",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "GeneratedPost",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackInput",
                table: "GeneratedPost",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hashtag",
                table: "GeneratedPost",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentGeneratedPostId",
                table: "GeneratedPost",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformPostId",
                table: "GeneratedPost",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PromptSnapshot",
                table: "GeneratedPost",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "GeneratedPost",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ai");

            migrationBuilder.AddColumn<Guid>(
                name: "SystemPromptId",
                table: "GeneratedPost",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "GeneratedPost",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "BasePost",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "Hashtag",
                table: "BasePost",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BasePost",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "BasePost",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "BasePost",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BasePostImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BasePostId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePostImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasePostImage_BasePost_BasePostId",
                        column: x => x.BasePostId,
                        principalTable: "BasePost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformPost",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BasePostId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<PlatformType>(type: "\"PlatformType\"", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OfficialGenerationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlatformAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalPostId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PublishError = table.Column<string>(type: "text", nullable: true),
                    PublishAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPost", x => x.Id);
                    table.CheckConstraint("CK_PlatformPost_Status", "\"Status\" IN ('Draft', 'Generating', 'Generated', 'Failed', 'Ready', 'Scheduled', 'Publishing', 'Published', 'PublishFailed')");
                    table.ForeignKey(
                        name: "FK_PlatformPost_BasePost_BasePostId",
                        column: x => x.BasePostId,
                        principalTable: "BasePost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformPost_GeneratedPost_OfficialGenerationId",
                        column: x => x.OfficialGenerationId,
                        principalTable: "GeneratedPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedPostImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BasePostId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBasePostImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RatioCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ParentGeneratedPostImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PixelWidth = table.Column<int>(type: "integer", nullable: false),
                    PixelHeight = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ImageExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StorageProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FeedbackInput = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    AiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AiRequestId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedPostImage", x => x.Id);
                    table.CheckConstraint("CK_GeneratedPostImage_RatioCode", "\"RatioCode\" IN ('Square1x1', 'Portrait4x5', 'Portrait3x4', 'Story9x16', 'Landscape16x9')");
                    table.CheckConstraint("CK_GeneratedPostImage_Status", "\"Status\" IN ('Succeeded', 'Failed')");
                    table.CheckConstraint("CK_GeneratedPostImage_Succeeded", "\"Status\" <> 'Succeeded' OR \"ImageUrl\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_GeneratedPostImage_BasePostImage_SourceBasePostImageId",
                        column: x => x.SourceBasePostImageId,
                        principalTable: "BasePostImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneratedPostImage_BasePost_BasePostId",
                        column: x => x.BasePostId,
                        principalTable: "BasePost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneratedPostImage_GeneratedPostImage_ParentGeneratedPostIm~",
                        column: x => x.ParentGeneratedPostImageId,
                        principalTable: "GeneratedPostImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformPostImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBasePostImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RatioCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SelectedGeneratedPostImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPostImage", x => x.Id);
                    table.CheckConstraint("CK_PlatformPostImage_RatioCode", "\"RatioCode\" IN ('Square1x1', 'Portrait4x5', 'Portrait3x4', 'Story9x16', 'Landscape16x9')");
                    table.ForeignKey(
                        name: "FK_PlatformPostImage_BasePostImage_SourceBasePostImageId",
                        column: x => x.SourceBasePostImageId,
                        principalTable: "BasePostImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformPostImage_GeneratedPostImage_SelectedGeneratedPostI~",
                        column: x => x.SelectedGeneratedPostImageId,
                        principalTable: "GeneratedPostImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformPostImage_PlatformPost_PlatformPostId",
                        column: x => x.PlatformPostId,
                        principalTable: "PlatformPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformStandard_Platform",
                table: "PlatformStandard",
                column: "Platform",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedPost_ParentGeneratedPostId",
                table: "GeneratedPost",
                column: "ParentGeneratedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedPost_PlatformPostId_CreatedAt_Id",
                table: "GeneratedPost",
                columns: new[] { "PlatformPostId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedPost_SystemPromptId",
                table: "GeneratedPost",
                column: "SystemPromptId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GeneratedPost_Source",
                table: "GeneratedPost",
                sql: "\"Source\" IN ('Ai', 'Manual')");

            migrationBuilder.CreateIndex(
                name: "IX_Configs_PostGenRatioPreset",
                table: "Configs",
                columns: new[] { "Group", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Group\" = 'postgen' AND \"Key\" = 'postgen.ratio-preset'");

            migrationBuilder.CreateIndex(
                name: "IX_BasePost_CreatedAt",
                table: "BasePost",
                column: "CreatedAt",
                descending: new[] { true },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BasePostImage_BasePostId_SortOrder",
                table: "BasePostImage",
                columns: new[] { "BasePostId", "SortOrder" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedPostImage_BasePostId_CreatedAt",
                table: "GeneratedPostImage",
                columns: new[] { "BasePostId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedPostImage_ParentGeneratedPostImageId",
                table: "GeneratedPostImage",
                column: "ParentGeneratedPostImageId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedPostImage_SourceBasePostImageId_RatioCode_CreatedA~",
                table: "GeneratedPostImage",
                columns: new[] { "SourceBasePostImageId", "RatioCode", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPost_BasePostId_Platform",
                table: "PlatformPost",
                columns: new[] { "BasePostId", "Platform" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPost_OfficialGenerationId",
                table: "PlatformPost",
                column: "OfficialGenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPost_Status_ScheduledAt",
                table: "PlatformPost",
                columns: new[] { "Status", "ScheduledAt" },
                filter: "\"Status\" = 'Scheduled'");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPostImage_PlatformPostId_SortOrder",
                table: "PlatformPostImage",
                columns: new[] { "PlatformPostId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPostImage_PlatformPostId_SourceBasePostImageId_Rati~",
                table: "PlatformPostImage",
                columns: new[] { "PlatformPostId", "SourceBasePostImageId", "RatioCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPostImage_SelectedGeneratedPostImageId",
                table: "PlatformPostImage",
                column: "SelectedGeneratedPostImageId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPostImage_SourceBasePostImageId",
                table: "PlatformPostImage",
                column: "SourceBasePostImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedPost_GeneratedPost_ParentGeneratedPostId",
                table: "GeneratedPost",
                column: "ParentGeneratedPostId",
                principalTable: "GeneratedPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedPost_PlatformPost_PlatformPostId",
                table: "GeneratedPost",
                column: "PlatformPostId",
                principalTable: "PlatformPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedPost_SystemPrompt_SystemPromptId",
                table: "GeneratedPost",
                column: "SystemPromptId",
                principalTable: "SystemPrompt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Keep the Post prompt if an admin already configured it through SystemPrompt APIs.
            migrationBuilder.Sql("""
                INSERT INTO "SystemPrompt" ("Id", "Type", "Content", "IsDeleted", "CreatedAt")
                VALUES ('e9b6e350-39e6-49d6-a100-000000000001', 'post',
                    'Viết lại bài đăng marketing bằng tiếng Việt dựa trên nội dung nguồn. Giữ đúng thông tin sản phẩm, không tự đặt giá, ưu đãi hoặc cam kết. Trả riêng title, content và hashtag; tuân theo hướng dẫn của nền tảng.', false, now())
                ON CONFLICT ("Type") DO NOTHING;
                """);
            var seededAt = new DateTimeOffset(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);
            migrationBuilder.InsertData(
                table: "PlatformStandard",
                columns: new[] { "Id", "Platform", "Description", "MaxContentLength", "MaxHashtagCount", "MaxImageCount", "IsActive", "IsDeleted", "CreatedAt" },
                columnTypes: new[] { "uuid", "\"PlatformType\"", "text", "integer", "integer", "integer", "boolean", "boolean", "timestamp with time zone" },
                values: new object[,]
                {
                    { new Guid("e9b6e350-39e6-49d6-a200-000000000001"), PlatformType.Facebook, "Bài Facebook Page: giọng tự nhiên, dễ đọc, nhấn mạnh sản phẩm và lời kêu gọi hành động rõ ràng.", 63206, 30, 80, true, false, seededAt },
                    { new Guid("e9b6e350-39e6-49d6-a200-000000000002"), PlatformType.Instagram, "Bài Instagram: caption ngắn gọn, giàu cảm xúc, hashtag liên quan đến sản phẩm và nội dung.", 2200, 30, 10, true, false, seededAt },
                    { new Guid("e9b6e350-39e6-49d6-a200-000000000003"), PlatformType.Zalo, "Bài Zalo OA: thông tin súc tích, thân thiện, ưu tiên giá trị sản phẩm và hướng dẫn liên hệ.", 2000, 10, 9, true, false, seededAt }
                });
            migrationBuilder.InsertData(
                table: "Configs",
                columns: new[] { "Id", "Group", "Key", "Value", "Kind", "UserId", "IsPublic", "IsDeleted", "CreatedAt" },
                values: new object[,]
                {
                    { new Guid("e9b6e350-39e6-49d6-a300-000000000001"), "postgen", "postgen.enabled", "true", "Setting", Guid.Empty, false, false, seededAt },
                    { new Guid("e9b6e350-39e6-49d6-a300-000000000002"), "postgen", "postgen.image_instruction", "\"Điều chỉnh bố cục hoặc mở rộng nền theo tỷ lệ đích. Giữ nguyên sản phẩm, chữ nhận diện thương hiệu và logo của ảnh gốc.\"", "Setting", Guid.Empty, false, false, seededAt }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedPost_GeneratedPost_ParentGeneratedPostId",
                table: "GeneratedPost");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedPost_PlatformPost_PlatformPostId",
                table: "GeneratedPost");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedPost_SystemPrompt_SystemPromptId",
                table: "GeneratedPost");

            migrationBuilder.DropTable(
                name: "PlatformPostImage");

            migrationBuilder.DropTable(
                name: "GeneratedPostImage");

            migrationBuilder.DropTable(
                name: "PlatformPost");

            migrationBuilder.DropTable(
                name: "BasePostImage");

            migrationBuilder.DropIndex(
                name: "IX_PlatformStandard_Platform",
                table: "PlatformStandard");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedPost_ParentGeneratedPostId",
                table: "GeneratedPost");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedPost_PlatformPostId_CreatedAt_Id",
                table: "GeneratedPost");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedPost_SystemPromptId",
                table: "GeneratedPost");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GeneratedPost_Source",
                table: "GeneratedPost");

            migrationBuilder.DropIndex(
                name: "IX_Configs_PostGenRatioPreset",
                table: "Configs");

            migrationBuilder.DropIndex(
                name: "IX_BasePost_CreatedAt",
                table: "BasePost");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "MaxContentLength",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "MaxHashtagCount",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "MaxImageCount",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PlatformStandard");

            migrationBuilder.DropColumn(
                name: "AiModel",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "AiRequestId",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "FeedbackInput",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "Hashtag",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "ParentGeneratedPostId",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "PlatformPostId",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "PromptSnapshot",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "SystemPromptId",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "GeneratedPost");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BasePost");

            migrationBuilder.DropColumn(
                name: "Hashtag",
                table: "BasePost");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BasePost");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "BasePost");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BasePost");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "GeneratedPost",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "BasePost",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformStandard_Platform",
                table: "PlatformStandard",
                column: "Platform");

            migrationBuilder.DeleteData("PlatformStandard", "Id", new Guid("e9b6e350-39e6-49d6-a200-000000000001"));
            migrationBuilder.DeleteData("PlatformStandard", "Id", new Guid("e9b6e350-39e6-49d6-a200-000000000002"));
            migrationBuilder.DeleteData("PlatformStandard", "Id", new Guid("e9b6e350-39e6-49d6-a200-000000000003"));
            migrationBuilder.DeleteData("Configs", "Id", new Guid("e9b6e350-39e6-49d6-a300-000000000001"));
            migrationBuilder.DeleteData("Configs", "Id", new Guid("e9b6e350-39e6-49d6-a300-000000000002"));
            migrationBuilder.DeleteData("SystemPrompt", "Id", new Guid("e9b6e350-39e6-49d6-a100-000000000001"));
        }
    }
}
