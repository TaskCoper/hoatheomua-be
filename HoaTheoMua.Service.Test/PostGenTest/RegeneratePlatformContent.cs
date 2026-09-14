using System.Net;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class RegeneratePlatformContent
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task GenerateRegenerateManualRegenerateKeepsLineageSnapshotsAndCurrentFeedbackOnly()
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        var inputs = new List<JsonElement>();
        var handler = new AiContentClient.Handler(async (r, ct) =>
        {
            using var json = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct));
            using var input = JsonDocument.Parse(json.RootElement.GetProperty("messages")[1].GetProperty("content")[0].GetProperty("text").GetString()!);
            inputs.Add(input.RootElement.Clone());
            return AiContentClient.Completion(JsonSerializer.Serialize(new { title = "Title", content = "AI " + inputs.Count, hashtag = "#hoa" }));
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var created = await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook));
        var v1 = created.Platforms[0].Content!.GenerationId;
        var regenerated = await service.RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = v1, Feedback = "First feedback" });
        var v2 = regenerated.Platforms[0].Content!.GenerationId;
        var manual = await service.EditPlatformContent(id, "facebook", new() { Title = "Manual title", Content = "Manual body", Hashtag = "#manual" });
        var v3 = manual.Platforms[0].Content!.GenerationId;
        await service.EditPlatformContent(id, "facebook", new() { Title = "Saved again", Content = "Manual edited", Hashtag = "#manual" });
        Assert.Equal(3, await db.GeneratedPosts.CountAsync());
        var final = await service.RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = v3, Feedback = "Second feedback" });
        var history = await service.GetContentVersions(id, "facebook");
        Assert.Equal(new[] { "Ai", "Ai", "Manual", "Ai" }, history.Versions.Select(x => x.Source));
        Assert.Equal(new[] { 1, 2, 3, 4 }, history.Versions.Select(x => x.VersionNo));
        Assert.Equal(new Guid?[] { null, v1, v2, v3 }, history.Versions.Select(x => x.ParentGeneratedPostId));
        Assert.Equal("Manual edited", inputs[2].GetProperty("previousContent").GetProperty("content").GetString());
        Assert.Equal("Second feedback", inputs[2].GetProperty("feedback").GetString());
        Assert.DoesNotContain("First feedback", inputs[2].ToString());
        Assert.Equal(final.Platforms[0].Content!.GenerationId, history.OfficialGenerationId);
        var saved = await db.GeneratedPosts.SingleAsync(x => x.Id == history.Versions[3].GenerationId);
        Assert.Equal("Second feedback", saved.FeedbackInput); Assert.NotNull(saved.SystemPromptConfigId); Assert.NotNull(saved.PromptSnapshot);
        var manualRow = await db.GeneratedPosts.SingleAsync(x => x.Id == v3);
        Assert.Null(manualRow.SystemPromptConfigId); Assert.Null(manualRow.SystemPromptId); Assert.Null(manualRow.PromptSnapshot);
        Assert.Null(manualRow.AiModel); Assert.Null(manualRow.AiRequestId); Assert.Null(manualRow.DurationMs);
        await service.RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = v1, Feedback = "Branch" });
        Assert.Equal("AI 1", inputs[3].GetProperty("previousContent").GetProperty("content").GetString());
        await service.RegeneratePlatformContent(id, "facebook", new());
        Assert.Equal(JsonValueKind.Null, inputs[4].GetProperty("previousContent").ValueKind);
        Assert.Null((await service.GetContentVersions(id, "facebook")).Versions.Last().ParentGeneratedPostId);
        Assert.Empty(db.GeneratedPostImages); Assert.Empty(db.PostHistories); Assert.Empty(db.ClientHistories);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProviderFailureOrCancellationKeepsOfficialAndReleasesClaim(bool cancel)
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        var ok = GeneratePlatformPosts.Service(db, AiContentClient.Client(new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()))));
        var initial = await ok.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook));
        using var cts = new CancellationTokenSource();
        var handler = new AiContentClient.Handler(async (_, ct) =>
        {
            if (cancel) { cts.Cancel(); await Task.Delay(Timeout.Infinite, ct); }
            return new(HttpStatusCode.InternalServerError);
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var call = () => service.RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = initial.Platforms[0].Content!.GenerationId }, cts.Token);
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(call);
        else await Assert.ThrowsAsync<BadGatewayException>(call);
        Assert.Single(db.GeneratedPosts); Assert.Equal(1, handler.Calls);
        var platform = await db.PlatformPosts.SingleAsync(); Assert.Equal(PlatformPostStatus.Failed, platform.Status);
        Assert.Equal(initial.Platforms[0].Content!.GenerationId, platform.OfficialGenerationId);
    }

    [Fact]
    public async Task ForeignSourceIsRejectedBeforeAnyAiCallOrClaim()
    {
        await using var db = Db(); var id = await GeneratePlatformPosts.Seed(db);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var initial = await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook, PlatformType.Instagram));
        var foreign = initial.Platforms.Single(x => x.Platform == "instagram").Content!.GenerationId;
        await Assert.ThrowsAsync<BadRequestException>(() => service.RegeneratePlatformContent(id, "facebook", new() { SourceGenerationId = foreign }));
        Assert.Equal(2, handler.Calls); Assert.All(db.PlatformPosts, p => Assert.Equal(PlatformPostStatus.Generated, p.Status));
    }
}
