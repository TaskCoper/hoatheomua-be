using System;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fixTableMockup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename Mockup → FlowerReference only if Mockup still exists and FlowerReference does not
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF to_regclass('""Mockup""') IS NOT NULL
                       AND to_regclass('""FlowerReference""') IS NULL THEN
                        ALTER TABLE ""Mockup"" RENAME TO ""FlowerReference"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF to_regtype('""FlowerReferenceType""') IS NULL THEN
                        CREATE TYPE ""FlowerReferenceType"" AS ENUM ('flower', 'bundle');
                    END IF;
                END $$;
            ");

            // Mockup had no Type column; FlowerReference stores it as a PostgreSQL enum.
            migrationBuilder.Sql(@"
                ALTER TABLE ""FlowerReference"" ADD COLUMN IF NOT EXISTS ""Type"" ""FlowerReferenceType"" NOT NULL DEFAULT 'flower';
                ALTER TABLE ""FlowerReference"" ALTER COLUMN ""Type"" DROP DEFAULT;
            ");
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_attribute
                        WHERE attrelid = '""FlowerReference""'::regclass
                          AND attname = 'Type'
                          AND atttypid <> '""FlowerReferenceType""'::regtype
                          AND NOT attisdropped
                    ) THEN
                        ALTER TABLE ""FlowerReference"" ALTER COLUMN ""Type"" TYPE ""FlowerReferenceType""
                            USING lower(""Type""::text)::""FlowerReferenceType"";
                    END IF;
                END $$;
            ");

            // Drop columns only if they still exist
            migrationBuilder.Sql(@"
                ALTER TABLE ""GeneratedCard"" DROP COLUMN IF EXISTS ""ReceiverName"";
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE ""GeneratedCard"" DROP COLUMN IF EXISTS ""SenderName"";
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'ClientHistory' AND column_name = 'RootID'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'ClientHistory' AND column_name = 'RootId'
                    ) THEN
                        ALTER TABLE ""ClientHistory"" RENAME COLUMN ""RootID"" TO ""RootId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.AlterColumn<SystemPromptType>(
                name: "Type",
                table: "SystemPrompt",
                type: "\"SystemPromptType\"",
                nullable: false,
                oldClrType: typeof(SystemPromptType),
                oldType: "SystemPromptType");

            migrationBuilder.AlterColumn<PlatformType>(
                name: "Platform",
                table: "PlatformStandard",
                type: "\"PlatformType\"",
                nullable: false,
                oldClrType: typeof(PlatformType),
                oldType: "PlatformType");

            migrationBuilder.Sql(@"
                ALTER TABLE ""OrderDetails"" ADD COLUMN IF NOT EXISTS ""SnapshotCard"" jsonb;
            ");

            migrationBuilder.AlterColumn<CardFormType>(
                name: "FormType",
                table: "GeneratedCard",
                type: "\"CardFormType\"",
                nullable: false,
                oldClrType: typeof(CardFormType),
                oldType: "CardFormType");

            migrationBuilder.AlterColumn<ClientHistoryType>(
                name: "Type",
                table: "ClientHistory",
                type: "\"ClientHistoryType\"",
                nullable: false,
                oldClrType: typeof(ClientHistoryType),
                oldType: "ClientHistoryType");

            // Create index only if not already exists
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_FlowerReference_IsActive_CreatedAt"" ON ""FlowerReference"" (""IsActive"", ""CreatedAt"") WHERE ""IsDeleted"" = false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename FlowerReference → Mockup only if FlowerReference exists and Mockup does not
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF to_regclass('""FlowerReference""') IS NOT NULL
                       AND to_regclass('""Mockup""') IS NULL THEN
                        ALTER TABLE ""FlowerReference"" RENAME TO ""Mockup"";
                    END IF;
                END $$;
            ");

            // Add columns back only if they don't exist
            migrationBuilder.Sql(@"
                ALTER TABLE ""GeneratedCard"" ADD COLUMN IF NOT EXISTS ""ReceiverName"" character varying(500) NOT NULL DEFAULT '';
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE ""GeneratedCard"" ADD COLUMN IF NOT EXISTS ""SenderName"" character varying(500) NOT NULL DEFAULT '';
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""OrderDetails"" DROP COLUMN IF EXISTS ""SnapshotCard"";
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'ClientHistory' AND column_name = 'RootId'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'ClientHistory' AND column_name = 'RootID'
                    ) THEN
                        ALTER TABLE ""ClientHistory"" RENAME COLUMN ""RootId"" TO ""RootID"";
                    END IF;
                END $$;
            ");

            migrationBuilder.AlterColumn<SystemPromptType>(
                name: "Type",
                table: "SystemPrompt",
                type: "SystemPromptType",
                nullable: false,
                oldClrType: typeof(SystemPromptType),
                oldType: "\"SystemPromptType\"");

            migrationBuilder.AlterColumn<PlatformType>(
                name: "Platform",
                table: "PlatformStandard",
                type: "PlatformType",
                nullable: false,
                oldClrType: typeof(PlatformType),
                oldType: "\"PlatformType\"");

            migrationBuilder.AlterColumn<CardFormType>(
                name: "FormType",
                table: "GeneratedCard",
                type: "CardFormType",
                nullable: false,
                oldClrType: typeof(CardFormType),
                oldType: "\"CardFormType\"");

            migrationBuilder.AlterColumn<ClientHistoryType>(
                name: "Type",
                table: "ClientHistory",
                type: "ClientHistoryType",
                nullable: false,
                oldClrType: typeof(ClientHistoryType),
                oldType: "\"ClientHistoryType\"");

            // Drop index only if it exists
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_FlowerReference_IsActive_CreatedAt"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Mockup"" DROP COLUMN IF EXISTS ""Type"";
            ");

            migrationBuilder.Sql(@"
                DROP TYPE IF EXISTS ""FlowerReferenceType"";
            ");
        }
    }
}
