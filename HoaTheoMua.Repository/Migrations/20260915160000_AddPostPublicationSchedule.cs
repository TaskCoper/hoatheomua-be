using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace HoaTheoMua.Repository.Migrations;

public partial class AddPostPublicationSchedule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(name: "ScheduledAt", table: "PostPublication", type: "timestamp with time zone", nullable: true);
        // M12 reserved Scheduled but exposed no schedule writer. If a legacy writer used it, require
        // explicit recovery rather than guessing a business timestamp from a retry/backoff timestamp.
        migrationBuilder.Sql("""
            DO $$ BEGIN
              IF EXISTS (SELECT 1 FROM "PostPublication" WHERE "Status" = 'Scheduled') THEN
                RAISE EXCEPTION 'Reserved Scheduled jobs exist; resolve them before applying schedule migration';
              END IF;
            END $$;
            """);
        migrationBuilder.AddCheckConstraint("CK_PostPublication_Schedule", "PostPublication",
            "\"Status\" <> 'Scheduled' OR (\"ScheduledAt\" IS NOT NULL AND \"NextRunAt\" IS NOT NULL AND \"NextRunAt\" >= \"ScheduledAt\" AND \"LeaseId\" IS NULL AND \"AttemptCount\" = 0 AND NOT \"PublishMayHaveOccurred\")");
        migrationBuilder.Sql("""
            CREATE FUNCTION postgen_protect_schedule_history() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF NEW."ScheduledAt" IS DISTINCT FROM OLD."ScheduledAt" AND NOT
                (OLD."Status" = 'Scheduled' AND NEW."Status" = 'Scheduled' AND OLD."LeaseId" IS NULL
                 AND OLD."AttemptCount" = 0 AND NOT OLD."PublishMayHaveOccurred") THEN
                RAISE EXCEPTION 'Schedule time may change only before claim' USING ERRCODE = '23514';
              END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER "TR_PostPublication_ScheduleHistory" BEFORE UPDATE ON "PostPublication"
              FOR EACH ROW EXECUTE FUNCTION postgen_protect_schedule_history();
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $$ BEGIN
              IF EXISTS (SELECT 1 FROM "PostPublication" WHERE "ScheduledAt" IS NOT NULL) THEN
                RAISE EXCEPTION 'Cannot remove schedule history; keep schema and disable publishing instead';
              END IF;
            END $$;
            """);
        migrationBuilder.DropCheckConstraint("CK_PostPublication_Schedule", "PostPublication");
        migrationBuilder.Sql("""
            DROP TRIGGER "TR_PostPublication_ScheduleHistory" ON "PostPublication";
            DROP FUNCTION postgen_protect_schedule_history();
            """);
        migrationBuilder.DropColumn("ScheduledAt", "PostPublication");
    }
}
