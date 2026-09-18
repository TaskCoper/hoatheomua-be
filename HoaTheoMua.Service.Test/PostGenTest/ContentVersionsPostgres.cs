using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class ContentVersionsPostgres
{
    [PostGenPostgresFact]
    public async Task ConcurrentSavesSourceProtectionAndContentTransactionsWorkOnPostgres()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "content_versions_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = database;
        try
        {
            var builder = new NpgsqlDataSourceBuilder(settings.ConnectionString); builder.EnableDynamicJson();
            builder.MapEnum<PlatformType>("PlatformType"); builder.MapEnum<SystemPromptType>("SystemPromptType");
            builder.MapEnum<CardFormType>("CardFormType"); builder.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = builder.Build();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options;
            await using var db = new AppDbContext(options); await db.Database.MigrateAsync();
            await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            var id = await GeneratePlatformPosts.Seed(db, configs: false);
            var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
            await GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook));
            async Task SaveManual(string text)
            {
                await using var context = new AppDbContext(options);
                await GeneratePlatformPosts.Service(context, AiContentClient.Client(handler)).EditPlatformContent(id, "facebook", new() { Content = text });
            }
            await Task.WhenAll(SaveManual("Manual A"), SaveManual("Manual B"));
            Assert.Equal(2, await db.GeneratedPosts.CountAsync());
            var manual = await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Source == ContentSource.Manual);
            Assert.Contains(manual.Content, new[] { "Manual A", "Manual B" });
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var blockedHandler = new AiContentClient.Handler(async (_, ct) => { entered.TrySetResult(); await release.Task.WaitAsync(ct); return AiContentClient.Completion(); });
            await using var generatingDb = new AppDbContext(options);
            var generation = GeneratePlatformPosts.Service(generatingDb, AiContentClient.Client(blockedHandler))
                .RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = manual.Id });
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await using var other = new AppDbContext(options);
                var service = GeneratePlatformPosts.Service(other, AiContentClient.Client(handler));
                await Assert.ThrowsAsync<ConflictException>(() => service.EditPlatformContent(id, "facebook", new() { Content = "Cannot rewrite source during AI" }));
                await Assert.ThrowsAsync<ConflictException>(() => service.SelectOfficialContent(id, "facebook", new() { GenerationId = manual.Id }));
                await Assert.ThrowsAsync<ConflictException>(() => service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook)));
                await Assert.ThrowsAsync<ConflictException>(() => new HoaTheoMua.Service.PostCore.Service(other).DeletePostCore(id));
                Assert.Equal(2, (await service.GetContentVersions(id, "facebook")).Versions.Count);
            }
            finally { release.TrySetResult(); }
            await generation;
            Assert.Equal(manual.Content, (await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Id == manual.Id)).Content);
            var official = (await db.PlatformPosts.AsNoTracking().SingleAsync()).OfficialGenerationId;
            var failureOptions = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).AddInterceptors(new RejectSelection()).Options;
            await using (var failureDb = new AppDbContext(failureOptions))
                await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failureDb, AiContentClient.Client(handler)).RegeneratePlatformContent(id, "facebook", new()));
            Assert.Equal(3, await db.GeneratedPosts.CountAsync());
            Assert.Equal(official, (await db.PlatformPosts.AsNoTracking().SingleAsync()).OfficialGenerationId);
            await using (var failureDb = new AppDbContext(failureOptions))
                await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failureDb, AiContentClient.Client(handler)).EditPlatformContent(id, "facebook", new() { Content = "Rollback new manual" }));
            Assert.Equal(3, await db.GeneratedPosts.CountAsync());
            // Coalesced manual update also rolls back if selecting it fails in the same transaction.
            await SaveManual("Stable manual");
            await using (var failureDb = new AppDbContext(failureOptions))
                await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failureDb, AiContentClient.Client(handler)).EditPlatformContent(id, "facebook", new() { Content = "Rollback overwrite" }));
            Assert.Equal(4, await db.GeneratedPosts.CountAsync());
            Assert.True(await db.GeneratedPosts.AnyAsync(x => x.Content == "Stable manual"));
            Assert.False(await db.GeneratedPosts.AnyAsync(x => x.Content == "Rollback overwrite"));
            var beforeSelect = (await db.PlatformPosts.AsNoTracking().SingleAsync()).OfficialGenerationId;
            await using (var failureDb = new AppDbContext(failureOptions))
                await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failureDb, AiContentClient.Client(handler)).SelectOfficialContent(id, "facebook", new() { GenerationId = manual.Id }));
            Assert.Equal(beforeSelect, (await db.PlatformPosts.AsNoTracking().SingleAsync()).OfficialGenerationId);
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
    private sealed class RejectSelection : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<PlatformPost>().Any(x => x.State == EntityState.Modified && x.Property(p => p.SelectedAt).IsModified))
                throw new InvalidOperationException("Simulated selection persistence failure");
            return ValueTask.FromResult(result);
        }
    }
}
