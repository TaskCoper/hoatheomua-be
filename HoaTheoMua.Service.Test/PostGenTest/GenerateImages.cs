using System.Net;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class GenerateImages
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    internal static Gen.Request.GenerateRequest Input(Guid[] images, params PlatformType[] platforms) => new()
    {
        Platforms = platforms.Select(p => new Gen.Request.PlatformInput { Platform = p, Ratios = new()
        { new() { RatioCode = AspectRatioType.Portrait4x5, BasePostImageIds = images.ToList() } } }).ToList()
    };
    internal static async Task<(Guid Id, Guid[] Images)> Seed(AppDbContext db, int count = 3)
    {
        var id = await GeneratePlatformPosts.Seed(db);
        db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.image_instruction", Kind = "Setting", Value = "\"Custom shared instruction\"" });
        for (var i = 1; i < count; i++) db.BasePostImages.Add(new() { Id = Guid.NewGuid(), BasePostId = id, ImageUrl = $"https://example.com/{i}.png", SortOrder = i + 1 });
        await db.SaveChangesAsync();
        return (id, await db.BasePostImages.Where(x => x.BasePostId == id).OrderBy(x => x.SortOrder).Select(x => x.Id).ToArrayAsync());
    }
    internal static bool IsImage(JsonElement body) => body.GetProperty("messages").EnumerateArray()
        .Any(m => m.GetProperty("content").EnumerateArray().Any(p => p.GetProperty("type").GetString() == "image_url"));

    [Fact]
    public async Task DeduplicatesAcrossPlatformsCapsConcurrencyAndKeepsSelectedImagesOnRerun()
    {
        await using var db = Db(); var (id, images) = await Seed(db, 6);
        var calls = 0; var active = 0; var max = 0;
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            if (!IsImage(json.RootElement)) return AiContentClient.Completion();
            var call = Interlocked.Increment(ref calls); var concurrent = Interlocked.Increment(ref active);
            max = Math.Max(max, concurrent);
            try { await Task.Delay(30, ct); return AiContentClient.Completion($"![generated image](https://example.com/out-{call}.png)"); }
            finally { Interlocked.Decrement(ref active); }
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var input = Input(images, PlatformType.Facebook, PlatformType.Instagram);
        var output = await service.GeneratePlatformPosts(id, input);
        Assert.Equal(6, calls); Assert.InRange(max, 2, 3);
        Assert.Equal(6, await db.GeneratedPostImages.CountAsync()); Assert.Equal(12, await db.PlatformPostImages.CountAsync());
        Assert.Equal(output.Platforms[0].Images.Select(x => x.SelectedGeneratedPostImageId), output.Platforms[1].Images.Select(x => x.SelectedGeneratedPostImageId));
        Assert.All(output.Platforms.SelectMany(x => x.Images), x => { Assert.Equal(1080, x.PixelWidth); Assert.Equal(1350, x.PixelHeight); Assert.Contains("out-", x.ImageUrl); });
        var selected = await db.PlatformPostImages.OrderBy(x => x.Id).Select(x => x.SelectedGeneratedPostImageId).ToListAsync();
        await service.GeneratePlatformPosts(id, input);
        Assert.Equal(12, calls); Assert.Equal(12, await db.GeneratedPostImages.CountAsync());
        Assert.Equal(selected, await db.PlatformPostImages.OrderBy(x => x.Id).Select(x => x.SelectedGeneratedPostImageId).ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PartialAndAllImageFailuresPersistAttemptsAndContent(bool failAll)
    {
        await using var db = Db(); var (id, images) = await Seed(db); var calls = 0;
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            if (!IsImage(json.RootElement)) return AiContentClient.Completion();
            var call = Interlocked.Increment(ref calls);
            return failAll || call == 2 ? new(HttpStatusCode.InternalServerError) : AiContentClient.Completion($"![image](https://example.com/out-{call}.png)");
        });
        var output = await GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, Input(images, PlatformType.Facebook));
        Assert.Equal(failAll ? "Failed" : "Partial", output.Platforms[0].Status);
        Assert.NotNull(output.Platforms[0].Content); Assert.NotNull(output.Platforms[0].LastError);
        Assert.Equal(failAll ? 0 : 2, output.Platforms[0].Images.Count);
        Assert.Equal(3, await db.GeneratedPostImages.CountAsync());
        Assert.Equal(failAll ? 3 : 1, await db.GeneratedPostImages.CountAsync(x => x.Status == ImageStatus.Failed && x.ImageUrl == null && x.ErrorMessage != null));
        var reread = await new HoaTheoMua.Service.PostCore.Service(db).GetPostCoreDetail(id);
        Assert.Equal(output.Platforms[0].Status, reread.Platforms[0].Status);
    }

    [Fact]
    public async Task PresetExpandsOrderedSourcesAndSharesPairsWithExplicitSelection()
    {
        await using var db = Db(); var (id, images) = await Seed(db);
        db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.ratio-preset", Kind = "Setting",
            Value = "{\"instagram\":{\"ratios\":[\"Portrait4x5\"],\"imageSelectionMode\":\"first_n\",\"imageCount\":2}}" }); await db.SaveChangesAsync();
        var input = Input(images.Take(2).ToArray(), PlatformType.Facebook);
        input.Platforms!.Add(new() { Platform = PlatformType.Instagram, UsePreset = true });
        var handler = new AiContentClient.Handler(async (r, ct) =>
        { using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct)); return AiContentClient.Completion(IsImage(json.RootElement) ? "![image](https://example.com/out.png)" : "{\"title\":\"t\",\"content\":\"c\",\"hashtag\":\"\"}"); });
        var output = await GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input);
        Assert.Equal(4, handler.Calls); Assert.Equal(2, await db.GeneratedPostImages.CountAsync());
        Assert.All(output.Platforms, p => Assert.Equal(images.Take(2), p.Images.Select(x => x.SourceBasePostImageId)));
    }

    [Theory]
    [InlineData("limit")]
    [InlineData("foreign")]
    [InlineData("deleted")]
    [InlineData("preset")]
    [InlineData("instruction")]
    public async Task InvalidImagePlanFailsBeforeClaimOrAnyAiCall(string scenario)
    {
        await using var db = Db(); var (id, images) = await Seed(db, scenario == "limit" ? 13 : 1);
        var input = Input(images, PlatformType.Facebook);
        if (scenario == "foreign") input.Platforms![0].Ratios![0].BasePostImageIds = new() { Guid.NewGuid() };
        if (scenario == "deleted") (await db.BasePostImages.FirstAsync()).IsDeleted = true;
        if (scenario == "preset") { input.Platforms![0].Ratios = null; input.Platforms[0].UsePreset = true;
            db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.ratio-preset", Kind = "Setting", Value = "{\"facebook\":{\"ratios\":[\"bad\"],\"imageSelectionMode\":\"all_images\"}}" }); }
        if (scenario == "instruction") (await db.Configs.SingleAsync(x => x.Key == "postgen.image_instruction")).Value = "{}";
        await db.SaveChangesAsync();
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        await Assert.ThrowsAnyAsync<AppException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, input));
        Assert.Equal(0, handler.Calls); Assert.Empty(db.PlatformPosts); Assert.Empty(db.GeneratedPostImages);
    }

    [Fact]
    public async Task EmptyImageIdsSelectAllAndPerCallTimeoutKeepsSuccessfulText()
    {
        await using var db = Db(); var (id, images) = await Seed(db, 1);
        var handler = new AiContentClient.Handler(async (r, ct) =>
        {
            using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct));
            if (!IsImage(json.RootElement)) return AiContentClient.Completion();
            await Task.Delay(Timeout.Infinite, ct); return AiContentClient.Completion();
        });
        var output = await GeneratePlatformPosts.Service(db, AiContentClient.Client(handler, new() { ["AiOption:TimeoutSeconds"] = "1" }))
            .GeneratePlatformPosts(id, Input(Array.Empty<Guid>(), PlatformType.Facebook));
        Assert.Equal(2, handler.Calls); Assert.Equal("Failed", output.Platforms[0].Status);
        Assert.NotNull(output.Platforms[0].Content);
        var image = await db.GeneratedPostImages.SingleAsync(); Assert.Equal(images[0], image.SourceBasePostImageId); Assert.Equal(ImageStatus.Failed, image.Status);
    }

    [Theory]
    [InlineData("{\"platforms\":[{\"platform\":\"facebook\",\"ratios\":[]}]}")]
    [InlineData("{\"platforms\":[{\"platform\":\"facebook\",\"ratios\":[null]}]}")]
    [InlineData("{\"platforms\":[{\"platform\":\"facebook\",\"ratios\":[{}]}]}")]
    [InlineData("{\"platforms\":[{\"platform\":\"facebook\",\"ratios\":[{\"ratioCode\":99}]}]}")]
    [InlineData("{\"platforms\":[{\"platform\":\"facebook\",\"ratios\":[{\"ratioCode\":\"Square1x1\"},{\"ratioCode\":\"Square1x1\"}]}]}")]
    public void RejectsInvalidRatioShapes(string json)
    {
        var input = JsonSerializer.Deserialize<Gen.Request.GenerateRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.False(new Gen.Validate.GenerateValidator().Validate(input!).IsValid);
    }

    [Fact]
    public async Task CancellationDuringImagesDoesNotPersistContentOrLeaveClaim()
    {
        await using var db = Db(); var (id, images) = await Seed(db); using var cancel = new CancellationTokenSource();
        var handler = new AiContentClient.Handler(async (r, ct) =>
        {
            using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct));
            if (!IsImage(json.RootElement)) return AiContentClient.Completion();
            cancel.Cancel(); await Task.Delay(Timeout.Infinite, ct); return AiContentClient.Completion();
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, Input(images, PlatformType.Facebook), cancel.Token));
        Assert.Empty(db.GeneratedPosts); Assert.Empty(db.GeneratedPostImages); Assert.Equal(PlatformPostStatus.Failed, (await db.PlatformPosts.SingleAsync()).Status);
    }
}
