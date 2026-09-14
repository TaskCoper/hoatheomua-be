using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostPublishing;
using HoaTheoMua.Service.Test.PostGenTest;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Response = HoaTheoMua.Service.PostPublishing.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class Postgres
{
    [PostGenPostgresFact]
    public async Task MigrationConcurrencyImmutableSnapshotsOutboxRollbackAndCrashRecovery()
    {
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(connection.Host, new[] { "localhost", "127.0.0.1" });
        var database = "publication_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        connection.Database = database;
        try
        {
            var builder = new NpgsqlDataSourceBuilder(connection.ConnectionString); builder.EnableDynamicJson();
            builder.MapEnum<PlatformType>("PlatformType"); builder.MapEnum<SystemPromptType>("SystemPromptType");
            builder.MapEnum<CardFormType>("CardFormType"); builder.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = builder.Build();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options;
            await using var db = new AppDbContext(options); await db.Database.MigrateAsync();
            await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            // Empty downgrade is safe; used-history downgrade must be rejected below.
            await db.GetService<IMigrator>().MigrateAsync("20260914120000_AddPostGenPlatformAccounts");
            await db.Database.MigrateAsync();
            var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db); var input = await Lifecycle.Input(s, id, account);

            // Same idempotency key, two real connections, one job. Separate keys cannot both win either.
            async Task<Response.Job> Publish()
            {
                await using var other = new AppDbContext(options);
                return await Lifecycle.Create(other).Service.Publish(id, "facebook", input);
            }
            var both = await Task.WhenAll(Publish(), Publish()); Assert.Equal(both[0].PublicationId, both[1].PublicationId);
            Assert.Equal(1, await db.PostPublications.CountAsync()); var jobId = both[0].PublicationId;
            var snapshotBefore = (await db.PostPublications.AsNoTracking().SingleAsync()).Snapshot;
            var badUpdate = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync("UPDATE \"PostPublication\" SET \"Snapshot\" = '{{}}'::jsonb"));
            Assert.Equal(PostgresErrorCodes.CheckViolation, badUpdate.SqlState);
            Assert.Equal(snapshotBefore, (await db.PostPublications.AsNoTracking().SingleAsync()).Snapshot);
            await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync("20260914120000_AddPostGenPlatformAccounts"));
            // Later reversible migrations may already have rolled back before M12 refuses history removal.
            await db.Database.MigrateAsync();
            Assert.Contains("20260915120000_AddPostGenPublications", await db.Database.GetAppliedMigrationsAsync());

            // Partial unique index protects active work even from a writer bypassing the service.
            var original = await db.PostPublications.AsNoTracking().SingleAsync();
            var duplicate = Store.Read<PostPublication>(Store.Json(original)); duplicate.Id = Guid.NewGuid(); duplicate.IdempotencyKey = "second-key";
            db.PostPublications.Add(duplicate);
            var unique = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(unique.InnerException).SqlState);
            db.Entry(duplicate).State = EntityState.Detached;

            // Cancellation wins over the old message, and queued locks disappear.
            await s.Service.Cancel(jobId, new() { ExpectedRevision = both[0].Revision });
            await s.Worker.Process(jobId, both[0].Revision, default); Assert.Equal(0, s.Adapter.Calls);
            await new HoaTheoMua.Service.PostCore.Service(db).UpdatePostCore(id, new() { Title = "Updated source", Content = "Updated source", Images = [] });

            // Actual scoped bus outbox, without starting a broker: rows must commit with the job.
            var intercept = new FailAfterSave();
            var services = new ServiceCollection(); services.AddLogging();
            services.AddDbContext<AppDbContext>(b => b.UseNpgsql(source).AddInterceptors(intercept));
            services.AddMassTransit(c => { c.AddEntityFrameworkOutbox<AppDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); }); c.UsingInMemory(); });
            await using var provider = services.BuildServiceProvider();
            (Guid PostId, Guid AccountId) second;
            await using (var seedScope = provider.CreateAsyncScope()) second = await Lifecycle.Seed(seedScope.ServiceProvider.GetRequiredService<AppDbContext>());
            async Task<Response.Job> OutboxPublish(bool fail)
            {
                await using var scope = provider.CreateAsyncScope(); var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var setup = Lifecycle.Create(scopedDb, endpoint: scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
                var request = await Lifecycle.Input(setup, second.PostId, second.AccountId);
                intercept.Enabled = fail;
                return await setup.Service.Publish(second.PostId, "facebook", request);
            }
            await Assert.ThrowsAsync<InvalidOperationException>(() => OutboxPublish(true)); intercept.Enabled = false;
            Assert.False(await db.PostPublications.AnyAsync(x => x.BasePostId == second.PostId));
            Assert.Equal(0, await db.Set<OutboxMessage>().CountAsync());
            var committed = await OutboxPublish(false);
            Assert.Equal(1, await db.Set<OutboxMessage>().CountAsync()); Assert.Equal(1, await db.Set<OutboxState>().CountAsync());

            // Only one worker may claim the same revision while HTTP is in flight.
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var adapter = new Lifecycle.Adapter(); var clock = new Lifecycle.Clock();
            adapter.Handle = async (op, snapshot, checkpoint) =>
            {
                Assert.Null(db.Database.CurrentTransaction);
                entered.SetResult(); await release.Task.WaitAsync(TimeSpan.FromSeconds(15));
                return new(PublicationOutcome.Continue) { Checkpoint = new() { ReadyToPublish = true }, RetryAfterSeconds = 1 };
            };
            await using var workerDb = new AppDbContext(options); var worker = Lifecycle.Create(workerDb, adapter, clock);
            var pending = worker.Worker.Process(committed.PublicationId, committed.Revision, default);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
            await using (var rival = new AppDbContext(options))
            {
                var competitor = Lifecycle.Create(rival, adapter, clock);
                await competitor.Worker.Process(committed.PublicationId, committed.Revision, default);
                var claimed = (await competitor.Service.Get(committed.PublicationId)).Job;
                await Assert.ThrowsAsync<PublicationException>(() => competitor.Service.Cancel(committed.PublicationId,
                    new() { ExpectedRevision = claimed.Revision }));
            }
            release.SetResult(); await pending; Assert.Equal(1, adapter.Calls);

            // Simulate lost acknowledgement after publish: durable intent survives a new process.
            var prepared = await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == committed.PublicationId);
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PostPublication\" SET \"PublishMayHaveOccurred\" = true, \"LeaseId\" = {Guid.NewGuid()}, \"LeaseUntil\" = {clock.Now.AddSeconds(-1)}, \"NextRunAt\" = {clock.Now} WHERE \"Id\" = {committed.PublicationId}");
            await using (var restart = new AppDbContext(options)) await Lifecycle.Create(restart, adapter, clock).Worker.Process(committed.PublicationId, prepared.Revision, default);
            var uncertain = await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == committed.PublicationId);
            Assert.Equal("NeedsReview", uncertain.Status); Assert.False(uncertain.CanRetry); Assert.Equal(0, adapter.Publishes);

            // Compare frozen migration metadata to the runtime schema for the newly added entities.
            var snapshotModel = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
            foreach (var type in new[] { typeof(PostPublication), typeof(PostPublicationAttempt), typeof(PostPublicationReview) })
            {
                var entity = db.Model.FindEntityType(type)!; var snap = snapshotModel.FindEntityType(type.FullName!)!;
                Assert.Equal(entity.GetProperties().Select(x => (x.Name, x.GetColumnType(), x.IsNullable)).OrderBy(x => x.Name),
                    snap.GetProperties().Select(x => (x.Name, x.GetColumnType(), x.IsNullable)).OrderBy(x => x.Name));
                Assert.Equal(entity.GetIndexes().Select(x => x.GetDatabaseName()).Order(), snap.GetIndexes().Select(x => x.GetDatabaseName()).Order());
            }
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
    private sealed class FailAfterSave : SaveChangesInterceptor
    {
        public bool Enabled;
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (Enabled) throw new InvalidOperationException("Failure after SQL, before transaction commit");
            return new(result);
        }
    }
}
