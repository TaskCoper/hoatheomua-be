using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Xunit;
namespace HoaTheoMua.Service.Test.PostGenTest;

public class ImageVersionsPostgres
{
    [PostGenPostgresFact]
    public async Task ConcurrentRegenerationKeepsNumbersStableAndSelectionIsAtomicWithDeletionRecheck()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "image_versions_test_" + Guid.NewGuid().ToString("N");
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
                return GenerateImages.IsImage(body.RootElement) ? AiContentClient.Completion("![image](https://example.com/out.png)") : AiContentClient.Completion();
            });
            var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
            await service.GeneratePlatformPosts(id, GenerateImages.Input(new[] { sourceId }, PlatformType.Facebook, PlatformType.Instagram));
            var original = await db.GeneratedPostImages.SingleAsync();
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var slow = new AiContentClient.Handler(async (_, ct) => { entered.TrySetResult(); await release.Task.WaitAsync(ct); return AiContentClient.Completion("![image](https://example.com/slow.png)"); });
            await using var slowDb = new AppDbContext(options);
            var request = new HoaTheoMua.Service.PostGeneration.Request.RegenerateImageRequest { SourceBasePostImageId = sourceId, RatioCode = original.RatioCode, SourceGeneratedPostImageId = original.Id };
            var pending = GeneratePlatformPosts.Service(slowDb, AiContentClient.Client(slow)).RegenerateImage(id, request);
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await using var other = new AppDbContext(options);
                var otherService = GeneratePlatformPosts.Service(other, AiContentClient.Client(handler));
                var faster = await otherService.RegenerateImage(id, request);
                Assert.Equal(2, faster.VersionNo);
                await otherService.UpdatePlatformImages(id, "facebook", UpdatePlatformImages.Select(original));
                // Generate can finish between regenerations without renumbering the returned v2.
                var imageOnly = GenerateImages.Input(new[] { sourceId }, PlatformType.Facebook); imageOnly.Mode = "images";
                await otherService.GeneratePlatformPosts(id, imageOnly);
            }
            finally { release.TrySetResult(); }
            var slower = await pending; Assert.Equal(4, slower.VersionNo);
            db.ChangeTracker.Clear();
            var history = await service.GetImageVersions(id);
            Assert.Equal(new[] { 1, 2, 3, 4 }, history.Groups.Single().Versions.Select(x => x.VersionNo));
            Assert.Equal("https://example.com/slow.png", history.Groups.Single().Versions[3].ImageUrl);
            Assert.All(await db.PlatformPostImages.ToListAsync(), x => Assert.Equal(original.Id, x.SelectedGeneratedPostImageId));
            var newImage = await db.GeneratedPostImages.SingleAsync(x => x.Id == slower.GeneratedPostImageId);
            var another = await service.RegenerateImage(id, new() { SourceBasePostImageId = sourceId, RatioCode = AspectRatioType.Square1x1 });
            var square = await db.GeneratedPostImages.SingleAsync(x => x.Id == another.GeneratedPostImageId);
            await service.UpdatePlatformImages(id, "facebook", UpdatePlatformImages.Select(newImage, square));
            var platform = await db.PlatformPosts.AsNoTracking().SingleAsync(x => x.Platform == PlatformType.Facebook);
            var slotsBefore = JsonSerializer.Serialize(await db.PlatformPostImages.AsNoTracking().Where(x => x.PlatformPostId == platform.Id).OrderBy(x => x.Id).ToListAsync());
            var rejectOptions = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).AddInterceptors(new RejectSlots()).Options;
            await using (var failure = new AppDbContext(rejectOptions))
                await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failure, AiContentClient.Client(handler)).UpdatePlatformImages(id, "facebook", UpdatePlatformImages.Select(original)));
            Assert.Equal(slotsBefore, JsonSerializer.Serialize(await db.PlatformPostImages.AsNoTracking().Where(x => x.PlatformPostId == platform.Id).OrderBy(x => x.Id).ToListAsync()));
            Assert.Equal(platform.SelectedAt, (await db.PlatformPosts.AsNoTracking().SingleAsync(x => x.Id == platform.Id)).SelectedAt);
            async Task Choose(params GeneratedPostImage[] images)
            {
                await using var context = new AppDbContext(options);
                await GeneratePlatformPosts.Service(context, AiContentClient.Client(handler)).UpdatePlatformImages(id, "facebook", UpdatePlatformImages.Select(images));
            }
            await Task.WhenAll(Choose(newImage, square), Choose(square, original));
            var final = await db.PlatformPostImages.AsNoTracking().Where(x => x.PlatformPostId == platform.Id).OrderBy(x => x.SortOrder).ToListAsync();
            var selected = final.Select(x => x.SelectedGeneratedPostImageId).ToArray();
            Assert.True(selected.SequenceEqual(new Guid?[] { newImage.Id, square.Id }) || selected.SequenceEqual(new Guid?[] { square.Id, original.Id }));
            Assert.Equal(new[] { 1, 2 }, final.Select(x => x.SortOrder));
            var count = await db.GeneratedPostImages.CountAsync();
            var failedInsertOptions = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).AddInterceptors(new RejectSlots(images: true)).Options;
            await using (var failure = new AppDbContext(failedInsertOptions))
                await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratePlatformPosts.Service(failure, AiContentClient.Client(handler)).RegenerateImage(id, request));
            Assert.Equal(count, await db.GeneratedPostImages.CountAsync());
            var removed = new AiContentClient.Handler(async (_, ct) =>
            {
                await using var other = new AppDbContext(options);
                await new HoaTheoMua.Service.PostCore.Service(other).UpdatePostCore(id, new() { Title = "Text remains", Content = "Source removed", Images = new() });
                return AiContentClient.Completion("![image](https://example.com/discard.png)");
            });
            await using (var changed = new AppDbContext(options))
                await Assert.ThrowsAsync<BadRequestException>(() => GeneratePlatformPosts.Service(changed, AiContentClient.Client(removed)).RegenerateImage(id, request));
            Assert.Equal(count, await db.GeneratedPostImages.CountAsync());
            Assert.All((await service.GetImageVersions(id)).Groups, x => Assert.True(x.SourceIsDeleted));
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
    private sealed class RejectSlots(bool images = false) : SaveChangesInterceptor
    {
        private bool reject;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            reject = images
                ? eventData.Context!.ChangeTracker.Entries<GeneratedPostImage>().Any(x => x.State == EntityState.Added)
                : eventData.Context!.ChangeTracker.Entries<PlatformPostImage>().Any(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
            return ValueTask.FromResult(result);
        }
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (reject) throw new InvalidOperationException("Simulated failure after SQL writes, before commit");
            return ValueTask.FromResult(result);
        }
    }
}
