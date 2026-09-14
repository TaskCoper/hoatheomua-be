using System.Net;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class GeneratePlatformPosts
{
    internal static Gen.Service Service(AppDbContext db, Ai.IService ai) => new(db, ai, new User(), new HoaTheoMua.Service.PostCore.Service(db), NullLogger<Gen.Service>.Instance);
    internal static Gen.Request.GenerateRequest Input(params PlatformType[] platforms) => new() { Platforms = platforms.Select(x => new Gen.Request.PlatformInput { Platform = x }).ToList() };
    internal sealed class User : ICurrentUserService
    {
        public Guid? UserId => Guid.Parse("ec9b5c58-3270-4914-9b3a-ff5a2ba4a201");
        public string? Email => null; public string? FullName => null; public string? Avatar => null;
        public bool IsVerified => true; public Guid GetRequiredUserId() => UserId!.Value;
    }
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    internal static async Task<Guid> Seed(AppDbContext db, bool configs = true)
    {
        var post = new BasePost { Id = Guid.NewGuid(), Title = "Bó hoa Cảm Ơn", Content = "10 cành hồng, giá 450.000đ. Đặt trước 1 ngày. Giá chưa bao gồm phí giao hàng.", CreatedAt = DateTimeOffset.UtcNow };
        db.BasePosts.Add(post);
        db.BasePostImages.Add(new() { Id = Guid.NewGuid(), BasePostId = post.Id, ImageUrl = "https://example.com/flower.png", SortOrder = 1, CreatedAt = DateTimeOffset.UtcNow });
        if (configs)
        foreach (var name in new[] { "facebook", "instagram", "zalo" })
        {
            db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.system-prompt." + name, Kind = "Setting", Value = JsonSerializer.Serialize(new { isActive = true, content = "  Complete " + name + " prompt\n" }) });
        }
        await db.SaveChangesAsync(); return post.Id;
    }

    [Fact]
    public async Task GeneratesContentForPostCreatedWithoutImages()
    {
        await using var db = Db(); await Seed(db);
        var post = await new HoaTheoMua.Service.PostCore.Service(db).CreatePostCore(new() { Title = "Hoa", Content = "Bản nháp chưa có ảnh" });
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        var request = Input(PlatformType.Facebook); request.Mode = "content";
        var result = await Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(post.BasePostId, request);
        Assert.Empty(result.BaseImages);
        var platform = Assert.Single(result.Platforms);
        Assert.NotNull(platform.Content);
        Assert.Empty(platform.Images);
        Assert.Equal(1, handler.Calls);
        Assert.Empty(db.GeneratedPostImages);
    }

    [Fact]
    public async Task ThreeIndependentPromptsSnapshotsVersionsAndOfficialPointers()
    {
        await using var db = Db(); var id = await Seed(db);
        var sent = new List<string>();
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            sent.Add(body.RootElement.GetProperty("messages")[0].GetProperty("content")[0].GetProperty("text").GetString()!);
            return AiContentClient.Completion();
        });
        var service = Service(db, AiContentClient.Client(handler));
        var result = await service.GeneratePlatformPosts(id, Input(PlatformType.Facebook, PlatformType.Instagram, PlatformType.Zalo));
        Assert.Equal(3, result.Platforms.Count); Assert.Equal(3, db.GeneratedPosts.Count());
        Assert.Equal(3, sent.Distinct().Count());
        foreach (var platform in result.Platforms)
        {
            Assert.Equal("Generated", platform.Status); Assert.Equal(1, platform.Content!.VersionNo); Assert.Empty(platform.Images);
            var version = await db.GeneratedPosts.SingleAsync(x => x.Id == platform.Content.GenerationId);
            Assert.Equal(ContentSource.Ai, version.Source); Assert.Null(version.SystemPromptId); Assert.Null(version.ParentGeneratedPostId);
            Assert.Equal("  Complete " + platform.Platform + " prompt\n", version.PromptSnapshot);
            Assert.Equal("postgen.system-prompt." + platform.Platform, (await db.Configs.SingleAsync(x => x.Id == version.SystemPromptConfigId)).Key);
            Assert.Equal(new User().GetRequiredUserId(), version.UserId);
        }
        var prompt = await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.facebook"); prompt.Value = "{\"isActive\":true,\"content\":\"Changed Facebook\"}"; await db.SaveChangesAsync();
        await service.GeneratePlatformPosts(id, Input(PlatformType.Facebook, PlatformType.Instagram));
        Assert.Equal(new[] { "Changed Facebook", "  Complete instagram prompt\n" }, sent.Skip(3));
        Assert.Equal(5, db.GeneratedPosts.Count());
        Assert.Single(db.GeneratedPosts.Where(x => x.PromptSnapshot == "  Complete facebook prompt\n"));
        Assert.Empty(db.PostHistories); Assert.Empty(db.ClientHistories);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("invalid")]
    [InlineData("deleted")]
    [InlineData("disabled")]
    [InlineData("missing-active")]
    [InlineData("invalid-active")]
    [InlineData("global-disabled")]
    public async Task InvalidConfigFailsBeforeAnyAiCallOrClaim(string scenario)
    {
        await using var db = Db(); var id = await Seed(db);
        var prompt = await db.Configs.SingleAsync(x => x.Key == "postgen.system-prompt.facebook");
        switch (scenario)
        {
            case "missing": db.Configs.Remove(prompt); break;
            case "empty": prompt.Value = "{\"content\":\" \"}"; break;
            case "invalid": prompt.Value = "[]"; break;
            case "deleted": prompt.IsDeleted = true; break;
            case "disabled": prompt.Value = "{\"content\":\"Keep prompt\",\"isActive\":false}"; break;
            case "missing-active": prompt.Value = "{\"content\":\"Keep prompt\"}"; break;
            case "invalid-active": prompt.Value = "{\"content\":\"Keep prompt\",\"isActive\":\"true\"}"; break;
            case "global-disabled": db.Configs.Add(new() { Id = Guid.NewGuid(), Group = "postgen", Key = "postgen.enabled", Kind = "Setting", Value = "false" }); break;
        }
        db.SystemPrompts.Add(new() { Id = Guid.NewGuid(), Type = SystemPromptType.Post, Content = "Legacy fallback forbidden" });
        await db.SaveChangesAsync();
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        await Assert.ThrowsAsync<BadRequestException>(() => Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, Input(PlatformType.Facebook)));
        Assert.Equal(0, handler.Calls); Assert.Empty(db.GeneratedPosts); Assert.Empty(db.PlatformPosts);
    }

    [Fact]
    public async Task SecondPlatformFailureWritesNoVersionsAndAllowsRetry()
    {
        await using var db = Db(); var id = await Seed(db); var calls = 0;
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(++calls == 2 ? new HttpResponseMessage(HttpStatusCode.InternalServerError) : AiContentClient.Completion()));
        var service = Service(db, AiContentClient.Client(handler));
        await Assert.ThrowsAsync<BadGatewayException>(() => service.GeneratePlatformPosts(id, Input(PlatformType.Facebook, PlatformType.Instagram)));
        Assert.Empty(db.GeneratedPosts); Assert.All(db.PlatformPosts, x => { Assert.Equal(PlatformPostStatus.Failed, x.Status); Assert.Null(x.OfficialGenerationId); });
        await service.GeneratePlatformPosts(id, Input(PlatformType.Facebook, PlatformType.Instagram));
        Assert.Equal(2, db.GeneratedPosts.Count());
    }

    [Fact]
    public async Task CancellationReleasesGeneratingState()
    {
        await using var db = Db(); var id = await Seed(db); using var cancellation = new CancellationTokenSource();
        var handler = new AiContentClient.Handler(async (_, ct) => { cancellation.Cancel(); await Task.Delay(Timeout.Infinite, ct); return AiContentClient.Completion(); });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(db, AiContentClient.Client(handler)).GeneratePlatformPosts(id, Input(PlatformType.Facebook), cancellation.Token));
        Assert.Empty(db.GeneratedPosts); Assert.Equal(PlatformPostStatus.Failed, (await db.PlatformPosts.SingleAsync()).Status);
    }

    [Fact]
    public void RejectsMissingDuplicatePlatformsAndConflictingImageRequests()
    {
        var validator = new Gen.Validate.GenerateValidator();
        Assert.False(validator.Validate(new Gen.Request.GenerateRequest()).IsValid);
        Assert.False(validator.Validate(new Gen.Request.GenerateRequest() { Platforms = new() { new() } }).IsValid);
        Assert.False(validator.Validate(Input(PlatformType.Facebook, PlatformType.Facebook)).IsValid);
        Assert.False(validator.Validate(Input((PlatformType)99)).IsValid);
        var imageRequest = Input(PlatformType.Facebook); imageRequest.Platforms![0].UsePreset = true;
        Assert.True(validator.Validate(imageRequest).IsValid);
        imageRequest.Platforms[0].Ratios = new();
        Assert.False(validator.Validate(imageRequest).IsValid);
        var parsed = JsonSerializer.Deserialize<Gen.Request.GenerateRequest>("{\"platforms\":[{\"platform\":\"facebook\"}]}", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.True(validator.Validate(parsed!).IsValid);
    }
}
