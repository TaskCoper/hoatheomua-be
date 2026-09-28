using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace HoaTheoMua.Repository.Migrations;

public partial class AddPostGenPlatformAccounts : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        // Reserved account IDs have never been assigned by PostGen APIs. Do not silently discard legacy values.
        m.Sql("""
            DO $migration$ BEGIN
                IF EXISTS (SELECT 1 FROM "PlatformPost" WHERE "PlatformAccountId" IS NOT NULL) THEN
                    RAISE EXCEPTION 'PlatformPost contains reserved PlatformAccountId values. Reconcile these before adding the account FK.';
                END IF;
            END $migration$;
            """);
        m.CreateTable("PlatformAccount", columns: t => new
        {
            Id = t.Column<Guid>("uuid", nullable: false),
            Platform = t.Column<PlatformType>("\"PlatformType\"", nullable: false),
            ExternalAccountId = t.Column<string>("character varying(128)", maxLength: 128, nullable: false),
            PageId = t.Column<string>("character varying(128)", maxLength: 128, nullable: true),
            DisplayName = t.Column<string>("character varying(300)", maxLength: 300, nullable: false),
            Status = t.Column<string>("character varying(20)", maxLength: 20, nullable: false),
            GrantedScopes = t.Column<string[]>("text[]", nullable: false),
            ProtectedCredentials = t.Column<string>("text", nullable: true),
            AccessTokenExpiresAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: true),
            RefreshTokenExpiresAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: true),
            LastValidatedAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: true),
            LastErrorCode = t.Column<string>("character varying(80)", maxLength: 80, nullable: true),
            ConnectedBy = t.Column<Guid>("uuid", nullable: false),
            CreatedAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: false),
            UpdatedAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: true),
            Revision = t.Column<long>("bigint", nullable: false),
            RefreshLeaseId = t.Column<Guid>("uuid", nullable: true),
            RefreshLeaseUntil = t.Column<DateTimeOffset>("timestamp with time zone", nullable: true),
            IsDeleted = t.Column<bool>("boolean", nullable: false)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_PlatformAccount", x => x.Id);
            t.CheckConstraint("CK_PlatformAccount_Status", "\"Status\" IN ('Connected','ReauthRequired','Disabled')");
            t.CheckConstraint("CK_PlatformAccount_Credentials", "\"Status\" <> 'Connected' OR \"ProtectedCredentials\" IS NOT NULL");
        });
        m.CreateTable("PlatformOAuthSession", columns: t => new
        {
            Id = t.Column<Guid>("uuid", nullable: false),
            Platform = t.Column<PlatformType>("\"PlatformType\"", nullable: false),
            StateHash = t.Column<string>("character varying(64)", maxLength: 64, nullable: false),
            BrowserHash = t.Column<string>("character varying(64)", maxLength: 64, nullable: false),
            ProtectedVerifier = t.Column<string>("text", nullable: true),
            RequestedBy = t.Column<Guid>("uuid", nullable: false),
            TargetAccountId = t.Column<Guid>("uuid", nullable: true),
            TargetRevision = t.Column<long>("bigint", nullable: true),
            CreatedAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: false),
            ExpiresAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: false),
            UsedAt = t.Column<DateTimeOffset>("timestamp with time zone", nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_PlatformOAuthSession", x => x.Id);
            t.ForeignKey("FK_PlatformOAuthSession_PlatformAccount_TargetAccountId", x => x.TargetAccountId, "PlatformAccount", "Id", onDelete: ReferentialAction.Restrict);
        });
        m.CreateIndex("IX_PlatformAccount_Platform_ExternalAccountId", "PlatformAccount", new[] { "Platform", "ExternalAccountId" }, unique: true);
        m.CreateIndex("IX_PlatformAccount_Status_AccessTokenExpiresAt", "PlatformAccount", new[] { "Status", "AccessTokenExpiresAt" });
        m.CreateIndex("IX_PlatformOAuthSession_StateHash", "PlatformOAuthSession", "StateHash", unique: true);
        m.CreateIndex("IX_PlatformOAuthSession_ExpiresAt", "PlatformOAuthSession", "ExpiresAt");
        m.CreateIndex("IX_PlatformOAuthSession_TargetAccountId", "PlatformOAuthSession", "TargetAccountId");
        m.CreateIndex("IX_PlatformPost_PlatformAccountId", "PlatformPost", "PlatformAccountId");
        m.AddForeignKey("FK_PlatformPost_PlatformAccount_PlatformAccountId", "PlatformPost", "PlatformAccountId", "PlatformAccount", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }
    protected override void Down(MigrationBuilder m)
    {
        m.DropForeignKey("FK_PlatformPost_PlatformAccount_PlatformAccountId", "PlatformPost");
        m.DropIndex("IX_PlatformPost_PlatformAccountId", "PlatformPost");
        m.DropTable("PlatformOAuthSession");
        m.DropTable("PlatformAccount");
    }
}
