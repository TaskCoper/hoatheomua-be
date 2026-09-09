using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AlignAiFieldsAndClientHistoryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_post_histories_system_prompts_system_prompt_id",
                table: "post_histories");

            migrationBuilder.DropIndex(
                name: "IX_mockup_is_active_created_at",
                table: "mockup");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_ai_images",
                table: "generated_cards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_card_type",
                table: "generated_cards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_handmade",
                table: "generated_cards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_non_negative_values",
                table: "generated_cards");

            migrationBuilder.DropIndex(
                name: "IX_card_templates_template_type_is_active_created_at",
                table: "card_templates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_card_templates_template_type",
                table: "card_templates");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "system_prompts",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "system_prompts",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "system_prompts",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_system_prompts_type",
                table: "system_prompts",
                newName: "IX_system_prompts_Type");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "post_histories",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "input",
                table: "post_histories",
                newName: "Input");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "post_histories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "system_prompt_id",
                table: "post_histories",
                newName: "SystemPromptId");

            migrationBuilder.RenameColumn(
                name: "output_id",
                table: "post_histories",
                newName: "OutputId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "post_histories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "base_id",
                table: "post_histories",
                newName: "BaseId");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_system_prompt_id",
                table: "post_histories",
                newName: "IX_post_histories_SystemPromptId");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_output_id",
                table: "post_histories",
                newName: "IX_post_histories_OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_base_id_created_at",
                table: "post_histories",
                newName: "IX_post_histories_BaseId_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "platform",
                table: "platform_standards",
                newName: "Platform");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "platform_standards",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "platform_standards",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_standards_platform",
                table: "platform_standards",
                newName: "IX_platform_standards_Platform");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "mockup",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "mockup",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "mockup",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "mockup",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "mockup",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "mockup",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "mockup",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "mockup",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "generated_posts",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "generated_posts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "generated_posts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "generated_posts",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "generated_posts",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_generated_posts_user_id_created_at",
                table: "generated_posts",
                newName: "IX_generated_posts_UserId_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "generated_flowers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "generated_flowers",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "generated_flowers",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "generated_flowers",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_generated_flowers_user_id_created_at",
                table: "generated_flowers",
                newName: "IX_generated_flowers_UserId_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "generated_cards",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "generated_cards",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "word_count",
                table: "generated_cards",
                newName: "WordCount");

            migrationBuilder.RenameColumn(
                name: "word_config_snapshot",
                table: "generated_cards",
                newName: "WordConfigSnapshot");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "generated_cards",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "total_price",
                table: "generated_cards",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "size_width",
                table: "generated_cards",
                newName: "SizeWidth");

            migrationBuilder.RenameColumn(
                name: "size_name",
                table: "generated_cards",
                newName: "SizeName");

            migrationBuilder.RenameColumn(
                name: "size_max_words",
                table: "generated_cards",
                newName: "SizeMaxWords");

            migrationBuilder.RenameColumn(
                name: "size_height",
                table: "generated_cards",
                newName: "SizeHeight");

            migrationBuilder.RenameColumn(
                name: "size_base_price",
                table: "generated_cards",
                newName: "SizeBasePrice");

            migrationBuilder.RenameColumn(
                name: "sender_name",
                table: "generated_cards",
                newName: "SenderName");

            migrationBuilder.RenameColumn(
                name: "receiver_name",
                table: "generated_cards",
                newName: "ReceiverName");

            migrationBuilder.RenameColumn(
                name: "raw_image",
                table: "generated_cards",
                newName: "RawImage");

            migrationBuilder.RenameColumn(
                name: "message_content",
                table: "generated_cards",
                newName: "MessageContent");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "generated_cards",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "form_type",
                table: "generated_cards",
                newName: "FormType");

            migrationBuilder.RenameColumn(
                name: "extra_price",
                table: "generated_cards",
                newName: "ExtraPrice");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "generated_cards",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "card_type",
                table: "generated_cards",
                newName: "CardType");

            migrationBuilder.RenameColumn(
                name: "base_price",
                table: "generated_cards",
                newName: "BasePrice");

            migrationBuilder.RenameColumn(
                name: "attached_image_url",
                table: "generated_cards",
                newName: "AttachedImageUrl");

            migrationBuilder.RenameIndex(
                name: "IX_generated_cards_user_id_created_at",
                table: "generated_cards",
                newName: "IX_generated_cards_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_generated_cards_card_type_created_at",
                table: "generated_cards",
                newName: "IX_generated_cards_CardType_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "client_histories",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "client_histories",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "client_histories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "client_histories",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "system_form",
                table: "client_histories",
                newName: "SystemForm");

            migrationBuilder.RenameColumn(
                name: "output_id",
                table: "client_histories",
                newName: "OutputId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "client_histories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "base_id",
                table: "client_histories",
                newName: "BaseId");

            migrationBuilder.RenameColumn(
                name: "root",
                table: "client_histories",
                newName: "RootID");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_user_id_type_created_at",
                table: "client_histories",
                newName: "IX_client_histories_UserId_Type_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_type_output_id",
                table: "client_histories",
                newName: "IX_client_histories_Type_OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_base_id_type_created_at",
                table: "client_histories",
                newName: "IX_client_histories_BaseId_Type_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "card_templates",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "card_templates",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "card_templates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "card_templates",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "template_type",
                table: "card_templates",
                newName: "TemplateType");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "card_templates",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "card_templates",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "card_templates",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "card_templates",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "base_posts",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "base_posts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "base_posts",
                newName: "ImageUrl");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:client_history_types", "flower,card,handmade_card,post")
                .Annotation("Npgsql:Enum:form_type", "go_may,calligraphy")
                .Annotation("Npgsql:Enum:platform_type", "zalo,facebook,instagram")
                .Annotation("Npgsql:Enum:system_prompt_types", "flower,card,post")
                .OldAnnotation("Npgsql:Enum:form_type", "go_may,calligraphy")
                .OldAnnotation("Npgsql:Enum:platform_type", "zalo,facebook,instagram")
                .OldAnnotation("Npgsql:Enum:system_prompt_types", "flower,card,post");

            migrationBuilder.Sql(
                """
                ALTER TABLE client_histories
                ALTER COLUMN "Type" TYPE client_history_types
                USING "Type"::text::client_history_types;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_mockup_IsActive_CreatedAt",
                table: "mockup",
                columns: new[] { "IsActive", "CreatedAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_ai_images",
                table: "generated_cards",
                sql: "\"CardType\" <> 'ai' OR (\"RawImage\" IS NOT NULL AND \"ImageUrl\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_card_type",
                table: "generated_cards",
                sql: "\"CardType\" IN ('ai', 'handmade')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_handmade",
                table: "generated_cards",
                sql: "\"CardType\" <> 'handmade' OR (\"FormType\" = 'calligraphy' AND \"AttachedImageUrl\" IS NULL AND \"RawImage\" IS NULL AND \"BasePrice\" = 0 AND \"TotalPrice\" = \"ExtraPrice\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_non_negative_values",
                table: "generated_cards",
                sql: "\"SizeWidth\" >= 0 AND \"SizeHeight\" >= 0 AND \"SizeBasePrice\" >= 0 AND \"SizeMaxWords\" >= 0 AND \"WordCount\" >= 0 AND \"BasePrice\" >= 0 AND \"ExtraPrice\" >= 0 AND \"TotalPrice\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_card_templates_TemplateType_IsActive_CreatedAt",
                table: "card_templates",
                columns: new[] { "TemplateType", "IsActive", "CreatedAt" },
                descending: new[] { false, false, true },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_card_templates_template_type",
                table: "card_templates",
                sql: "\"TemplateType\" IN ('ai', 'handmade')");

            migrationBuilder.AddForeignKey(
                name: "FK_post_histories_system_prompts_SystemPromptId",
                table: "post_histories",
                column: "SystemPromptId",
                principalTable: "system_prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_post_histories_system_prompts_SystemPromptId",
                table: "post_histories");

            migrationBuilder.DropIndex(
                name: "IX_mockup_IsActive_CreatedAt",
                table: "mockup");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_ai_images",
                table: "generated_cards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_card_type",
                table: "generated_cards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_handmade",
                table: "generated_cards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_generated_cards_non_negative_values",
                table: "generated_cards");

            migrationBuilder.DropIndex(
                name: "IX_card_templates_TemplateType_IsActive_CreatedAt",
                table: "card_templates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_card_templates_template_type",
                table: "card_templates");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "system_prompts",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "system_prompts",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "system_prompts",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_system_prompts_Type",
                table: "system_prompts",
                newName: "IX_system_prompts_type");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "post_histories",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Input",
                table: "post_histories",
                newName: "input");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "post_histories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SystemPromptId",
                table: "post_histories",
                newName: "system_prompt_id");

            migrationBuilder.RenameColumn(
                name: "OutputId",
                table: "post_histories",
                newName: "output_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "post_histories",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BaseId",
                table: "post_histories",
                newName: "base_id");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_SystemPromptId",
                table: "post_histories",
                newName: "IX_post_histories_system_prompt_id");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_OutputId",
                table: "post_histories",
                newName: "IX_post_histories_output_id");

            migrationBuilder.RenameIndex(
                name: "IX_post_histories_BaseId_CreatedAt",
                table: "post_histories",
                newName: "IX_post_histories_base_id_created_at");

            migrationBuilder.RenameColumn(
                name: "Platform",
                table: "platform_standards",
                newName: "platform");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "platform_standards",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "platform_standards",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_standards_Platform",
                table: "platform_standards",
                newName: "IX_platform_standards_platform");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "mockup",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "mockup",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "mockup",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "mockup",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "mockup",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "mockup",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "mockup",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "mockup",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "generated_posts",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "generated_posts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "generated_posts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "generated_posts",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "generated_posts",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_generated_posts_UserId_CreatedAt",
                table: "generated_posts",
                newName: "IX_generated_posts_user_id_created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "generated_flowers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "generated_flowers",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "generated_flowers",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "generated_flowers",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_generated_flowers_UserId_CreatedAt",
                table: "generated_flowers",
                newName: "IX_generated_flowers_user_id_created_at");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "generated_cards",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "generated_cards",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WordCount",
                table: "generated_cards",
                newName: "word_count");

            migrationBuilder.RenameColumn(
                name: "WordConfigSnapshot",
                table: "generated_cards",
                newName: "word_config_snapshot");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "generated_cards",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "generated_cards",
                newName: "total_price");

            migrationBuilder.RenameColumn(
                name: "SizeWidth",
                table: "generated_cards",
                newName: "size_width");

            migrationBuilder.RenameColumn(
                name: "SizeName",
                table: "generated_cards",
                newName: "size_name");

            migrationBuilder.RenameColumn(
                name: "SizeMaxWords",
                table: "generated_cards",
                newName: "size_max_words");

            migrationBuilder.RenameColumn(
                name: "SizeHeight",
                table: "generated_cards",
                newName: "size_height");

            migrationBuilder.RenameColumn(
                name: "SizeBasePrice",
                table: "generated_cards",
                newName: "size_base_price");

            migrationBuilder.RenameColumn(
                name: "SenderName",
                table: "generated_cards",
                newName: "sender_name");

            migrationBuilder.RenameColumn(
                name: "ReceiverName",
                table: "generated_cards",
                newName: "receiver_name");

            migrationBuilder.RenameColumn(
                name: "RawImage",
                table: "generated_cards",
                newName: "raw_image");

            migrationBuilder.RenameColumn(
                name: "MessageContent",
                table: "generated_cards",
                newName: "message_content");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "generated_cards",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "FormType",
                table: "generated_cards",
                newName: "form_type");

            migrationBuilder.RenameColumn(
                name: "ExtraPrice",
                table: "generated_cards",
                newName: "extra_price");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "generated_cards",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CardType",
                table: "generated_cards",
                newName: "card_type");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "generated_cards",
                newName: "base_price");

            migrationBuilder.RenameColumn(
                name: "AttachedImageUrl",
                table: "generated_cards",
                newName: "attached_image_url");

            migrationBuilder.RenameIndex(
                name: "IX_generated_cards_UserId_CreatedAt",
                table: "generated_cards",
                newName: "IX_generated_cards_user_id_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_generated_cards_CardType_CreatedAt",
                table: "generated_cards",
                newName: "IX_generated_cards_card_type_created_at");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "client_histories",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "client_histories",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "client_histories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "client_histories",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "SystemForm",
                table: "client_histories",
                newName: "system_form");

            migrationBuilder.RenameColumn(
                name: "OutputId",
                table: "client_histories",
                newName: "output_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "client_histories",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BaseId",
                table: "client_histories",
                newName: "base_id");

            migrationBuilder.RenameColumn(
                name: "RootID",
                table: "client_histories",
                newName: "root");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_UserId_Type_CreatedAt",
                table: "client_histories",
                newName: "IX_client_histories_user_id_type_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_Type_OutputId",
                table: "client_histories",
                newName: "IX_client_histories_type_output_id");

            migrationBuilder.RenameIndex(
                name: "IX_client_histories_BaseId_Type_CreatedAt",
                table: "client_histories",
                newName: "IX_client_histories_base_id_type_created_at");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "card_templates",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "card_templates",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "card_templates",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "card_templates",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TemplateType",
                table: "card_templates",
                newName: "template_type");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "card_templates",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "card_templates",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "card_templates",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "card_templates",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "base_posts",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "base_posts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "base_posts",
                newName: "image_url");

            migrationBuilder.Sql(
                """
                ALTER TABLE client_histories
                ALTER COLUMN "type" TYPE character varying(50)
                USING "type"::text;
                """);

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:form_type", "go_may,calligraphy")
                .Annotation("Npgsql:Enum:platform_type", "zalo,facebook,instagram")
                .Annotation("Npgsql:Enum:system_prompt_types", "flower,card,post")
                .OldAnnotation("Npgsql:Enum:client_history_types", "flower,card,handmade_card,post")
                .OldAnnotation("Npgsql:Enum:form_type", "go_may,calligraphy")
                .OldAnnotation("Npgsql:Enum:platform_type", "zalo,facebook,instagram")
                .OldAnnotation("Npgsql:Enum:system_prompt_types", "flower,card,post");

            migrationBuilder.CreateIndex(
                name: "IX_mockup_is_active_created_at",
                table: "mockup",
                columns: new[] { "is_active", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_ai_images",
                table: "generated_cards",
                sql: "card_type <> 'ai' OR (raw_image IS NOT NULL AND image_url IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_card_type",
                table: "generated_cards",
                sql: "card_type IN ('ai', 'handmade')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_handmade",
                table: "generated_cards",
                sql: "card_type <> 'handmade' OR (form_type = 'calligraphy' AND attached_image_url IS NULL AND raw_image IS NULL AND base_price = 0 AND total_price = extra_price)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_generated_cards_non_negative_values",
                table: "generated_cards",
                sql: "size_width >= 0 AND size_height >= 0 AND size_base_price >= 0 AND size_max_words >= 0 AND word_count >= 0 AND base_price >= 0 AND extra_price >= 0 AND total_price >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_card_templates_template_type_is_active_created_at",
                table: "card_templates",
                columns: new[] { "template_type", "is_active", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_card_templates_template_type",
                table: "card_templates",
                sql: "template_type IN ('ai', 'handmade')");

            migrationBuilder.AddForeignKey(
                name: "FK_post_histories_system_prompts_system_prompt_id",
                table: "post_histories",
                column: "system_prompt_id",
                principalTable: "system_prompts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
