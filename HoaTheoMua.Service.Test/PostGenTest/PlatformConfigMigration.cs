using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class PlatformConfigMigration
{
    [PostGenPostgresFact]
    public async Task PreservesDataRejectsConflictsAndRollsBackWithoutPartialWrites()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "platform_config_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString);
        await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = database;
        try
        {
            var builder = new NpgsqlDataSourceBuilder(settings.ConnectionString);
            builder.EnableDynamicJson();
            builder.MapEnum<PlatformType>("PlatformType");
            builder.MapEnum<SystemPromptType>("SystemPromptType");
            builder.MapEnum<CardFormType>("CardFormType");
            builder.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = builder.Build();
            await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options);
            var migrator = db.GetService<IMigrator>();
            const string previous = "20260912053534_AddPostGenCore";
            var current = db.Database.GetMigrations().Last();
            await migrator.MigrateAsync(previous);
            await db.Database.OpenConnectionAsync();
            await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            var facebookId = Guid.Parse("e9b6e350-39e6-49d6-a200-000000000001");
            const string facebookKey = "postgen.platform.facebook";
            await db.Database.ExecuteSqlRawAsync("""
                UPDATE "PlatformStandard" SET "Description" = 'Custom description', "MaxContentLength" = NULL,
                  "MaxHashtagCount" = 0, "MaxImageCount" = 7, "IsActive" = false,
                  "UpdatedAt" = '2026-09-13T01:02:03Z' WHERE "Platform" = 'facebook';
                UPDATE "PlatformStandard" SET "IsDeleted" = true WHERE "Platform" = 'zalo';
                """);
            var baseline = await Snapshot(db);
            var conflictId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Configs" ("Id", "Group", "Key", "Kind", "Value", "UserId", "CreatedAt", "IsDeleted")
                VALUES ({conflictId}, 'postgen', {facebookKey}, 'Setting', CAST({"{}"} AS jsonb), {Guid.Empty}, now(), false)
                """);
            var conflict = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(current));
            Assert.Contains("Platform config migration conflict", conflict.MessageText);
            Assert.Equal(baseline, await Snapshot(db));
            Assert.False((await db.Database.GetAppliedMigrationsAsync()).Contains(current));
            Assert.Equal(1, await db.Configs.CountAsync(x => x.Key.StartsWith("postgen.platform.")));
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Configs\" WHERE \"Id\" = {conflictId}");
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Configs" ("Id", "Group", "Key", "Kind", "Value", "UserId", "CreatedAt", "IsDeleted")
                VALUES ({facebookId}, 'unrelated', 'unrelated.id-conflict', 'Setting', CAST({"{}"} AS jsonb), {Guid.Empty}, now(), false)
                """);
            await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(current));
            Assert.Equal(baseline, await Snapshot(db));
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Configs\" WHERE \"Id\" = {facebookId}");

            await migrator.MigrateAsync(current);
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.False(await TableExists(db));
            var configs = await db.Configs.AsNoTracking().Where(x => x.Key.StartsWith("postgen.platform.")).ToListAsync();
            Assert.Equal(3, configs.Count);
            Assert.All(configs, c => { Assert.Equal("postgen", c.Group); Assert.Equal("Setting", c.Kind); Assert.False(c.IsPublic); Assert.Equal(Guid.Empty, c.UserId); });
            var facebook = configs.Single(x => x.Key == facebookKey);
            Assert.Equal(facebookId, facebook.Id);
            Assert.Equal(new DateTimeOffset(2026, 9, 13, 1, 2, 3, TimeSpan.Zero), facebook.UpdatedAt);
            Assert.Equal(new DateTimeOffset(2026, 9, 12, 0, 0, 0, TimeSpan.Zero), facebook.CreatedAt);
            var value = JsonDocument.Parse(facebook.Value).RootElement;
            Assert.Equal("Custom description", value.GetProperty("description").GetString());
            Assert.Equal(JsonValueKind.Null, value.GetProperty("maxContentLength").ValueKind);
            Assert.Equal(0, value.GetProperty("maxHashtagCount").GetInt32());
            Assert.Equal(7, value.GetProperty("maxImageCount").GetInt32());
            Assert.False(value.GetProperty("isActive").GetBoolean());
            Assert.True(configs.Single(x => x.Key.EndsWith("zalo")).IsDeleted);
            var duplicate = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Configs" ("Id", "Group", "Key", "Kind", "Value", "UserId", "CreatedAt", "IsDeleted")
                VALUES ({Guid.NewGuid()}, 'postgen', {facebookKey}, 'Setting', CAST({"{}"} AS jsonb), {Guid.Empty}, now(), false)
                """));
            Assert.Equal(PostgresErrorCodes.UniqueViolation, duplicate.SqlState);

            await migrator.MigrateAsync(previous);
            Assert.Equal(baseline, await Snapshot(db));
            Assert.False(await db.Configs.AnyAsync(x => x.Key.StartsWith("postgen.platform.")));
            await migrator.MigrateAsync(current);
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Configs\" SET \"Value\" = '{{}}' WHERE \"Id\" = {facebookId}");
            var invalid = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(previous));
            Assert.Contains("Cannot restore PlatformStandard", invalid.MessageText);
            Assert.False(await TableExists(db));
            Assert.Equal(3, await db.Configs.CountAsync(x => x.Key.StartsWith("postgen.platform.")));
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Configs\" SET \"Value\" = CAST({facebook.Value} AS jsonb) WHERE \"Id\" = {facebookId}");
            // Config allows deleted history plus a new active row. The old table cannot represent both.
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Configs" ("Id", "Group", "Key", "Kind", "Value", "UserId", "CreatedAt", "IsDeleted")
                VALUES ({Guid.NewGuid()}, 'postgen', {facebookKey}, 'Setting', CAST({facebook.Value} AS jsonb), {Guid.Empty}, now(), true)
                """);
            await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(previous));
            Assert.False(await TableExists(db));
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Configs\" WHERE \"Key\" = {facebookKey} AND \"Id\" <> {facebookId}");
            await migrator.MigrateAsync(previous);
            Assert.Equal(baseline, await Snapshot(db));
            Assert.Equal(2, await db.Configs.CountAsync(x => x.Group == "postgen"));
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }

    private static Task<string> Snapshot(AppDbContext db) => db.Database.SqlQueryRaw<string>("""
        SELECT jsonb_agg(to_jsonb(p) ORDER BY p."Id")::text AS "Value" FROM "PlatformStandard" p
        """).SingleAsync();
    private static Task<bool> TableExists(AppDbContext db) => db.Database.SqlQueryRaw<bool>("""
        SELECT to_regclass('public."PlatformStandard"') IS NOT NULL AS "Value"
        """).SingleAsync();
}
