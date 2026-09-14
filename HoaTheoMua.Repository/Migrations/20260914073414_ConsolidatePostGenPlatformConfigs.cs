using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations;

public partial class ConsolidatePostGenPlatformConfigs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            LOCK TABLE "Configs" IN SHARE ROW EXCLUSIVE MODE;
            DO $migration$
            DECLARE prompt record; standard jsonb; enabled boolean; legacy_key text;
            BEGIN
                FOR prompt IN SELECT * FROM "Configs"
                    WHERE "Group" = 'postgen' AND NOT "IsDeleted"
                    AND "Key" IN ('postgen.system-prompt.facebook', 'postgen.system-prompt.instagram', 'postgen.system-prompt.zalo')
                LOOP
                    IF jsonb_typeof(prompt."Value") IS DISTINCT FROM 'object'
                        OR (prompt."Value" ? 'isActive' AND jsonb_typeof(prompt."Value"->'isActive') IS DISTINCT FROM 'boolean')
                    THEN
                        RAISE EXCEPTION 'Cannot consolidate PostGen configs: invalid prompt value for %', prompt."Key";
                    END IF;
                    -- Explicit settings in the destination win; never overwrite prompt content.
                    IF NOT (prompt."Value" ? 'isActive') THEN
                        legacy_key := replace(prompt."Key", 'postgen.system-prompt.', 'postgen.platform.');
                        SELECT "Value" INTO standard FROM "Configs"
                            WHERE "Group" = 'postgen' AND "Key" = legacy_key AND NOT "IsDeleted";
                        IF FOUND THEN
                            IF jsonb_typeof(standard->'isActive') IS DISTINCT FROM 'boolean' THEN
                                RAISE EXCEPTION 'Cannot consolidate PostGen configs: invalid isActive for %', legacy_key;
                            END IF;
                            enabled := (standard->>'isActive')::boolean;
                        ELSE
                            -- A deleted standard previously disabled that platform. With no old
                            -- standard at all, initialize a usable standalone prompt config.
                            enabled := NOT EXISTS (SELECT 1 FROM "Configs" WHERE "Group" = 'postgen' AND "Key" = legacy_key);
                        END IF;
                        UPDATE "Configs" SET "Value" = jsonb_set("Value", '{isActive}', to_jsonb(enabled)), "UpdatedAt" = now()
                            WHERE "Id" = prompt."Id";
                    END IF;
                END LOOP;
            END $migration$;
            DELETE FROM "Configs" WHERE "Group" = 'postgen' AND "Key" LIKE 'postgen.platform.%';
            """);
        migrationBuilder.DropIndex(name: "IX_Configs_PostGenPlatforms", table: "Configs");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The removed descriptions/limits are intentionally retired, not retained in another
        // runtime store. A fabricated Down would silently lose custom data; restore a backup.
        migrationBuilder.Sql("""
            DO $migration$
            BEGIN
                RAISE EXCEPTION 'Cannot roll back consolidated PostGen platform configs: restore a pre-migration backup';
            END $migration$;
            """);
    }
}
