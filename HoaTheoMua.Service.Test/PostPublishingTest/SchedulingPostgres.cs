using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostPublishing;
using HoaTheoMua.Service.Test.PostGenTest;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;
using Response = HoaTheoMua.Service.PostPublishing.Response;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class SchedulingPostgres
{
    [PostGenPostgresFact]
    public async Task UpgradeExistingHistoryConcurrentDispatchRescheduleCancelAndClaim()
    {
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(connection.Host, new[] { "localhost", "127.0.0.1" });
        var database = "schedule_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        connection.Database = database;
        try
        {
            var sourceBuilder = new NpgsqlDataSourceBuilder(connection.ConnectionString); sourceBuilder.EnableDynamicJson();
            sourceBuilder.MapEnum<PlatformType>("PlatformType"); sourceBuilder.MapEnum<SystemPromptType>("SystemPromptType");
            sourceBuilder.MapEnum<CardFormType>("CardFormType"); sourceBuilder.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = sourceBuilder.Build(); var opts = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options;
            await using var db = new AppDbContext(opts);
            await db.GetService<IMigrator>().MigrateAsync("20260915120000_AddPostGenPublications");
            await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            var (oldPost, oldAccount) = await Lifecycle.Seed(db);
            var legacy = new PostPublication { Id = Guid.NewGuid(), BasePostId = oldPost, PlatformPostId = (await db.PlatformPosts.SingleAsync()).Id,
                PlatformAccountId = oldAccount, RequestedBy = Guid.NewGuid(), IdempotencyKey = "legacy", RequestHash = "legacy-hash", Snapshot = "{}", Status = "Cancelled",
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"PostPublication\" SELECT * FROM jsonb_populate_record(NULL::\"PostPublication\", {Store.Json(legacy)}::jsonb)");
            await db.Database.MigrateAsync();
            Assert.Null((await db.PostPublications.SingleAsync()).ScheduledAt);
            Assert.Equal("Cancelled", (await db.PostPublications.SingleAsync()).Status);
            await db.GetService<IMigrator>().MigrateAsync("20260915120000_AddPostGenPublications"); // Safe with immediate-publication history only.
            await db.Database.MigrateAsync();

            var services = new ServiceCollection(); services.AddLogging(); services.AddDbContext<AppDbContext>(b => b.UseNpgsql(source));
            services.AddMassTransit(c => { c.AddEntityFrameworkOutbox<AppDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); }); c.UsingInMemory(); });
            await using var provider = services.BuildServiceProvider();
            var clock = new Lifecycle.Clock(); var (post, account) = await Lifecycle.Seed(db);
            async Task<Response.Job> Schedule()
            {
                await using var scope = provider.CreateAsyncScope(); var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var setup = Lifecycle.Create(context, clock: clock, endpoint: scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
                return await setup.Service.Schedule(post, "facebook", await Scheduling.Input(setup, post, account));
            }
            var duplicates = await Task.WhenAll(Schedule(), Schedule()); Assert.Equal(duplicates[0].PublicationId, duplicates[1].PublicationId);
            var job = duplicates[0]; Assert.Equal(0, await db.Set<OutboxMessage>().CountAsync());
            var before = (await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == job.PublicationId)).Snapshot;
            await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PostPublication\" SET \"NextRunAt\" = NULL WHERE \"Id\" = {job.PublicationId}"));
            await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync("20260915120000_AddPostGenPublications"));
            clock.Now = job.ScheduledAt!.Value;
            async Task<int> Dispatch()
            {
                await using var scope = provider.CreateAsyncScope();
                return await new Recovery(scope.ServiceProvider.GetRequiredService<AppDbContext>(), scope.ServiceProvider.GetRequiredService<IPublishEndpoint>(), clock).Dispatch(default);
            }
            var sent = await Task.WhenAll(Dispatch(), Dispatch()); Assert.Equal(1, sent.Sum());
            Assert.Equal(1, await db.Set<OutboxMessage>().CountAsync());
            var s = Lifecycle.Create(db, clock: clock);
            var moved = await s.Service.Reschedule(post, "facebook", new() { PublicationId = job.PublicationId, ExpectedRevision = job.Revision, ScheduledAt = clock.Now.AddHours(1).ToString("O") });
            await using (var restarted = new AppDbContext(opts))
            {
                var setup = Lifecycle.Create(restarted, clock: clock); await setup.Worker.Process(job.PublicationId, job.Revision, default);
                Assert.Equal(0, setup.Adapter.Calls); Assert.Equal("Scheduled", (await setup.Service.Get(job.PublicationId)).Job.Status);
            }
            Assert.Equal(before, (await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == job.PublicationId)).Snapshot);
            Assert.Equal(0, await Dispatch());
            await s.Service.CancelSchedule(post, "facebook", new() { PublicationId = job.PublicationId, ExpectedRevision = moved.Revision });
            await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PostPublication\" SET \"ScheduledAt\" = {clock.Now.AddDays(1)} WHERE \"Id\" = {job.PublicationId}"));

            // Claim wins: concurrent workers get one lease; change/cancel is rejected while I/O is pending.
            var (secondPost, secondAccount) = await Lifecycle.Seed(db);
            var second = await s.Service.Schedule(secondPost, "facebook", await Scheduling.Input(s, secondPost, secondAccount)); clock.Now = second.ScheduledAt!.Value;
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var adapter = new Lifecycle.Adapter { Handle = async (_, _, _) => { entered.TrySetResult(); await release.Task.WaitAsync(TimeSpan.FromSeconds(15)); return new(PublicationOutcome.Continue) { Checkpoint = new() { ReadyToPublish = true } }; } };
            await using var workerDb = new AppDbContext(opts); var worker = Lifecycle.Create(workerDb, adapter, clock);
            var running = worker.Worker.Process(second.PublicationId, second.Revision, default);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
            try
            {
                await using var rivalDb = new AppDbContext(opts); var rival = Lifecycle.Create(rivalDb, adapter, clock);
                await rival.Worker.Process(second.PublicationId, second.Revision, default);
                var latest = (await rival.Service.Get(second.PublicationId)).Job;
                await Assert.ThrowsAsync<PublicationException>(() => rival.Service.CancelSchedule(secondPost, "facebook", new() { PublicationId = second.PublicationId, ExpectedRevision = latest.Revision }));
                await Assert.ThrowsAsync<PublicationException>(() => rival.Service.Reschedule(secondPost, "facebook", new() { PublicationId = second.PublicationId, ExpectedRevision = latest.Revision, ScheduledAt = clock.Now.AddHours(1).ToString("O") }));
            }
            finally { release.TrySetResult(); await running; }
            Assert.Equal(1, adapter.Calls);

            // Simultaneous cancel vs claim can have either winner, but cannot publish a cancelled job.
            var (thirdPost, thirdAccount) = await Lifecycle.Seed(db);
            var third = await s.Service.Schedule(thirdPost, "facebook", await Scheduling.Input(s, thirdPost, thirdAccount)); clock.Now = third.ScheduledAt!.Value;
            var raceAdapter = new Lifecycle.Adapter();
            async Task Cancel()
            {
                await using var context = new AppDbContext(opts); var setup = Lifecycle.Create(context, raceAdapter, clock);
                try { await setup.Service.CancelSchedule(thirdPost, "facebook", new() { PublicationId = third.PublicationId, ExpectedRevision = third.Revision }); }
                catch (PublicationException ex) { Assert.Equal(409, ex.StatusCode); }
            }
            async Task Claim()
            {
                await using var context = new AppDbContext(opts); await Lifecycle.Create(context, raceAdapter, clock).Worker.Process(third.PublicationId, third.Revision, default);
            }
            await Task.WhenAll(Cancel(), Claim());
            var final = await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == third.PublicationId);
            Assert.Equal(final.Status == "Cancelled" ? 0 : 1, raceAdapter.Calls);
            Assert.Contains(final.Status, new[] { "Cancelled", "Publishing" });
            Assert.Equal(0, raceAdapter.Publishes);
        }
        finally { await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync(); }
    }
}
