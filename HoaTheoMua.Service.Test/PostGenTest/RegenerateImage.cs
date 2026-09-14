using System.Net;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class RegenerateImage
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task BranchesFromExactVersionWithOriginalReferenceAndOnlyCurrentFeedbackWithoutSelecting()
    {
        await using var db = Db(); var (id, sources) = await GenerateImages.Seed(db, 1);
        var bodies = new List<JsonElement>();
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct)); bodies.Add(json.RootElement.Clone());
            return GenerateImages.IsImage(json.RootElement) ? AiContentClient.Completion($"![image](https://example.com/v{bodies.Count}.png)") : AiContentClient.Completion();
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        await service.GeneratePlatformPosts(id, GenerateImages.Input(sources, PlatformType.Facebook, PlatformType.Instagram));
        var original = await db.GeneratedPostImages.SingleAsync();
        var platformsBefore = JsonSerializer.Serialize(await db.PlatformPosts.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        var slotsBefore = JsonSerializer.Serialize(await db.PlatformPostImages.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        var request = new Gen.Request.RegenerateImageRequest { SourceBasePostImageId = sources[0], RatioCode = original.RatioCode, SourceGeneratedPostImageId = original.Id, Feedback = "Nền sáng hơn" };
        var v2 = await service.RegenerateImage(id, request);
        Assert.Equal(2, v2.VersionNo); Assert.Equal(original.Id, v2.ParentGeneratedPostImageId); Assert.Equal(original.ImageUrl, v2.SourceImageUrl);
        Assert.Empty(v2.SelectedPlatforms);
        var parts = bodies.Last().GetProperty("messages")[0].GetProperty("content");
        Assert.Equal(new[] { "text", "image_url", "text", "image_url", "text", "text", "text" }, parts.EnumerateArray().Select(x => x.GetProperty("type").GetString()));
        Assert.Equal(original.SourceImageUrl, parts[1].GetProperty("image_url").GetProperty("url").GetString());
        Assert.Equal(original.ImageUrl, parts[3].GetProperty("image_url").GetProperty("url").GetString());
        request.SourceGeneratedPostImageId = v2.GeneratedPostImageId; request.Feedback = "Thêm khoảng trống bên phải";
        var v3 = await service.RegenerateImage(id, request);
        Assert.Equal(v2.GeneratedPostImageId, v3.ParentGeneratedPostImageId);
        Assert.DoesNotContain("Nền sáng hơn", bodies.Last().ToString());
        request.SourceGeneratedPostImageId = original.Id; request.Feedback = null;
        Assert.Equal(original.Id, (await service.RegenerateImage(id, request)).ParentGeneratedPostImageId);
        request.SourceGeneratedPostImageId = null;
        var fresh = await service.RegenerateImage(id, request);
        Assert.Null(fresh.ParentGeneratedPostImageId); Assert.Equal(original.SourceImageUrl, fresh.SourceImageUrl);
        Assert.Equal(4, bodies.Last().GetProperty("messages")[0].GetProperty("content").GetArrayLength());
        Assert.Equal(platformsBefore, JsonSerializer.Serialize(await db.PlatformPosts.AsNoTracking().OrderBy(x => x.Id).ToListAsync()));
        Assert.Equal(slotsBefore, JsonSerializer.Serialize(await db.PlatformPostImages.AsNoTracking().OrderBy(x => x.Id).ToListAsync()));
        Assert.Equal(2, await db.GeneratedPosts.CountAsync());
    }

    [Fact]
    public async Task FailedAttemptIsRetainedButCancellationDoesNotAppendAndInvalidSourcesNeverCallAi()
    {
        await using var db = Db(); var (id, sources) = await GenerateImages.Seed(db, 2);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var request = new Gen.Request.RegenerateImageRequest { SourceBasePostImageId = sources[0], RatioCode = AspectRatioType.Square1x1 };
        var failed = await service.RegenerateImage(id, request);
        Assert.Equal("Failed", failed.Status); Assert.Null(failed.ImageUrl); Assert.NotNull(failed.ErrorMessage);
        Assert.Empty(db.PlatformPosts); Assert.Empty(db.PlatformPostImages);
        request.SourceGeneratedPostImageId = failed.GeneratedPostImageId;
        Assert.Equal("Cannot regenerate from a failed image", (await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id, request))).Message);
        request.RatioCode = AspectRatioType.Story9x16;
        Assert.Equal("Source image does not match the requested ratio", (await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id, request))).Message);
        request.SourceGeneratedPostImageId = Guid.NewGuid();
        await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id, request));
        request.SourceGeneratedPostImageId = null; request.SourceBasePostImageId = Guid.NewGuid();
        await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id, request));
        Assert.Equal(1, handler.Calls);
        request.SourceBasePostImageId = sources[0];
        using var cts = new CancellationTokenSource();
        var canceled = new AiContentClient.Handler((_, ct) => { cts.Cancel(); ct.ThrowIfCancellationRequested(); return Task.FromResult(AiContentClient.Completion()); });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GeneratePlatformPosts.Service(db, AiContentClient.Client(canceled)).RegenerateImage(id, request, cts.Token));
        Assert.Single(db.GeneratedPostImages);
        db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.enabled", Kind = "Setting", Value = "false" }); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id, request)); Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task RejectsRemovedSourcesAndForeignParentsBeforeAi()
    {
        await using var db = Db(); var (id, sources) = await GenerateImages.Seed(db, 1);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion("![image](https://example.com/out.png)")));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var request = new Gen.Request.RegenerateImageRequest { SourceBasePostImageId = sources[0], RatioCode = AspectRatioType.Square1x1 };
        var version = await service.RegenerateImage(id, request);
        var other = await new HoaTheoMua.Service.PostCore.Service(db).CreatePostCore(new() { Title = "Other", Content = "Other", Images = new() { new() { ImageUrl = "https://example.com/other.png", SortOrder = 1 } } });
        request.SourceBasePostImageId = other.BaseImages[0].Id; request.SourceGeneratedPostImageId = version.GeneratedPostImageId;
        await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(other.BasePostId, request));
        request.SourceBasePostImageId = sources[0]; request.SourceGeneratedPostImageId = null;
        (await db.BasePostImages.SingleAsync(x => x.Id == sources[0])).IsDeleted = true; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BadRequestException>(() => service.RegenerateImage(id, request));
        Assert.Equal(1, handler.Calls);
    }
}
