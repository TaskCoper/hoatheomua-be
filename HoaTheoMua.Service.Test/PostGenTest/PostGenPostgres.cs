using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using Npgsql;
using Xunit;
using Core = HoaTheoMua.Service.PostCore;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class PostGenPostgres
{
    // A fresh, uniquely named database is created and dropped for this test.
    // Supply a connection to a disposable PostgreSQL server, never a deployed database.
    [PostGenPostgresFact]
    public async Task MigrationSeedsRollbackAndConstraintsWorkOnPostgres()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "postgen_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString);
        await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = database;
        try
        {
            var sourceBuilder = new NpgsqlDataSourceBuilder(settings.ConnectionString);
            sourceBuilder.EnableDynamicJson();
            sourceBuilder.MapEnum<PlatformType>("PlatformType");
            sourceBuilder.MapEnum<SystemPromptType>("SystemPromptType");
            sourceBuilder.MapEnum<ClientHistoryType>("ClientHistoryType");
            sourceBuilder.MapEnum<CardFormType>("CardFormType");
            await using var source = sourceBuilder.Build();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options;
            await using var db = new AppDbContext(options);
            var migrator = db.GetService<IMigrator>();
            const string previous = "20260909161228_UPDBV2";
            var current = db.Database.GetMigrations().Last();
            var sql = migrator.GenerateScript(previous, current);
            Assert.DoesNotContain("ALTER TYPE", sql, StringComparison.OrdinalIgnoreCase);
            await migrator.MigrateAsync(previous);
            var legacyId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"BasePost\" (\"Id\", \"Content\", \"ImageUrl\") VALUES ({legacyId}, 'legacy', 'https://example.com/old.jpg')");
            var error = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync(current));
            Assert.Contains("backfill existing data first", error.MessageText);
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"BasePost\" WHERE \"Id\" = {legacyId}");
            var promptId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"SystemPrompt\" (\"Id\", \"Type\", \"Content\", \"IsDeleted\", \"CreatedAt\") VALUES ({promptId}, 'post', 'Existing custom prompt', false, now())");
            await migrator.MigrateAsync("20260914065606_AddPostGenSystemPromptConfigs");
            await migrator.MigrateAsync(previous);
            await migrator.MigrateAsync(current);
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Null(db.Model.FindEntityType(typeof(BasePost))!.FindProperty("UserId"));
            await db.Database.OpenConnectionAsync();
            await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
            Assert.Equal("Existing custom prompt", (await db.SystemPrompts.SingleAsync(x => x.Type == SystemPromptType.Post)).Content);
            Assert.Equal(0, await db.Configs.CountAsync(x => x.Group == "postgen" && x.Key.StartsWith("postgen.platform.")));
            Assert.Null(db.Model.FindEntityType("HoaTheoMua.Repository.Entity.PlatformStandard"));
            Assert.Equal(5, await db.Configs.CountAsync(x => x.Group == "postgen" && x.Key != "postgen.ratio-preset"));
            Assert.False(await db.GeneratedCards.AnyAsync(x => x.FormType == CardFormType.Calligraphy));
            Assert.False(await db.ClientHistories.AnyAsync(x => x.Type == ClientHistoryType.Post));

            var user = new TestUser(Guid.NewGuid());
            var core = new Core.Service(db);
            var post = await core.CreatePostCore(new()
            {
                Title = "Postgres", Content = "Test",
                Images = new() { new() { ImageUrl = "https://example.com/1.jpg", SortOrder = 1 }, new() { ImageUrl = "https://example.com/2.jpg", SortOrder = 2 } }
            });
            var reordered = await core.UpdatePostCore(post.BasePostId, new()
            {
                Title = "Updated", Content = "Test",
                Images = post.BaseImages.AsEnumerable().Reverse().Select((x, i) => new Core.Request.ImageInput { Id = x.Id, ImageUrl = x.ImageUrl, SortOrder = i + 1 }).ToList()
            });
            Assert.Equal(post.BaseImages[1].Id, reordered.BaseImages[0].Id);
            await core.GetPostCoreList(new() { Platform = PlatformType.Facebook });

            var commands = new DetailCommandCapture();
            var readOptions = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).AddInterceptors(commands).Options;
            await using var readDb = new AppDbContext(readOptions);
            var reader = new Core.Service(readDb);
            Assert.Empty((await reader.GetPostCoreDetail(post.BasePostId)).Platforms);
            Assert.Equal(3, commands.Sql.Count);
            Assert.Empty(readDb.ChangeTracker.Entries());
            commands.Sql.Clear();
            await Assert.ThrowsAsync<HoaTheoMua.Service.Exceptions.NotFoundException>(() => reader.GetPostCoreDetail(Guid.NewGuid()));
            Assert.Single(commands.Sql);

            var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.BasePostId, Platform = PlatformType.Facebook, UserId = user.GetRequiredUserId() };
            db.PlatformPosts.Add(platform);
            await db.SaveChangesAsync();
            var content = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Content = "Generated", Source = ContentSource.Ai, UserId = user.GetRequiredUserId() };
            db.GeneratedPosts.Add(content);
            await db.SaveChangesAsync();
            platform.OfficialGenerationId = content.Id;
            await db.SaveChangesAsync();
            Assert.Equal(content.Id, (await core.GetPostCoreDetail(post.BasePostId)).Platforms.Single().Content!.GenerationId);
            commands.Sql.Clear();
            Assert.Equal(content.Id, (await reader.GetPostCoreDetail(post.BasePostId)).Platforms.Single().Content!.GenerationId);
            Assert.Equal(6, commands.Sql.Count);
            Assert.Empty(readDb.ChangeTracker.Entries());
            var detailSql = string.Join("\n", commands.Sql);
            foreach (var unusedColumn in new[] { "PromptSnapshot", "FeedbackInput", "AiModel", "AiRequestId", "SourceImageUrl", "PublishError", "PublishAttemptCount", "UserId" })
                Assert.DoesNotContain($"\"{unusedColumn}\"", detailSql);
            // A successful generation without a result URL must be rejected by PostgreSQL itself.
            var badImage = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.BasePostId, SourceBasePostImageId = post.BaseImages[0].Id, SourceImageUrl = post.BaseImages[0].ImageUrl, RatioCode = AspectRatioType.Square1x1, Status = ImageStatus.Succeeded, PixelWidth = 1080, PixelHeight = 1080 };
            db.GeneratedPostImages.Add(badImage);
            var invalid = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.CheckViolation, Assert.IsType<PostgresException>(invalid.InnerException).SqlState);
            db.Entry(badImage).State = EntityState.Detached;
            var preset = new Repository.Entity.Config { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.ratio-preset", Kind = "Setting", Value = "{}" };
            db.Configs.Add(preset);
            await db.SaveChangesAsync();
            var duplicatePreset = new Repository.Entity.Config { Id = Guid.NewGuid(), Group = preset.Group, Key = preset.Key, Kind = preset.Kind, Value = "{}" };
            db.Configs.Add(duplicatePreset);
            var duplicate = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(duplicate.InnerException).SqlState);
            db.Entry(duplicatePreset).State = EntityState.Detached;
            await core.DeletePostCore(post.BasePostId);
            Assert.True((await db.BasePosts.SingleAsync()).IsDeleted);
            Assert.True((await db.PlatformPosts.SingleAsync()).IsDeleted);
            Assert.Single(await db.GeneratedPosts.ToListAsync());
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }

    private sealed class DetailCommandCapture : DbCommandInterceptor
    {
        public List<string> Sql { get; } = new();

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Sql.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }

    private sealed class TestUser(Guid id) : ICurrentUserService
    {
        public Guid? UserId => id;
        public string? Email => null;
        public string? FullName => null;
        public string? Avatar => null;
        public bool IsVerified => true;
        public Guid GetRequiredUserId() => id;
    }
}

public sealed class PostGenPostgresFactAttribute : FactAttribute
{
    public PostGenPostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING")))
            Skip = "Set POSTGEN_TEST_CONNECTION_STRING to a disposable local PostgreSQL server to run database integration checks.";
    }
}
