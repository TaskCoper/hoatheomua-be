using System.Data;
using System.Data.Common;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class OfficialVersionPostgres
{
    [PostGenPostgresFact]
    public async Task ReadsOneSnapshotWhenManualContentAndImageSelectionChangeBetweenQueries()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "official_version_test_" + Guid.NewGuid().ToString("N");
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
            var sourceId = await db.BasePostImages.Select(x => x.Id).SingleAsync();
            var handler = new AiContentClient.Handler(async (request, ct) =>
            {
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
                return GenerateImages.IsImage(body.RootElement) ? AiContentClient.Completion("![image](https://example.com/image.png)") : AiContentClient.Completion();
            });
            var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
            await service.GeneratePlatformPosts(id, GenerateImages.Input(new[] { sourceId }, PlatformType.Facebook));
            await service.EditPlatformContent(id, "facebook", new() { Title = "Before", Content = "Manual before concurrent update", Hashtag = "#Before" });
            var alternative = await service.RegenerateImage(id, new() { SourceBasePostImageId = sourceId, RatioCode = AspectRatioType.Square1x1 });
            var newImage = await db.GeneratedPostImages.SingleAsync(x => x.Id == alternative.GeneratedPostImageId);
            var before = await service.GetOfficialVersion(id, "facebook");
            var calls = handler.Calls;
            var interceptor = new InterleavedSelection(async () =>
            {
                await using var other = new AppDbContext(options);
                var otherService = GeneratePlatformPosts.Service(other, AiContentClient.Client(handler));
                await otherService.EditPlatformContent(id, "facebook", new() { Title = "After", Content = "Manual after concurrent update", Hashtag = "#After" });
                await otherService.UpdatePlatformImages(id, "facebook", UpdatePlatformImages.Select(newImage));
            });
            var readOptions = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).AddInterceptors(interceptor, new RejectReadWrites()).Options;
            await using var reader = new AppDbContext(readOptions);
            var during = await GeneratePlatformPosts.Service(reader, AiContentClient.Client(handler)).GetOfficialVersion(id, "facebook");
            Assert.True(interceptor.Changed); Assert.Equal(4, interceptor.QueryCount);
            Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(during));
            Assert.Empty(reader.ChangeTracker.Entries());
            var after = await service.GetOfficialVersion(id, "facebook");
            Assert.Equal(before.GenerationId, after.GenerationId); // Same coalesced Manual row, different committed text.
            Assert.Equal("Manual after concurrent update", after.Content);
            Assert.Equal("After", after.Title); Assert.Equal("#After", after.Hashtag);
            Assert.Equal(newImage.Id, Assert.Single(after.Images).SelectedGeneratedPostImageId);
            Assert.True(after.IsReady);
            Assert.Equal(calls, handler.Calls); Assert.Equal(2, await db.GeneratedPosts.CountAsync());
            Assert.Equal(2, await db.GeneratedPostImages.CountAsync());
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
    private sealed class InterleavedSelection(Func<Task> change) : DbCommandInterceptor
    {
        public bool Changed { get; private set; }
        public int QueryCount { get; private set; }
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            QueryCount++;
            Assert.Equal(IsolationLevel.RepeatableRead, eventData.Context!.Database.CurrentTransaction!.GetDbTransaction().IsolationLevel);
            if (!Changed && command.CommandText.Contains("FROM \"PlatformPost\"")) { Changed = true; await change(); }
            return result;
        }
    }
    private sealed class RejectReadWrites : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
            => throw new Exception("Official GET must never write");
    }
}
