using System;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAiCustomizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:form_type", "go_may,calligraphy")
                .Annotation("Npgsql:Enum:platform_type", "zalo,facebook,instagram")
                .Annotation("Npgsql:Enum:system_prompt_types", "flower,card,post");

            migrationBuilder.CreateTable(
                name: "base_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_base_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "card_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    template_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_templates", x => x.id);
                    table.CheckConstraint("CK_card_templates_template_type", "template_type IN ('ai', 'handmade')");
                });

            migrationBuilder.CreateTable(
                name: "generated_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    raw_image = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    card_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    form_type = table.Column<CardFormType>(type: "form_type", nullable: false),
                    size_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    message_content = table.Column<string>(type: "text", nullable: false),
                    attached_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    size_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size_width = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    size_height = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    size_base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    size_max_words = table.Column<int>(type: "integer", nullable: false),
                    word_count = table.Column<int>(type: "integer", nullable: false),
                    word_config_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    extra_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_cards", x => x.id);
                    table.CheckConstraint("CK_generated_cards_ai_images", "card_type <> 'ai' OR (raw_image IS NOT NULL AND image_url IS NOT NULL)");
                    table.CheckConstraint("CK_generated_cards_card_type", "card_type IN ('ai', 'handmade')");
                    table.CheckConstraint("CK_generated_cards_handmade", "card_type <> 'handmade' OR (form_type = 'calligraphy' AND attached_image_url IS NULL AND raw_image IS NULL AND base_price = 0 AND total_price = extra_price)");
                    table.CheckConstraint("CK_generated_cards_non_negative_values", "size_width >= 0 AND size_height >= 0 AND size_base_price >= 0 AND size_max_words >= 0 AND word_count >= 0 AND base_price >= 0 AND extra_price >= 0 AND total_price >= 0");
                });

            migrationBuilder.CreateTable(
                name: "generated_flowers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    input_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_flowers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "generated_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mockup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mockup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "platform_standards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform = table.Column<PlatformType>(type: "platform_type", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_standards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_prompts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<SystemPromptType>(type: "system_prompt_types", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_prompts", x => x.id);
                });

            migrationBuilder.Sql(
                """
                CREATE FUNCTION prevent_card_template_type_change()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    IF NEW.template_type IS DISTINCT FROM OLD.template_type THEN
                        RAISE EXCEPTION 'card_templates.template_type is immutable'
                            USING ERRCODE = '23514';
                    END IF;

                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER TR_card_templates_template_type_immutable
                BEFORE UPDATE OF template_type ON card_templates
                FOR EACH ROW
                EXECUTE FUNCTION prevent_card_template_type_change();
                """);

            migrationBuilder.CreateTable(
                name: "client_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    input = table.Column<string>(type: "jsonb", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    base_id = table.Column<Guid>(type: "uuid", nullable: false),
                    output_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_prompt_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_histories_system_prompts_system_prompt_id",
                        column: x => x.system_prompt_id,
                        principalTable: "system_prompts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "post_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    input = table.Column<string>(type: "jsonb", nullable: false),
                    base_id = table.Column<Guid>(type: "uuid", nullable: false),
                    output_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_prompt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_post_histories_system_prompts_system_prompt_id",
                        column: x => x.system_prompt_id,
                        principalTable: "system_prompts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configs_Group_Key",
                table: "Configs",
                columns: new[] { "Group", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsPublic\" = true AND \"Kind\" = 'Setting' AND \"Group\" IN ('card_size', 'card_config')");

            migrationBuilder.CreateIndex(
                name: "IX_card_templates_template_type_is_active_created_at",
                table: "card_templates",
                columns: new[] { "template_type", "is_active", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_client_histories_base_id_type_created_at",
                table: "client_histories",
                columns: new[] { "base_id", "type", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_client_histories_system_prompt_id",
                table: "client_histories",
                column: "system_prompt_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_histories_type_output_id",
                table: "client_histories",
                columns: new[] { "type", "output_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_histories_user_id_type_created_at",
                table: "client_histories",
                columns: new[] { "user_id", "type", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_generated_cards_card_type_created_at",
                table: "generated_cards",
                columns: new[] { "card_type", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_generated_cards_user_id_created_at",
                table: "generated_cards",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_generated_flowers_user_id_created_at",
                table: "generated_flowers",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_generated_posts_user_id_created_at",
                table: "generated_posts",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_mockup_is_active_created_at",
                table: "mockup",
                columns: new[] { "is_active", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_platform_standards_platform",
                table: "platform_standards",
                column: "platform");

            migrationBuilder.CreateIndex(
                name: "IX_post_histories_base_id_created_at",
                table: "post_histories",
                columns: new[] { "base_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_post_histories_output_id",
                table: "post_histories",
                column: "output_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_histories_system_prompt_id",
                table: "post_histories",
                column: "system_prompt_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_prompts_type",
                table: "system_prompts",
                column: "type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "base_posts");

            migrationBuilder.DropTable(
                name: "card_templates");

            migrationBuilder.DropTable(
                name: "client_histories");

            migrationBuilder.DropTable(
                name: "generated_cards");

            migrationBuilder.DropTable(
                name: "generated_flowers");

            migrationBuilder.DropTable(
                name: "generated_posts");

            migrationBuilder.DropTable(
                name: "mockup");

            migrationBuilder.DropTable(
                name: "platform_standards");

            migrationBuilder.DropTable(
                name: "post_histories");

            migrationBuilder.DropTable(
                name: "system_prompts");

            migrationBuilder.Sql("DROP FUNCTION prevent_card_template_type_change();");

            migrationBuilder.DropIndex(
                name: "IX_Configs_Group_Key",
                table: "Configs");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:form_type", "go_may,calligraphy")
                .OldAnnotation("Npgsql:Enum:platform_type", "zalo,facebook,instagram")
                .OldAnnotation("Npgsql:Enum:system_prompt_types", "flower,card,post");
        }
    }
}
