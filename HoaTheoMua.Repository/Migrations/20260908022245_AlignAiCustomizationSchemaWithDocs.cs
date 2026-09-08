using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AlignAiCustomizationSchemaWithDocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "system_form",
                table: "client_histories",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE client_histories AS history
                SET system_form = jsonb_build_object(
                    'type', prompt.type::text,
                    'content', prompt.content)
                FROM system_prompts AS prompt
                WHERE history.system_prompt_id = prompt.id;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_client_histories_system_prompts_system_prompt_id",
                table: "client_histories");

            migrationBuilder.DropIndex(
                name: "IX_Configs_Group_Key",
                table: "Configs");

            migrationBuilder.DropIndex(
                name: "IX_client_histories_system_prompt_id",
                table: "client_histories");

            migrationBuilder.DropColumn(
                name: "input",
                table: "client_histories");

            migrationBuilder.DropColumn(
                name: "system_prompt_id",
                table: "client_histories");

            migrationBuilder.CreateIndex(
                name: "IX_Configs_Group_Key",
                table: "Configs",
                columns: new[] { "Group", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Kind\" = 'Setting' AND \"Group\" = 'card_config' AND \"Key\" IN ('card_config.sizes', 'card_config.prices')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Configs_Group_Key",
                table: "Configs");

            migrationBuilder.AddColumn<string>(
                name: "input",
                table: "client_histories",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<Guid>(
                name: "system_prompt_id",
                table: "client_histories",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE client_histories AS history
                SET system_prompt_id = prompt.id
                FROM system_prompts AS prompt
                WHERE history.system_form IS NOT NULL
                  AND history.system_form ->> 'type' = prompt.type::text;
                """);

            migrationBuilder.DropColumn(
                name: "system_form",
                table: "client_histories");

            migrationBuilder.CreateIndex(
                name: "IX_Configs_Group_Key",
                table: "Configs",
                columns: new[] { "Group", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsPublic\" = true AND \"Kind\" = 'Setting' AND \"Group\" IN ('card_size', 'card_config')");

            migrationBuilder.CreateIndex(
                name: "IX_client_histories_system_prompt_id",
                table: "client_histories",
                column: "system_prompt_id");

            migrationBuilder.AddForeignKey(
                name: "FK_client_histories_system_prompts_system_prompt_id",
                table: "client_histories",
                column: "system_prompt_id",
                principalTable: "system_prompts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
