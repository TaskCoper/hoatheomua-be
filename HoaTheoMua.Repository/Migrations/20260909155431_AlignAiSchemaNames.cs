using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AlignAiSchemaNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_post_histories_system_prompts_SystemPromptId",
                table: "post_histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mockup",
                table: "mockup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_system_prompts",
                table: "system_prompts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_post_histories",
                table: "post_histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_standards",
                table: "platform_standards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_generated_posts",
                table: "generated_posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_generated_flowers",
                table: "generated_flowers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_generated_cards",
                table: "generated_cards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_client_histories",
                table: "client_histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_card_templates",
                table: "card_templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_base_posts",
                table: "base_posts");

            migrationBuilder.RenameTable(
                name: "mockup",
                newName: "Mockup");

            migrationBuilder.RenameTable(
                name: "system_prompts",
                newName: "SystemPrompt");

            migrationBuilder.RenameTable(
                name: "post_histories",
                newName: "PostHistory");

            migrationBuilder.RenameTable(
                name: "platform_standards",
                newName: "PlatformStandard");

            migrationBuilder.RenameTable(
                name: "generated_posts",
                newName: "GeneratedPost");

            migrationBuilder.RenameTable(
                name: "generated_flowers",
                newName: "GeneratedFlower");

            migrationBuilder.RenameTable(
                name: "generated_cards",
                newName: "GeneratedCard");

            migrationBuilder.RenameTable(
                name: "client_histories",
                newName: "ClientHistory");

            migrationBuilder.RenameTable(
                name: "card_templates",
                newName: "CardTemplate");

            migrationBuilder.RenameTable(
                name: "base_posts",
                newName: "BasePost");

            migrationBuilder.RenameIndex(
                name: "IX_mockup_IsActive_CreatedAt",
                table: "Mockup",
                newName: "IX_Mockup_IsActive_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_system_prompts_Type",
                table: "SystemPrompt",
                newName: "IX_SystemPrompt_Type");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_SystemPromptId",
                table: "PostHistory",
                newName: "IX_PostHistory_SystemPromptId");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_OutputId",
                table: "PostHistory",
                newName: "IX_PostHistory_OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_BaseId_CreatedAt",
                table: "PostHistory",
                newName: "IX_PostHistory_BaseId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_platform_standards_Platform",
                table: "PlatformStandard",
                newName: "IX_PlatformStandard_Platform");

            migrationBuilder.RenameIndex(
                name: "IX_generated_posts_UserId_CreatedAt",
                table: "GeneratedPost",
                newName: "IX_GeneratedPost_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_generated_flowers_UserId_CreatedAt",
                table: "GeneratedFlower",
                newName: "IX_GeneratedFlower_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_generated_cards_UserId_CreatedAt",
                table: "GeneratedCard",
                newName: "IX_GeneratedCard_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_generated_cards_CardType_CreatedAt",
                table: "GeneratedCard",
                newName: "IX_GeneratedCard_CardType_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_UserId_Type_CreatedAt",
                table: "ClientHistory",
                newName: "IX_ClientHistory_UserId_Type_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_Type_OutputId",
                table: "ClientHistory",
                newName: "IX_ClientHistory_Type_OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_BaseId_Type_CreatedAt",
                table: "ClientHistory",
                newName: "IX_ClientHistory_BaseId_Type_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_card_templates_TemplateType_IsActive_CreatedAt",
                table: "CardTemplate",
                newName: "IX_CardTemplate_TemplateType_IsActive_CreatedAt");

            migrationBuilder.Sql("ALTER TYPE system_prompt_types RENAME TO \"SystemPromptType\";");
            migrationBuilder.Sql("ALTER TYPE platform_type RENAME TO \"PlatformType\";");
            migrationBuilder.Sql("ALTER TYPE form_type RENAME TO \"CardFormType\";");
            migrationBuilder.Sql("ALTER TYPE client_history_types RENAME TO \"ClientHistoryType\";");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mockup",
                table: "Mockup",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemPrompt",
                table: "SystemPrompt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostHistory",
                table: "PostHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlatformStandard",
                table: "PlatformStandard",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneratedPost",
                table: "GeneratedPost",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneratedFlower",
                table: "GeneratedFlower",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneratedCard",
                table: "GeneratedCard",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientHistory",
                table: "ClientHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardTemplate",
                table: "CardTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BasePost",
                table: "BasePost",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostHistory_SystemPrompt_SystemPromptId",
                table: "PostHistory",
                column: "SystemPromptId",
                principalTable: "SystemPrompt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostHistory_SystemPrompt_SystemPromptId",
                table: "PostHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mockup",
                table: "Mockup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemPrompt",
                table: "SystemPrompt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostHistory",
                table: "PostHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlatformStandard",
                table: "PlatformStandard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneratedPost",
                table: "GeneratedPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneratedFlower",
                table: "GeneratedFlower");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneratedCard",
                table: "GeneratedCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientHistory",
                table: "ClientHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardTemplate",
                table: "CardTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BasePost",
                table: "BasePost");

            migrationBuilder.RenameTable(
                name: "Mockup",
                newName: "mockup");

            migrationBuilder.RenameTable(
                name: "SystemPrompt",
                newName: "system_prompts");

            migrationBuilder.RenameTable(
                name: "PostHistory",
                newName: "post_histories");

            migrationBuilder.RenameTable(
                name: "PlatformStandard",
                newName: "platform_standards");

            migrationBuilder.RenameTable(
                name: "GeneratedPost",
                newName: "generated_posts");

            migrationBuilder.RenameTable(
                name: "GeneratedFlower",
                newName: "generated_flowers");

            migrationBuilder.RenameTable(
                name: "GeneratedCard",
                newName: "generated_cards");

            migrationBuilder.RenameTable(
                name: "ClientHistory",
                newName: "client_histories");

            migrationBuilder.RenameTable(
                name: "CardTemplate",
                newName: "card_templates");

            migrationBuilder.RenameTable(
                name: "BasePost",
                newName: "base_posts");

            migrationBuilder.RenameIndex(
                name: "IX_Mockup_IsActive_CreatedAt",
                table: "mockup",
                newName: "IX_mockup_IsActive_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_SystemPrompt_Type",
                table: "system_prompts",
                newName: "IX_system_prompts_Type");

            migrationBuilder.RenameIndex(
                name: "IX_PostHistory_SystemPromptId",
                table: "post_histories",
                newName: "IX_post_histories_SystemPromptId");

            migrationBuilder.RenameIndex(
                name: "IX_PostHistory_OutputId",
                table: "post_histories",
                newName: "IX_post_histories_OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_PostHistory_BaseId_CreatedAt",
                table: "post_histories",
                newName: "IX_post_histories_BaseId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_PlatformStandard_Platform",
                table: "platform_standards",
                newName: "IX_platform_standards_Platform");

            migrationBuilder.RenameIndex(
                name: "IX_GeneratedPost_UserId_CreatedAt",
                table: "generated_posts",
                newName: "IX_generated_posts_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_GeneratedFlower_UserId_CreatedAt",
                table: "generated_flowers",
                newName: "IX_generated_flowers_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_GeneratedCard_UserId_CreatedAt",
                table: "generated_cards",
                newName: "IX_generated_cards_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_GeneratedCard_CardType_CreatedAt",
                table: "generated_cards",
                newName: "IX_generated_cards_CardType_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ClientHistory_UserId_Type_CreatedAt",
                table: "client_histories",
                newName: "IX_client_histories_UserId_Type_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ClientHistory_Type_OutputId",
                table: "client_histories",
                newName: "IX_client_histories_Type_OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientHistory_BaseId_Type_CreatedAt",
                table: "client_histories",
                newName: "IX_client_histories_BaseId_Type_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CardTemplate_TemplateType_IsActive_CreatedAt",
                table: "card_templates",
                newName: "IX_card_templates_TemplateType_IsActive_CreatedAt");

            migrationBuilder.Sql("ALTER TYPE \"SystemPromptType\" RENAME TO system_prompt_types;");
            migrationBuilder.Sql("ALTER TYPE \"PlatformType\" RENAME TO platform_type;");
            migrationBuilder.Sql("ALTER TYPE \"CardFormType\" RENAME TO form_type;");
            migrationBuilder.Sql("ALTER TYPE \"ClientHistoryType\" RENAME TO client_history_types;");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mockup",
                table: "mockup",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_system_prompts",
                table: "system_prompts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_post_histories",
                table: "post_histories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_standards",
                table: "platform_standards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_generated_posts",
                table: "generated_posts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_generated_flowers",
                table: "generated_flowers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_generated_cards",
                table: "generated_cards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_client_histories",
                table: "client_histories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_card_templates",
                table: "card_templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_base_posts",
                table: "base_posts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_post_histories_system_prompts_SystemPromptId",
                table: "post_histories",
                column: "SystemPromptId",
                principalTable: "system_prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
