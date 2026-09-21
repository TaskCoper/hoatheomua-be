using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations;

public partial class AddGeneratedFlowerImageStages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "FlowerWithMockupImageUrl", table: "GeneratedFlower",
            type: "character varying(2048)", maxLength: 2048, nullable: true);
        migrationBuilder.AddColumn<string>(name: "FlowerWithLogoImageUrl", table: "GeneratedFlower",
            type: "character varying(2048)", maxLength: 2048, nullable: true);
        // Earlier two-stage builds kept this URL only in JSON. Preserve it without inventing outputs.
        migrationBuilder.Sql("""
            UPDATE "GeneratedFlower"
            SET "FlowerWithMockupImageUrl" = "Metadata"->>'flowerWithMockupImageUrl'
            WHERE jsonb_typeof("Metadata"->'flowerWithMockupImageUrl') = 'string'
              AND length(btrim("Metadata"->>'flowerWithMockupImageUrl')) BETWEEN 1 AND 2048
              AND length("Metadata"->>'flowerWithMockupImageUrl') <= 2048;
            UPDATE "GeneratedFlower"
            SET "FlowerWithLogoImageUrl" = "Metadata"->>'flowerWithLogoImageUrl'
            WHERE "FlowerWithMockupImageUrl" IS NOT NULL
              AND jsonb_typeof("Metadata"->'flowerWithLogoImageUrl') = 'string'
              AND length(btrim("Metadata"->>'flowerWithLogoImageUrl')) BETWEEN 1 AND 2048
              AND length("Metadata"->>'flowerWithLogoImageUrl') <= 2048;
            """);
        migrationBuilder.AddCheckConstraint(name: "CK_generated_flower_logo_requires_mockup", table: "GeneratedFlower",
            sql: "\"FlowerWithLogoImageUrl\" IS NULL OR \"FlowerWithMockupImageUrl\" IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep new URL facts in the legacy JSON before removing the columns.
        migrationBuilder.Sql("""
            UPDATE "GeneratedFlower"
            SET "Metadata" = "Metadata" || jsonb_strip_nulls(jsonb_build_object(
                'flowerWithMockupImageUrl', "FlowerWithMockupImageUrl",
                'flowerWithLogoImageUrl', "FlowerWithLogoImageUrl"))
            WHERE jsonb_typeof("Metadata") = 'object';
            """);
        migrationBuilder.DropCheckConstraint(name: "CK_generated_flower_logo_requires_mockup", table: "GeneratedFlower");
        migrationBuilder.DropColumn(name: "FlowerWithLogoImageUrl", table: "GeneratedFlower");
        migrationBuilder.DropColumn(name: "FlowerWithMockupImageUrl", table: "GeneratedFlower");
    }
}
