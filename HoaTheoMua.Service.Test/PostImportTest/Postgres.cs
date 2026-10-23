using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostImport;
using HoaTheoMua.Service.Test.PostGenTest;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostImportTest;

public class Postgres
{
    [PostGenPostgresFact]
    public async Task MigrationConcurrentUploadsAndWorkersPersistExactlyOnePostJobAndOutboxMessage()
    {
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(connection.Host, new[] { "localhost", "127.0.0.1" });
        var database = "bulk_import_test_" + Guid.NewGuid().ToString("N");
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
            var actor = Guid.NewGuid(); var setup = Imports.Create(db, actor);
            var accountId = await Imports.Account(db, setup, PlatformType.Zalo);
            await setup.Service.SaveSettings(new() { Accounts = [new() { Platform = "zalo", AccountId = accountId }] });
            var bytes = await Imports.Book();
            async Task<Guid> Upload(string key)
            {
                await using var other = new AppDbContext(options);
                return (await Imports.Upload(Imports.Create(other, actor), bytes, key)).Id;
            }
            var duplicates = await Task.WhenAll(Upload("same-request"), Upload("same-request"));
            Assert.Equal(duplicates[0], duplicates[1]); Assert.Equal(1, await db.PostImportBatches.CountAsync());
            var secondBatch = await Upload("different-request-same-post");

            var services = new ServiceCollection(); services.AddLogging();
            services.AddDbContext<AppDbContext>(b => b.UseNpgsql(source));
            services.AddMassTransit(c =>
            {
                c.AddEntityFrameworkOutbox<AppDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
                c.UsingInMemory();
            });
            await using var provider = services.BuildServiceProvider();
            async Task<bool> Process(Guid batchId)
            {
                await using var scope = provider.CreateAsyncScope();
                var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await Imports.Create(scopedDb, actor, scope.ServiceProvider.GetRequiredService<IPublishEndpoint>()).Worker.ProcessOne(batchId);
            }
            await Task.WhenAll(Process(duplicates[0]), Process(secondBatch));
            Assert.Equal(1, await db.BasePosts.CountAsync()); Assert.Equal(1, await db.ImportedPostIdentities.CountAsync());
            Assert.Equal(1, await db.PlatformPosts.CountAsync()); Assert.Equal(1, await db.PostPublications.CountAsync());
            Assert.Equal(1, await db.Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>().CountAsync());
            Assert.Equal(2, await db.PostImportChannels.CountAsync(x => x.PublicationId != null));
            var publication = await db.PostPublications.AsNoTracking().SingleAsync();
            Assert.Equal("Queued", publication.Status); Assert.Equal(actor, publication.RequestedBy);
            Assert.Equal(accountId, publication.PlatformAccountId);
            Assert.Equal(1, (await setup.Service.Get(secondBatch)).Batch.Existing + (await setup.Service.Get(duplicates[0])).Batch.Existing);

            // A completed upload can replay safely without more outbox rows.
            Assert.Equal(duplicates[0], await Upload("same-request"));
            Assert.Equal(1, await db.Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>().CountAsync());
            var rollback = await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync("20261014164002_add_filter_isDeletd=false_orderNhanhVn"));
            Assert.Contains("Cannot remove bulk import history", rollback.MessageText);
            Assert.Equal(2, await db.PostImportBatches.CountAsync());
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
