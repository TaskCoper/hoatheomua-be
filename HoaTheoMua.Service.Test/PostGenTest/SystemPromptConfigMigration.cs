using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class SystemPromptConfigMigration
{
    [PostGenPostgresFact]
    public async Task PreservesCustomPromptsSeedsMissingKeysAndProtectsProvenanceOnRollback()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "prompt_config_test_" + Guid.NewGuid().ToString("N");
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
            const string previous = "20260914055741_MovePlatformStandardsToConfig";
            const string current = "20260914065606_AddPostGenSystemPromptConfigs";
            Assert.DoesNotContain("ALTER TYPE", migrator.GenerateScript(previous, current), StringComparison.OrdinalIgnoreCase);
            await migrator.MigrateAsync(previous);
            await db.Database.OpenConnectionAsync();
            await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            var custom = Prompt("facebook", "Custom Facebook prompt — keep exactly.");
            var duplicate = Prompt("facebook", "Duplicate must block migration");
            var deleted = Prompt("instagram", "Deleted history");
            deleted.IsDeleted = true;
            db.Configs.AddRange(custom, duplicate, deleted);
            await db.SaveChangesAsync();
            var baseline = await Snapshot(db);
            var conflict = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(current));
            Assert.Equal(PostgresErrorCodes.UniqueViolation, conflict.SqlState);
            Assert.Equal(baseline, await Snapshot(db));
            Assert.DoesNotContain(current, await db.Database.GetAppliedMigrationsAsync());
            Assert.False(await HasReferenceColumn(db));
            db.Configs.Remove(duplicate);
            await db.SaveChangesAsync();
            var customBefore = await ConfigSnapshot(db, custom.Id);

            await migrator.MigrateAsync(current);
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal(customBefore, await ConfigSnapshot(db, custom.Id));
            var prompts = await db.Configs.AsNoTracking().Where(x => x.Group == "postgen" && x.Key.StartsWith("postgen.system-prompt.") && !x.IsDeleted).ToListAsync();
            Assert.Equal(3, prompts.Count);
            Assert.Equal(3, prompts.Select(x => x.Value).Distinct().Count());
            Assert.True(await db.Configs.AnyAsync(x => x.Id == deleted.Id && x.IsDeleted));
            foreach (var prompt in prompts.Where(x => x.Id != custom.Id))
            {
                Assert.Equal("Setting", prompt.Kind);
                Assert.False(prompt.IsPublic);
                Assert.Equal(Guid.Empty, prompt.UserId);
                var content = JsonDocument.Parse(prompt.Value).RootElement.GetProperty("content").GetString()!;
                Assert.True(content.Length > 1000);
                Assert.Contains(prompt.Key.Split('.').Last(), content);
                Assert.Contains("QUY TẮC NỀN TẢNG", content);
            }
            var uniqueError = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Configs" ("Id", "Group", "Key", "Kind", "Value", "UserId", "CreatedAt", "IsDeleted")
                VALUES ({Guid.NewGuid()}, 'postgen', {custom.Key}, 'Content', CAST({"{}"} AS jsonb), {Guid.Empty}, now(), false)
                """));
            Assert.Equal(PostgresErrorCodes.UniqueViolation, uniqueError.SqlState);

            var post = new BasePost { Id = Guid.NewGuid(), Title = "Test", Content = "Source", CreatedAt = DateTimeOffset.UtcNow };
            var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Generated, CreatedAt = DateTimeOffset.UtcNow };
            var legacyPrompt = await db.SystemPrompts.SingleAsync(x => x.Type == SystemPromptType.Post);
            var legacy = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Source = ContentSource.Ai, Content = "Legacy", SystemPromptId = legacyPrompt.Id, PromptSnapshot = "Legacy snapshot", CreatedAt = DateTimeOffset.UtcNow };
            var generated = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Source = ContentSource.Ai, Content = "New", SystemPromptConfigId = custom.Id, PromptSnapshot = "Custom Facebook prompt — keep exactly.", CreatedAt = DateTimeOffset.UtcNow };
            var manual = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Source = ContentSource.Manual, Content = "Manual", CreatedAt = DateTimeOffset.UtcNow };
            db.BasePosts.Add(post); db.PlatformPosts.Add(platform); db.GeneratedPosts.AddRange(legacy, generated, manual);
            await db.SaveChangesAsync();
            var invalidReference = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"GeneratedPost\" SET \"SystemPromptConfigId\" = {Guid.NewGuid()} WHERE \"Id\" = {generated.Id}"));
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, invalidReference.SqlState);
            var deleteError = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Configs\" WHERE \"Id\" = {custom.Id}"));
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, deleteError.SqlState);

            custom.Value = "{\"content\":\"Updated prompt\"}";
            custom.IsDeleted = true;
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var history = await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Id == generated.Id);
            Assert.Equal("Custom Facebook prompt — keep exactly.", history.PromptSnapshot);
            Assert.Equal(custom.Id, history.SystemPromptConfigId);
            Assert.Null(history.SystemPromptId);
            var rollbackError = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(previous));
            Assert.Contains("generated posts still reference Config prompts", rollbackError.MessageText);
            Assert.True(await HasReferenceColumn(db));
            Assert.Contains(current, await db.Database.GetAppliedMigrationsAsync());

            // Only remove the synthetic version to exercise a safe rollback; legacy and manual remain.
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"GeneratedPost\" WHERE \"Id\" = {generated.Id}");
            var preserved = await Snapshot(db);
            await migrator.MigrateAsync(previous);
            Assert.False(await HasReferenceColumn(db));
            Assert.Equal(preserved, await Snapshot(db));
            await migrator.MigrateAsync(current);
            Assert.Equal(3, await db.Configs.CountAsync(x => x.Group == "postgen" && x.Key.StartsWith("postgen.system-prompt.") && !x.IsDeleted));
            Assert.Equal("{\"content\": \"Updated prompt\"}", (await db.Configs.AsNoTracking().SingleAsync(x => x.Id == custom.Id)).Value);
            Assert.Equal(legacyPrompt.Id, (await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Id == legacy.Id)).SystemPromptId);
            var manualAfter = await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Id == manual.Id);
            Assert.Null(manualAfter.SystemPromptId); Assert.Null(manualAfter.SystemPromptConfigId); Assert.Null(manualAfter.PromptSnapshot);
            var afterReapply = await Snapshot(db);
            await migrator.MigrateAsync(previous);
            await migrator.MigrateAsync(current);
            Assert.Equal(afterReapply, await Snapshot(db));
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }

    private static HoaTheoMua.Repository.Entity.Config Prompt(string platform, string content) => new()
    {
        Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.system-prompt." + platform, Kind = "Setting",
        Value = JsonSerializer.Serialize(new { content }), UserId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow
    };
    private static Task<string> Snapshot(AppDbContext db) => db.Database.SqlQueryRaw<string>("""
        SELECT jsonb_agg(to_jsonb(c) ORDER BY c."Id")::text AS "Value" FROM "Configs" c
        """).SingleAsync();
    private static Task<string> ConfigSnapshot(AppDbContext db, Guid id) => db.Database.SqlQuery<string>($"SELECT to_jsonb(c)::text AS \"Value\" FROM \"Configs\" c WHERE \"Id\" = {id}").SingleAsync();
    private static Task<bool> HasReferenceColumn(AppDbContext db) => db.Database.SqlQueryRaw<bool>("""
        SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'GeneratedPost' AND column_name = 'SystemPromptConfigId') AS "Value"
        """).SingleAsync();
}
