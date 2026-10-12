using System.Net;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Moq;
using HoaTheoMua.Service.Test.PostGenTest;
using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.Extensions.Options;
using Xunit;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class ZaloAdapters
{
    internal sealed class Handler : HttpMessageHandler
    {
        public List<(string Path, JsonElement Body)> Requests = [];
        public Func<string, HttpResponseMessage> Reply = _ => Adapters.Json("{\"error\":0,\"data\":{}}");
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("openapi.zalo.me", request.RequestUri!.Host);
            Assert.Equal("test-token", request.Headers.GetValues("access_token").Single());
            Assert.DoesNotContain("test-token", request.RequestUri.ToString());
            using var body = JsonDocument.Parse(request.Content == null ? "{}" : await request.Content.ReadAsStringAsync(ct));
            Requests.Add((request.RequestUri.PathAndQuery, body.RootElement.Clone()));
            return Reply(request.RequestUri.PathAndQuery);
        }
    }
    internal static IOptions<PlatformAccountOptions> Config() => Options.Create(new PlatformAccountOptions
    { Zalo = new() { Enabled = true }, ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) } });
    internal static PublicationSnapshot Snapshot() => Adapters.Snapshot(PlatformType.Zalo) with
    { PageId = null, ApiVersion = "v2.0", Article = new("Hoa theo mùa", "Hoa 🌸") };
    internal static ZaloPublisher Create(Handler handler, Adapters.MediaHandler? media = null) => new(
        new(new HttpClient(handler)), new(new HttpClient(media ?? new())), new(Config()), Config(), new Lifecycle.Clock());

    [Fact]
    public async Task WorkerPersistsProcessReferenceAndDuplicateDeliveryDoesNotDuplicateArticle()
    {
        await using var db = Lifecycle.Db(); var (postId, accountId) = await Lifecycle.Seed(db);
        var account = await db.PlatformAccounts.SingleAsync(); account.Platform = PlatformType.Zalo;
        account.ExternalAccountId = "456"; account.PageId = null; account.GrantedScopes = [];
        (await db.PlatformPosts.SingleAsync()).Platform = PlatformType.Zalo; await db.SaveChangesAsync();
        var handler = new Handler { Reply = path => Adapters.Json(JsonSerializer.Serialize(new { error = 0, data = path switch
        {
            "/v2.0/oa/getoa" => (object)new { oa_id = "456" },
            "/v2.0/article/create" => new { token = "private-process-reference" },
            "/v2.0/article/verify" => new { id = "article-123" },
            _ => new { id = "article-123", type = "normal", status = "show", title = "Title" }
        } })) };
        var adapter = Create(handler); var clock = new Lifecycle.Clock(); var store = new Store(db, clock);
        var options = Options.Create(new PublishingOptions { ZaloEnabled = true });
        var worker = new Worker(db, store, [adapter], options, new(Config()), clock);
        var service = new HoaTheoMua.Service.PostPublishing.Service(db, store, new GeneratePlatformPosts.User(),
            new Mock<IPublishEndpoint>().Object, [adapter], options, Config(), clock, worker);
        var preview = await service.Preview(postId, "zalo", accountId);
        var job = await service.Publish(postId, "zalo", new() { AccountId = accountId, ExpectedRevision = preview.Revision, IdempotencyKey = "zalo-test" });
        for (var i = 0; i < 3; i++)
        {
            var revision = (await service.Get(job.PublicationId)).Job.Revision;
            await worker.Process(job.PublicationId, revision, default);
            await worker.Process(job.PublicationId, revision, default);
            clock.Now = clock.Now.AddMinutes(1);
        }
        var stored = await db.PostPublications.SingleAsync();
        Assert.Equal("Succeeded", stored.Status); Assert.Equal("article-123", stored.ExternalPostId);
        Assert.DoesNotContain("private-process-reference", stored.Checkpoint);
        Assert.DoesNotContain("private-process-reference", JsonSerializer.Serialize(await service.Get(job.PublicationId)));
        Assert.Single(handler.Requests, x => x.Path.EndsWith("/create"));
        Assert.Equal(new[] { "Prepare", "Publish", "Reconcile" }, (await db.PostPublicationAttempts.OrderBy(x => x.Number).ToListAsync()).Select(x => x.Operation));
    }

    [Fact]
    public async Task AsyncCreateEncryptsReferenceAndVerifiesExactArticleWithoutCreatingAgain()
    {
        var handler = new Handler(); var adapter = Create(handler); var s = Snapshot();
        var reference = new string('s', 400); var pending = true;
        handler.Reply = path => Adapters.Json(JsonSerializer.Serialize(new { error = 0, data = path switch
        {
            "/v2.0/oa/getoa" => (object)new { oa_id = "456" },
            "/v2.0/article/create" => new { token = reference },
            "/v2.0/article/verify" => pending ? new { id = "" } : new { id = "article-123" },
            _ => new { id = "article-123", type = "normal", status = "show", title = s.Title }
        } }));
        Assert.Empty(adapter.Validate(s)); Assert.Empty(await adapter.Preflight(s, Adapters.Credentials, default));
        var prepared = await adapter.Execute(PublicationOperation.Prepare, s, new(), Adapters.Credentials, null, default);
        var created = await adapter.Execute(PublicationOperation.Publish, s, prepared.Checkpoint!, Adapters.Credentials, null, default);
        Assert.Equal(PublicationOutcome.Pending, created.Outcome); Assert.True(created.PublishMayHaveOccurred);
        Assert.DoesNotContain(reference, Store.Json(created.Checkpoint));
        var request = handler.Requests.Single(x => x.Path.EndsWith("/create")).Body;
        Assert.Equal(s.Title, request.GetProperty("title").GetString());
        Assert.Equal("https://example.com/1.jpg", request.GetProperty("cover").GetProperty("photo_url").GetString());
        Assert.Equal("Nội dung\n\n#hoa", request.GetProperty("body")[0].GetProperty("content").GetString());
        Assert.Equal("https://example.com/2.jpg", request.GetProperty("body")[1].GetProperty("url").GetString());
        Assert.Equal(PublicationOutcome.Unknown, (await adapter.Execute(PublicationOperation.Publish, s, created.Checkpoint!, Adapters.Credentials, null, default)).Outcome);
        Assert.Equal(PublicationOutcome.Pending, (await adapter.Execute(PublicationOperation.Reconcile, s, created.Checkpoint!, Adapters.Credentials, null, default)).Outcome);
        pending = false;
        var result = await adapter.Execute(PublicationOperation.Reconcile, s, created.Checkpoint!, Adapters.Credentials, "untrusted-candidate", default);
        Assert.Equal(PublicationOutcome.Succeeded, result.Outcome); Assert.Equal("article-123", result.ExternalPostId); Assert.Null(result.ExternalPostUrl);
        Assert.Single(handler.Requests, x => x.Path.EndsWith("/create"));
        Assert.All(handler.Requests.Where(x => x.Path.EndsWith("/verify")), x => Assert.Equal(reference, x.Body.GetProperty("token").GetString()));
    }
    [Theory]
    [InlineData("timeout")] [InlineData("network")] [InlineData("html")] [InlineData("string-error")] [InlineData("server")]
    public async Task AmbiguousCreateNeverClaimsSafeToRetry(string scenario)
    {
        var handler = new Handler { Reply = _ => scenario switch
        {
            "timeout" => throw new TaskCanceledException("SECRET"), "network" => throw new HttpRequestException("SECRET"),
            "html" => Adapters.Json("<html>SECRET</html>"), "server" => Adapters.Json("SECRET", HttpStatusCode.BadGateway),
            _ => Adapters.Json("{\"error\":\"SECRET\"}")
        } };
        var ex = await Assert.ThrowsAsync<ZaloPublishingException>(() => Create(handler).Execute(PublicationOperation.Publish, Snapshot(), new() { ReadyToPublish = true }, Adapters.Credentials, null, default));
        var result = ex.Result(true); Assert.Equal(PublicationOutcome.Unknown, result.Outcome); Assert.False(result.ConfirmedNotPublished);
        Assert.DoesNotContain("SECRET", ex.ToString()); Assert.Single(handler.Requests);
    }
    [Theory]
    [InlineData(401, "ZALO_TOKEN_INVALID")] [InlineData(403, "ZALO_PERMISSION_DENIED")] [InlineData(429, "ZALO_RATE_LIMITED")]
    public async Task DefiniteRejectionIsSafeBeforePublishButNotDuringReconciliation(int status, string code)
    {
        var handler = new Handler { Reply = _ => Adapters.Json("SECRET", (HttpStatusCode)status) };
        var ex = await Assert.ThrowsAsync<ZaloPublishingException>(() => new ZaloPublishingClient(new HttpClient(handler)).Send("article/create", Adapters.Credentials, new { }, default));
        Assert.Equal(code, ex.Result(true).ErrorCode); Assert.True(ex.Result(true).ConfirmedNotPublished);
        Assert.Equal(PublicationOutcome.Unknown, ex.Result(true, true).Outcome);
    }
    [Fact]
    public async Task MissingProcessTokenRequiresReviewAndCannotUseArbitraryCandidate()
    {
        var handler = new Handler(); var adapter = Create(handler);
        var created = await adapter.Execute(PublicationOperation.Publish, Snapshot(), new() { ReadyToPublish = true }, Adapters.Credentials, null, default);
        Assert.Equal(PublicationOutcome.Unknown, created.Outcome);
        var review = await adapter.Execute(PublicationOperation.Reconcile, Snapshot(), new(), Adapters.Credentials, "123", default);
        Assert.Equal(PublicationOutcome.Unknown, review.Outcome); Assert.Single(handler.Requests);
    }
    [Fact]
    public async Task RejectsOversizedCoverBeforeCreate()
    {
        var handler = new Handler(); var adapter = Create(handler, new() { Length = 1_000_001 });
        await Assert.ThrowsAsync<MetaPublishingException>(() => adapter.Execute(PublicationOperation.Publish, Snapshot(), new() { ReadyToPublish = true }, Adapters.Credentials, null, default));
        Assert.Empty(handler.Requests);
    }
    [Fact]
    public void RequiresArticleMetadataAndCountsUnicodeTitleWithoutTruncating()
    {
        var adapter = Create(new()); var s = Snapshot(); var title = string.Concat(Enumerable.Repeat("🌸", 151));
        Assert.Contains("ZALO_TITLE_INVALID", adapter.Validate(s with { Title = title }));
        Assert.Contains("ZALO_ARTICLE_METADATA_INVALID", adapter.Validate(s with { Article = null }));
        Assert.Contains("PUBLICATION_IMAGE_COUNT_INVALID", adapter.Validate(s with { Images = [] }));
        Assert.False(new PublishingOptions().Enabled(PlatformType.Zalo));
    }
}
