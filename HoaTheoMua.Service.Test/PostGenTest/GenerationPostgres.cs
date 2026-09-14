using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class GenerationPostgres
{
    [PostGenPostgresFact]
    public async Task ConcurrentGenerationCoreMutationsAndFailedOfficialWriteAreAtomic()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "generation_test_" + Guid.NewGuid().ToString("N");
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
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var handler = new AiContentClient.Handler(async (_, ct) => { entered.TrySetResult(); await release.Task.WaitAsync(ct); return AiContentClient.Completion(); });
            var generation = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id,
                GeneratePlatformPosts.Input(PlatformType.Facebook, PlatformType.Instagram));
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await using var otherDb = new AppDbContext(options);
                var other = GeneratePlatformPosts.Service(otherDb, AiContentClient.Client(handler));
                await Assert.ThrowsAsync<ConflictException>(() => other.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook)));
                var core = new HoaTheoMua.Service.PostCore.Service(otherDb);
                await Assert.ThrowsAsync<ConflictException>(() => core.DeletePostCore(id));
                await Assert.ThrowsAsync<ConflictException>(() => core.UpdatePostCore(id, new() { Title = "Changed", Content = "Changed", Images = new() }));
                Assert.Equal(1, handler.Calls);
            }
            finally { release.TrySetResult(); }
            await generation;
            Assert.Equal(2, await db.GeneratedPosts.CountAsync());
            var officialBefore = await db.PlatformPosts.AsNoTracking().OrderBy(x => x.Id).Select(x => x.OfficialGenerationId).ToListAsync();

            var failureOptions = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).AddInterceptors(new RejectOfficialWrite()).Options;
            await using var failingDb = new AppDbContext(failureOptions);
            var successHandler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
            await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failingDb, AiContentClient.Client(successHandler))
                .GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook, PlatformType.Instagram)));
            Assert.Equal(2, await db.GeneratedPosts.CountAsync()); // New versions rolled back with failed official update.
            Assert.Equal(officialBefore, await db.PlatformPosts.AsNoTracking().OrderBy(x => x.Id).Select(x => x.OfficialGenerationId).ToListAsync());
            Assert.All(await db.PlatformPosts.AsNoTracking().ToListAsync(), x => Assert.Equal(PlatformPostStatus.Failed, x.Status));
            Assert.False(await db.PostHistories.AnyAsync()); Assert.False(await db.ClientHistories.AnyAsync());
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }

    private sealed class RejectOfficialWrite : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<HoaTheoMua.Repository.Entity.PlatformPost>()
                .Any(x => x.State == EntityState.Modified && x.Entity.Status == PlatformPostStatus.Generated))
                throw new InvalidOperationException("Simulated official pointer write failure");
            return ValueTask.FromResult(result);
        }
    }
}
