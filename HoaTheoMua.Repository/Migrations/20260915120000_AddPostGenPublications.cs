using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace HoaTheoMua.Repository.Migrations;

public partial class AddPostGenPublications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "PostPublication" (
              "Id" uuid CONSTRAINT "PK_PostPublication" PRIMARY KEY, "BasePostId" uuid NOT NULL CONSTRAINT "FK_PostPublication_BasePost_BasePostId" REFERENCES "BasePost"("Id") ON DELETE RESTRICT,
              "PlatformPostId" uuid NOT NULL CONSTRAINT "FK_PostPublication_PlatformPost_PlatformPostId" REFERENCES "PlatformPost"("Id") ON DELETE RESTRICT,
              "PlatformAccountId" uuid NOT NULL CONSTRAINT "FK_PostPublication_PlatformAccount_PlatformAccountId" REFERENCES "PlatformAccount"("Id") ON DELETE RESTRICT,
              "RequestedBy" uuid NOT NULL, "IdempotencyKey" varchar(128) NOT NULL, "RequestHash" varchar(64) NOT NULL,
              "Snapshot" jsonb NOT NULL, "Status" varchar(20) NOT NULL, "Revision" bigint NOT NULL,
              "AttemptCount" integer NOT NULL, "StepCount" integer NOT NULL, "Checkpoint" jsonb NOT NULL,
              "PublishMayHaveOccurred" boolean NOT NULL, "CanRetry" boolean NOT NULL,
              "LeaseId" uuid, "LeaseUntil" timestamptz, "NextRunAt" timestamptz, "LastDispatchedAt" timestamptz,
              "ExternalPostId" varchar(200), "ExternalPostUrl" varchar(2048), "ErrorCode" varchar(100),
              "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL, "CompletedAt" timestamptz,
              CONSTRAINT "CK_PostPublication_Status" CHECK ("Status" IN ('Queued','Scheduled','Publishing','Succeeded','Failed','Cancelled','NeedsReview','Closed')),
              CONSTRAINT "CK_PostPublication_Counters" CHECK ("Revision" > 0 AND "AttemptCount" >= 0 AND "StepCount" >= 0),
              CONSTRAINT "CK_PostPublication_Lease" CHECK (("LeaseId" IS NULL) = ("LeaseUntil" IS NULL)),
              CONSTRAINT "CK_PostPublication_Retry" CHECK (NOT "CanRetry" OR ("Status" = 'Failed' AND NOT "PublishMayHaveOccurred")),
              CONSTRAINT "CK_PostPublication_Success" CHECK ("Status" <> 'Succeeded' OR ("ExternalPostId" IS NOT NULL AND length("ExternalPostId") > 0))
            );
            CREATE UNIQUE INDEX "IX_PostPublication_PlatformPostId_IdempotencyKey" ON "PostPublication" ("PlatformPostId", "IdempotencyKey");
            CREATE UNIQUE INDEX "UX_PostPublication_Active" ON "PostPublication" ("PlatformPostId") WHERE "Status" IN ('Queued','Scheduled','Publishing','NeedsReview');
            CREATE INDEX "IX_PostPublication_PlatformPostId_CreatedAt_Id" ON "PostPublication" ("PlatformPostId", "CreatedAt", "Id");
            CREATE INDEX "IX_PostPublication_Status_NextRunAt_LeaseUntil" ON "PostPublication" ("Status", "NextRunAt", "LeaseUntil");
            CREATE INDEX "IX_PostPublication_BasePostId" ON "PostPublication" ("BasePostId");
            CREATE INDEX "IX_PostPublication_PlatformAccountId" ON "PostPublication" ("PlatformAccountId");

            CREATE TABLE "PostPublicationAttempt" (
              "Id" uuid CONSTRAINT "PK_PostPublicationAttempt" PRIMARY KEY, "PublicationId" uuid NOT NULL CONSTRAINT "FK_PostPublicationAttempt_PostPublication_PublicationId" REFERENCES "PostPublication"("Id") ON DELETE RESTRICT,
              "Number" integer NOT NULL, "Operation" varchar(40) NOT NULL, "Status" varchar(30) NOT NULL,
              "ErrorCode" varchar(100), "ProviderCode" varchar(100), "ProviderRequestId" varchar(200), "ExternalPostId" varchar(200),
              "StartedAt" timestamptz NOT NULL, "CompletedAt" timestamptz,
              CONSTRAINT "CK_PostPublicationAttempt_Number" CHECK ("Number" > 0)
            );
            CREATE UNIQUE INDEX "IX_PostPublicationAttempt_PublicationId_Number" ON "PostPublicationAttempt" ("PublicationId", "Number");
            CREATE TABLE "PostPublicationReview" (
              "Id" uuid CONSTRAINT "PK_PostPublicationReview" PRIMARY KEY, "PublicationId" uuid NOT NULL CONSTRAINT "FK_PostPublicationReview_PostPublication_PublicationId" REFERENCES "PostPublication"("Id") ON DELETE RESTRICT,
              "ActorId" uuid NOT NULL, "Action" varchar(40) NOT NULL, "Note" varchar(2000) NOT NULL,
              "ExternalPostId" varchar(200), "CreatedAt" timestamptz NOT NULL
            );
            CREATE INDEX "IX_PostPublicationReview_PublicationId_CreatedAt" ON "PostPublicationReview" ("PublicationId", "CreatedAt");

            CREATE FUNCTION postgen_protect_publication_snapshot() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF ROW(NEW."Id",NEW."BasePostId",NEW."PlatformPostId",NEW."PlatformAccountId",NEW."RequestedBy",NEW."IdempotencyKey",NEW."RequestHash",NEW."Snapshot",NEW."CreatedAt")
                 IS DISTINCT FROM
                 ROW(OLD."Id",OLD."BasePostId",OLD."PlatformPostId",OLD."PlatformAccountId",OLD."RequestedBy",OLD."IdempotencyKey",OLD."RequestHash",OLD."Snapshot",OLD."CreatedAt") THEN
                RAISE EXCEPTION 'Publication identity and snapshot are immutable' USING ERRCODE = '23514';
              END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER "TR_PostPublication_ImmutableSnapshot" BEFORE UPDATE ON "PostPublication"
              FOR EACH ROW EXECUTE FUNCTION postgen_protect_publication_snapshot();
            CREATE FUNCTION postgen_protect_publication_review() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              RAISE EXCEPTION 'Publication review is append-only' USING ERRCODE = '23514';
            END $$;
            CREATE TRIGGER "TR_PostPublicationReview_AppendOnly" BEFORE UPDATE OR DELETE ON "PostPublicationReview"
              FOR EACH ROW EXECUTE FUNCTION postgen_protect_publication_review();
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // A rollback must never erase publication evidence. Disable dispatch instead on a used DB.
        migrationBuilder.Sql("""
            DO $$ BEGIN
              IF EXISTS (SELECT 1 FROM "PostPublication") THEN
                RAISE EXCEPTION 'Cannot remove publication history; disable publishing instead';
              END IF;
            END $$;
            DROP TABLE "PostPublicationReview";
            DROP TABLE "PostPublicationAttempt";
            DROP TABLE "PostPublication";
            DROP FUNCTION postgen_protect_publication_review();
            DROP FUNCTION postgen_protect_publication_snapshot();
            """);
    }
}
