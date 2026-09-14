using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MovePlatformStandardsToConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Serialize source and destination writes until the copy and drop commit together.
            migrationBuilder.Sql("""
                LOCK TABLE "PlatformStandard", "Configs" IN ACCESS EXCLUSIVE MODE;
                DO $migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Configs" c JOIN "PlatformStandard" p
                        ON c."Id" = p."Id" OR lower(btrim(c."Key")) = 'postgen.platform.' || p."Platform"::text
                    ) THEN
                        RAISE EXCEPTION 'Platform config migration conflict: destination key or id already exists; reconcile before migrating';
                    END IF;
                END $migration$;
                INSERT INTO "Configs" ("Id", "Group", "Key", "Kind", "Value", "UserId", "IsPublic", "IsDeleted", "CreatedAt", "UpdatedAt")
                SELECT "Id", 'postgen', 'postgen.platform.' || "Platform"::text, 'Setting',
                    jsonb_build_object(
                        'description', "Description", 'maxContentLength', "MaxContentLength",
                        'maxHashtagCount', "MaxHashtagCount", 'maxImageCount', "MaxImageCount", 'isActive', "IsActive"),
                    '00000000-0000-0000-0000-000000000000'::uuid, false, "IsDeleted", "CreatedAt", "UpdatedAt"
                FROM "PlatformStandard";
                """);

            migrationBuilder.DropTable(name: "PlatformStandard");

            migrationBuilder.CreateIndex(
                name: "IX_Configs_PostGenPlatforms",
                table: "Configs",
                columns: new[] { "Group", "Key" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Group\" = 'postgen' AND \"Key\" IN ('postgen.platform.facebook', 'postgen.platform.instagram', 'postgen.platform.zalo')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reject shapes that cannot be restored losslessly to the old table.
            migrationBuilder.Sql("""
                LOCK TABLE "Configs" IN ACCESS EXCLUSIVE MODE;
                DO $migration$
                DECLARE config_row record;
                BEGIN
                    IF EXISTS (
                        SELECT "Key" FROM "Configs"
                        WHERE "Group" = 'postgen' AND "Key" IN ('postgen.platform.facebook', 'postgen.platform.instagram', 'postgen.platform.zalo')
                        GROUP BY "Key" HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Cannot restore PlatformStandard: multiple configs including deleted history exist for a platform';
                    END IF;
                    FOR config_row IN SELECT * FROM "Configs"
                        WHERE "Group" = 'postgen' AND "Key" IN ('postgen.platform.facebook', 'postgen.platform.instagram', 'postgen.platform.zalo')
                    LOOP
                        IF config_row."Kind" IS DISTINCT FROM 'Setting'
                            OR jsonb_typeof(config_row."Value") IS DISTINCT FROM 'object'
                            OR jsonb_typeof(config_row."Value"->'description') IS DISTINCT FROM 'string'
                            OR jsonb_typeof(config_row."Value"->'isActive') IS DISTINCT FROM 'boolean'
                            OR NOT (config_row."Value" ?& ARRAY['maxContentLength', 'maxHashtagCount', 'maxImageCount'])
                            OR jsonb_typeof(config_row."Value"->'maxContentLength') NOT IN ('number', 'null')
                            OR jsonb_typeof(config_row."Value"->'maxHashtagCount') NOT IN ('number', 'null')
                            OR jsonb_typeof(config_row."Value"->'maxImageCount') NOT IN ('number', 'null')
                            OR (config_row."Value" - ARRAY['description', 'isActive', 'maxContentLength', 'maxHashtagCount', 'maxImageCount']) <> '{}'::jsonb
                        THEN
                            RAISE EXCEPTION 'Cannot restore PlatformStandard: invalid config shape for %', config_row."Key";
                        END IF;
                        -- Integer casts reject fractional and out-of-range values before any data is deleted.
                        PERFORM (config_row."Value"->>'maxContentLength')::integer,
                            (config_row."Value"->>'maxHashtagCount')::integer,
                            (config_row."Value"->>'maxImageCount')::integer;
                    END LOOP;
                END $migration$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Configs_PostGenPlatforms",
                table: "Configs");

            migrationBuilder.CreateTable(
                name: "PlatformStandard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MaxContentLength = table.Column<int>(type: "integer", nullable: true),
                    MaxHashtagCount = table.Column<int>(type: "integer", nullable: true),
                    MaxImageCount = table.Column<int>(type: "integer", nullable: true),
                    Platform = table.Column<int>(type: "\"PlatformType\"", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformStandard", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformStandard_Platform",
                table: "PlatformStandard",
                column: "Platform",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO "PlatformStandard" ("Id", "Platform", "Description", "MaxContentLength", "MaxHashtagCount", "MaxImageCount", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
                SELECT "Id", replace("Key", 'postgen.platform.', '')::"PlatformType",
                    "Value"->>'description', ("Value"->>'maxContentLength')::integer,
                    ("Value"->>'maxHashtagCount')::integer, ("Value"->>'maxImageCount')::integer,
                    ("Value"->>'isActive')::boolean, "IsDeleted", "CreatedAt", "UpdatedAt"
                FROM "Configs"
                WHERE "Group" = 'postgen' AND "Key" IN ('postgen.platform.facebook', 'postgen.platform.instagram', 'postgen.platform.zalo');

                DELETE FROM "Configs"
                WHERE "Group" = 'postgen' AND "Key" IN ('postgen.platform.facebook', 'postgen.platform.instagram', 'postgen.platform.zalo');
                """);
        }
    }
}
