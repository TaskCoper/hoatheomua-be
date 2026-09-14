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

public class ConsolidatePlatformConfigs
{
    [PostGenPostgresFact]
    public async Task MovesActivationPreservesPromptProvenanceAndDeletesRetiredKeysAtomically()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "consolidate_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = database;
        try
        {
            var builder = new NpgsqlDataSourceBuilder(settings.ConnectionString); builder.EnableDynamicJson();
            builder.MapEnum<PlatformType>("PlatformType"); builder.MapEnum<SystemPromptType>("SystemPromptType");
            builder.MapEnum<CardFormType>("CardFormType"); builder.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = builder.Build();
            await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options);
            var migrator = db.GetService<IMigrator>();
            const string previous = "20260914065606_AddPostGenSystemPromptConfigs";
            const string current = "20260914073414_ConsolidatePostGenPlatformConfigs";
            await migrator.MigrateAsync(previous);
            await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            var prompts = await db.Configs.Where(x => x.Key.StartsWith("postgen.system-prompt.")).ToListAsync();
            var facebook = prompts.Single(x => x.Key.EndsWith("facebook"));
            var instagram = prompts.Single(x => x.Key.EndsWith("instagram"));
            var originalContent = prompts.ToDictionary(x => x.Id, x => JsonDocument.Parse(x.Value).RootElement.GetProperty("content").GetString());
            var fbStandard = await db.Configs.SingleAsync(x => x.Key == "postgen.platform.facebook");
            fbStandard.Value = "{\"isActive\":false,\"description\":\"Retired custom rules\"}";
            (await db.Configs.SingleAsync(x => x.Key == "postgen.platform.instagram")).Value = "{\"isActive\":false}";
            (await db.Configs.SingleAsync(x => x.Key == "postgen.platform.zalo")).IsDeleted = true;
            instagram.Value = JsonSerializer.Serialize(new { isActive = true, content = originalContent[instagram.Id] });
            var unrelated = new HoaTheoMua.Repository.Entity.Config { Id = Guid.NewGuid(), Group = "other", Key = "postgen.platform.facebook", Kind = "Setting", Value = "{}" };
            db.Configs.Add(unrelated);
            var baseId = await GeneratePlatformPosts.Seed(db, configs: false);
            var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = baseId, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Generated };
            var version = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Source = ContentSource.Ai, Content = "Generated", SystemPromptConfigId = facebook.Id, PromptSnapshot = originalContent[facebook.Id] };
            db.PlatformPosts.Add(platform); db.GeneratedPosts.Add(version);
            facebook.Value = "{\"content\":\"Invalid temporary test value\",\"isActive\":\"true\"}";
            await db.SaveChangesAsync();
            var beforeFailure = await Snapshot(db);
            await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(current));
            Assert.Equal(beforeFailure, await Snapshot(db));
            Assert.DoesNotContain(current, await db.Database.GetAppliedMigrationsAsync());
            facebook.Value = JsonSerializer.Serialize(new { content = originalContent[facebook.Id] });
            fbStandard.Value = "{\"isActive\":\"false\"}";
            await db.SaveChangesAsync();
            beforeFailure = await Snapshot(db);
            await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(current));
            Assert.Equal(beforeFailure, await Snapshot(db));
            fbStandard.Value = "{\"isActive\":false}";
            await db.SaveChangesAsync();
            await migrator.MigrateAsync(current);
            db.ChangeTracker.Clear();
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.False(await db.Configs.AnyAsync(x => x.Group == "postgen" && x.Key.StartsWith("postgen.platform.")));
            Assert.True(await db.Configs.AnyAsync(x => x.Id == unrelated.Id));
            foreach (var prompt in await db.Configs.AsNoTracking().Where(x => x.Key.StartsWith("postgen.system-prompt.")).ToListAsync())
            {
                using var value = JsonDocument.Parse(prompt.Value);
                Assert.Equal(originalContent[prompt.Id], value.RootElement.GetProperty("content").GetString());
                Assert.Equal(prompt.Id == instagram.Id, value.RootElement.GetProperty("isActive").GetBoolean());
            }
            var saved = await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Id == version.Id);
            Assert.Equal(facebook.Id, saved.SystemPromptConfigId); Assert.Equal(originalContent[facebook.Id], saved.PromptSnapshot);
            var consolidated = await Snapshot(db);
            var downError = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(previous));
            Assert.Contains("restore a pre-migration backup", downError.MessageText);
            Assert.Equal(consolidated, await Snapshot(db));
            Assert.Contains(current, await db.Database.GetAppliedMigrationsAsync());
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
    private static Task<string> Snapshot(AppDbContext db) => db.Database.SqlQueryRaw<string>("""
        SELECT jsonb_agg(to_jsonb(c) ORDER BY c."Id")::text AS "Value" FROM "Configs" c
        """).SingleAsync();
}
