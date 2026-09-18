using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.Test.PostGenTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Moq;
using Npgsql;
using Xunit;
using AccountService = HoaTheoMua.Service.PlatformAccount.Service;

namespace HoaTheoMua.Service.Test.PlatformAccountTest;

public class AccountPostgres
{
    [PostGenPostgresFact]
    public async Task MigrationCallbackRefreshConcurrencyAndConstraintsWorkOnPostgres()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var name = "platform_account_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = name;
        try
        {
            var builder = new NpgsqlDataSourceBuilder(settings.ConnectionString); builder.EnableDynamicJson();
            builder.MapEnum<PlatformType>("PlatformType"); builder.MapEnum<SystemPromptType>("SystemPromptType");
            builder.MapEnum<CardFormType>("CardFormType"); builder.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = builder.Build();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();
            await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            Assert.Contains("20260914120000_AddPostGenPlatformAccounts", await db.Database.GetAppliedMigrationsAsync());
            var clock = new AccountLifecycle.Clock(); var provider = AccountLifecycle.Provider();
            var svc = AccountLifecycle.Create(db, provider.Object, clock);
            var proof = AccountService.Random(); var session = await svc.Connect("zalo", proof);
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            provider.Setup(x => x.Exchange(PlatformType.Zalo, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () => { entered.SetResult(); await release.Task.WaitAsync(TimeSpan.FromSeconds(10)); return new List<ProviderAccount> { AccountLifecycle.Candidate() }; });
            var pending = svc.Callback("zalo", AccountLifecycle.State(session), "code", null, proof);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await using (var second = new AppDbContext(options))
            {
                var replay = AccountLifecycle.Create(second, provider.Object, clock);
                Assert.Equal("PLATFORM_OAUTH_STATE_INVALID", (await Assert.ThrowsAsync<AccountException>(() => replay.Callback("zalo", AccountLifecycle.State(session), "code", null, proof))).MessageCode);
                Assert.Empty(await second.PlatformAccounts.ToListAsync());
            }
            release.SetResult(); var account = Assert.Single(await pending);
            Assert.Single(await db.PlatformAccounts.ToListAsync());
            Assert.NotNull((await db.PlatformOAuthSessions.AsNoTracking().SingleAsync()).UsedAt);

            // Two refreshers must not consume the same refresh token. Disconnect wins over the in-flight result.
            var refreshEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var refreshRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            provider.Setup(x => x.Refresh(PlatformType.Zalo, "page", "page", It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
                .Returns(async () => { refreshEntered.SetResult(); await refreshRelease.Task.WaitAsync(TimeSpan.FromSeconds(10)); return AccountLifecycle.Candidate(); });
            var pendingRefresh = svc.Refresh(account.Id);
            await refreshEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await using (var other = new AppDbContext(options))
            {
                var otherSvc = AccountLifecycle.Create(other, provider.Object, clock);
                Assert.Equal("PLATFORM_REFRESH_IN_PROGRESS", (await Assert.ThrowsAsync<AccountException>(() => otherSvc.Refresh(account.Id))).MessageCode);
                Assert.Equal("Disabled", (await otherSvc.Disconnect(account.Id)).Status);
            }
            refreshRelease.SetResult();
            Assert.Equal("PLATFORM_ACCOUNT_CHANGED", (await Assert.ThrowsAsync<AccountException>(() => pendingRefresh)).MessageCode);
            var disabled = await db.PlatformAccounts.AsNoTracking().SingleAsync();
            Assert.Equal("Disabled", disabled.Status); Assert.Null(disabled.ProtectedCredentials);

            // Reconnect restores the same row and a fresh token pair.
            provider = AccountLifecycle.Provider(); svc = AccountLifecycle.Create(db, provider.Object, clock);
            var reconnect = await svc.Reconnect(account.Id, proof);
            Assert.Equal(account.Id, Assert.Single(await svc.Callback("zalo", AccountLifecycle.State(reconnect), "new-code", null, proof)).Id);

            var duplicate = new Repository.Entity.PlatformAccount { Id = Guid.NewGuid(), Platform = PlatformType.Zalo, ExternalAccountId = "page", DisplayName = "duplicate", Status = "Disabled" };
            db.PlatformAccounts.Add(duplicate);
            var uniqueError = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(uniqueError.InnerException).SqlState);
            db.Entry(duplicate).State = EntityState.Detached;

            var post = new BasePost { Id = Guid.NewGuid(), Title = "Account FK", Content = "test" }; db.BasePosts.Add(post); await db.SaveChangesAsync();
            var platformPost = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = PlatformType.Zalo, PlatformAccountId = Guid.NewGuid() }; db.PlatformPosts.Add(platformPost);
            var fk = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, Assert.IsType<PostgresException>(fk.InnerException).SqlState);
            platformPost.PlatformAccountId = account.Id; platformPost.Status = PlatformPostStatus.Scheduled; await db.SaveChangesAsync();
            Assert.Equal("PLATFORM_ACCOUNT_IN_USE", (await Assert.ThrowsAsync<AccountException>(() => svc.Disconnect(account.Id))).MessageCode);
            Assert.Equal("Connected", (await db.PlatformAccounts.AsNoTracking().SingleAsync()).Status);

            // Snapshot contains the same new properties/FK/indexes as the runtime model.
            var snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
            foreach (var type in new[] { typeof(Repository.Entity.PlatformAccount), typeof(PlatformOAuthSession) })
            {
                var entity = db.Model.FindEntityType(type)!; var snap = snapshot.FindEntityType(type.FullName!)!;
                Assert.Equal(entity.GetProperties().Select(x => x.Name).Order(), snap.GetProperties().Select(x => x.Name).Order());
                Assert.Equal(entity.GetIndexes().Select(x => x.GetDatabaseName()).Order(), snap.GetIndexes().Select(x => x.GetDatabaseName()).Order());
            }

            // Downgrade/upgrade and legacy reserved-ID preflight: failure must leave data and migration state intact.
            platformPost.PlatformAccountId = null; platformPost.Status = PlatformPostStatus.Ready; await db.SaveChangesAsync();
            await db.GetService<IMigrator>().MigrateAsync("20260914073414_ConsolidatePostGenPlatformConfigs");
            await using (var orphan = new NpgsqlCommand("UPDATE \"PlatformPost\" SET \"PlatformAccountId\" = @id", (NpgsqlConnection)db.Database.GetDbConnection()))
            { orphan.Parameters.AddWithValue("id", account.Id); await orphan.ExecuteNonQueryAsync(); }
            await Assert.ThrowsAsync<PostgresException>(() => db.Database.MigrateAsync());
            Assert.DoesNotContain("20260914120000_AddPostGenPlatformAccounts", await db.Database.GetAppliedMigrationsAsync());
            await db.Database.ExecuteSqlRawAsync("UPDATE \"PlatformPost\" SET \"PlatformAccountId\" = NULL");
            await db.Database.MigrateAsync();
            Assert.Empty(await db.PlatformAccounts.AsNoTracking().ToListAsync());
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{name}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
        await using var remaining = new NpgsqlCommand("SELECT count(*) FROM pg_database WHERE datname = @name", admin);
        remaining.Parameters.AddWithValue("name", name); Assert.Equal(0L, await remaining.ExecuteScalarAsync());
    }
}
