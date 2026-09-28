using System.Net;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostInsights;
using HoaTheoMua.Service.PostPublishing;
using HoaTheoMua.Service.Test.PostGenTest;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;
using InsightsService = HoaTheoMua.Service.PostInsights.Service;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public partial class Insights
{
    private sealed class Handler : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];
        public Func<HttpRequestMessage, HttpResponseMessage>? Reply;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.DoesNotContain("access_token", request.RequestUri!.Query);
            Requests.Add(request.RequestUri);
            if (Reply != null) return Task.FromResult(Reply(request));
            var query = QueryHelpers.ParseQuery(request.RequestUri.Query);
            if (query.TryGetValue("metric", out var metric))
                return Task.FromResult(Json(new { data = new[] { new { name = metric.ToString(), values = new[] { new { value = 0L } } } } }));
            return Task.FromResult(Json(new { id = request.RequestUri.AbsolutePath.Split('/').Last() }));
        }
    }
    private static HttpResponseMessage Json(object value, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(JsonSerializer.Serialize(value)) };
    private static HttpResponseMessage Error(int code) => Json(new { error = new { code, message = "secret provider detail" } }, HttpStatusCode.BadRequest);
    private static PublicationSnapshot Snapshot(PlatformType platform) => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        "123", "123", platform, "v25.0", "postgen-text-v1", Guid.NewGuid(), "Title", "Content", "", "Title\n\nContent", []);
    private static MetaInsightsClient Client(Handler handler) => new(new(new HttpClient(handler)));
    private static Credentials Credentials => new() { AccessToken = "test-token" };

    [Fact]
    public async Task InstagramReadsAllSevenLifetimeMetricsAndKeepsRealZeros()
    {
        var handler = new Handler();
        var result = await Client(handler).Get(Snapshot(PlatformType.Instagram), "456", Credentials, default);
        Assert.Equal(7, result.Count); Assert.Equal(8, handler.Requests.Count);
        Assert.All(result.Values, x => { Assert.Equal(0L, x.Value); Assert.Equal("Available", x.Status); });
        Assert.All(handler.Requests.Skip(1), x => Assert.Equal("lifetime", QueryHelpers.ParseQuery(x.Query)["period"]));
    }

    [Fact]
    public async Task InstagramSupportsTotalValueAndIsolatesUnavailableMetrics()
    {
        var handler = new Handler { Reply = r =>
        {
            var metric = QueryHelpers.ParseQuery(r.RequestUri!.Query).GetValueOrDefault("metric").ToString();
            return metric switch
            {
                "" => Json(new { id = "456" }),
                "saved" => Error(100),
                "shares" => Json(new { data = Array.Empty<object>() }),
                _ => Json(new { data = new[] { new { name = metric, total_value = new { value = 12L } } } })
            };
        } };
        var result = await Client(handler).Get(Snapshot(PlatformType.Instagram), "456", Credentials, default);
        Assert.Equal(12L, result["views"].Value); Assert.Equal(12L, result["total_interactions"].Value);
        Assert.Null(result["saved"].Value); Assert.Equal("META_METRIC_UNAVAILABLE", result["saved"].Reason);
        Assert.Null(result["shares"].Value); Assert.Equal("META_NO_DATA", result["shares"].Reason);
    }

    [Fact]
    public async Task FacebookUsesCurrentViewMetricsAndAllReactionsForDerivedTotal()
    {
        var handler = new Handler { Reply = r =>
        {
            var metric = QueryHelpers.ParseQuery(r.RequestUri!.Query).GetValueOrDefault("metric").ToString();
            if (metric.Length > 0) return Json(new { data = new[] { new { name = metric, values = new[] { new { value = 200L } } } } });
            var fields = QueryHelpers.ParseQuery(r.RequestUri.Query)["fields"].ToString();
            Assert.Contains("likes:reactions.type(LIKE)", fields);
            return Json(new { id = "123_456", likes = new { summary = new { total_count = 3 } },
                reactions = new { summary = new { total_count = 9 } }, comments = new { summary = new { total_count = 2 } }, shares = new { count = 4 } });
        } };
        var result = await Client(handler).Get(Snapshot(PlatformType.Facebook), "123_456", Credentials, default);
        Assert.Equal(3L, result["likes"].Value); Assert.Equal(15L, result["total_interactions"].Value);
        Assert.Equal("Derived", result["total_interactions"].Status);
        Assert.Null(result["saved"].Value); Assert.Equal("Unsupported", result["saved"].Status);
        Assert.Equal("post_media_view", result["views"].Source);
        Assert.Equal("post_total_media_view_unique", result["reach"].Source);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task MissingFacebookShareCountIsNotSilentlyReportedAsZero()
    {
        var handler = new Handler();
        var result = await Client(handler).Get(Snapshot(PlatformType.Facebook), "123_456", Credentials, default);
        Assert.Null(result["shares"].Value); Assert.Null(result["total_interactions"].Value);
    }

    [Theory]
    [InlineData(-1)] [InlineData(1.5)]
    public async Task InvalidCountsAreRejected(double value)
    {
        var handler = new Handler { Reply = r => r.RequestUri!.AbsolutePath.EndsWith("insights")
            ? Json(new { data = new[] { new { name = "views", values = new[] { new { value } } } } }) : Json(new { id = "456" }) };
        var ex = await Assert.ThrowsAsync<MetaPublishingException>(() => Client(handler).Get(Snapshot(PlatformType.Instagram), "456", Credentials, default));
        Assert.Equal("META_INVALID_RESPONSE", ex.Result(false).ErrorCode);
    }

    [Theory]
    [InlineData(190, "META_TOKEN_INVALID")] [InlineData(10, "META_PERMISSION_DENIED")]
    [InlineData(4, "META_RATE_LIMITED")] [InlineData(2, "META_UNAVAILABLE")]
    public async Task ProviderFailuresAreNotConvertedIntoZeroOrUnsupported(int code, string expected)
    {
        var handler = new Handler { Reply = r => r.RequestUri!.AbsolutePath.EndsWith("insights") ? Error(code) : Json(new { id = "456" }) };
        var ex = await Assert.ThrowsAsync<MetaPublishingException>(() => Client(handler).Get(Snapshot(PlatformType.Instagram), "456", Credentials, default));
        Assert.Equal(expected, ex.Result(false).ErrorCode);
    }

    [Theory]
    [InlineData(PlatformType.Facebook, "456")]
    [InlineData(PlatformType.Facebook, "999_456")]
    [InlineData(PlatformType.Instagram, "456/insights")]
    public async Task InvalidOrWrongPageExternalIdsNeverReachProvider(PlatformType platform, string id)
    {
        var handler = new Handler();
        await Assert.ThrowsAsync<MetaPublishingException>(() => Client(handler).Get(Snapshot(platform), id, Credentials, default));
        Assert.Empty(handler.Requests);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public AppDbContext Db { get; } = Lifecycle.Db();
        public Handler Handler { get; } = new();
        public PlatformAccountOptions Options { get; } = new()
        {
            ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) },
            Meta = new() { Enabled = true, GraphApiVersion = "v26.0" }
        };
        public PostPublication Job { get; private set; } = null!;
        public Repository.Entity.PlatformAccount Account { get; private set; } = null!;
        public InsightsService Service => new(Db, new GeneratePlatformPosts.User(), new TokenProtector(Microsoft.Extensions.Options.Options.Create(Options)),
            Client(Handler), Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System);
        public async Task Seed(PlatformType platform = PlatformType.Instagram)
        {
            var snapshot = Snapshot(platform);
            var protector = new TokenProtector(Microsoft.Extensions.Options.Options.Create(Options));
            Account = new() { Id = snapshot.AccountId, Platform = platform, ExternalAccountId = "123", PageId = "123",
                GrantedScopes = ["pages_read_engagement", "instagram_basic", "instagram_manage_insights", "read_insights"],
                ProtectedCredentials = protector.Protect(Store.Json(Credentials), $"postgen:account:{snapshot.AccountId}") };
            Job = new() { Id = Guid.NewGuid(), BasePostId = snapshot.BasePostId, PlatformPostId = snapshot.PlatformPostId,
                PlatformAccountId = Account.Id, Status = "Succeeded", Snapshot = Store.Json(snapshot), ExternalPostId = platform == PlatformType.Facebook ? "123_456" : "456" };
            Db.PlatformAccounts.Add(Account); Db.PostPublications.Add(Job); await Db.SaveChangesAsync();
        }
        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }

    [Fact]
    public async Task ServiceReadsHistoryWithCurrentApiVersionWithoutMutatingPublication()
    {
        await using var f = new Fixture(); await f.Seed();
        var before = Store.Json(f.Job);
        var result = await f.Service.Get(f.Job.Id);
        Assert.Equal(f.Job.Id, result.PublicationId); Assert.False(result.IsPartial);
        Assert.Equal("v26.0", result.ApiVersion);
        Assert.All(f.Handler.Requests, r => Assert.StartsWith("/v26.0/", r.AbsolutePath));
        Assert.Equal(before, Store.Json(await f.Db.PostPublications.AsNoTracking().SingleAsync()));
        Assert.Empty(f.Db.PostPublicationAttempts); Assert.Equal("Connected", f.Account.Status);
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("\"total_interactions\":0", json); Assert.DoesNotContain("test-token", json);
    }

    [Theory]
    [InlineData("Queued", "INSIGHTS_PUBLICATION_NOT_PUBLISHED")]
    [InlineData("Failed", "INSIGHTS_PUBLICATION_NOT_PUBLISHED")]
    [InlineData("NeedsReview", "INSIGHTS_PUBLICATION_NOT_PUBLISHED")]
    public async Task UnconfirmedPublicationDoesNotCallMeta(string state, string code)
    {
        await using var f = new Fixture(); await f.Seed(); f.Job.Status = state; await f.Db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<PublicationException>(() => f.Service.Get(f.Job.Id));
        Assert.Equal(code, ex.MessageCode); Assert.Empty(f.Handler.Requests);
    }

    [Theory]
    [InlineData("scopes", "INSIGHTS_PERMISSION_REQUIRED")]
    [InlineData("expired", "PLATFORM_REAUTH_REQUIRED")]
    [InlineData("disconnected", "PLATFORM_REAUTH_REQUIRED")]
    [InlineData("wrong-account", "PLATFORM_ACCOUNT_MISMATCH")]
    [InlineData("credentials", "PLATFORM_TOKEN_UNAVAILABLE")]
    [InlineData("snapshot", "INSIGHTS_PUBLICATION_INVALID")]
    public async Task InvalidAccountAndSnapshotAreBlockedBeforeProvider(string scenario, string expected)
    {
        await using var f = new Fixture(); await f.Seed();
        switch (scenario)
        {
            case "scopes": f.Account.GrantedScopes = ["instagram_basic"]; break;
            case "expired": f.Account.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1); break;
            case "disconnected": f.Account.Status = "Disconnected"; break;
            case "wrong-account": f.Account.ExternalAccountId = "999"; break;
            case "credentials": f.Account.ProtectedCredentials = "corrupt"; break;
            case "snapshot": f.Job.Snapshot = "{}"; break;
        }
        await f.Db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<PublicationException>(() => f.Service.Get(f.Job.Id));
        Assert.Equal(expected, ex.MessageCode); Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task ZaloIsExplicitlyUnsupported()
    {
        await using var f = new Fixture(); await f.Seed(PlatformType.Zalo);
        Assert.Equal("INSIGHTS_PLATFORM_UNSUPPORTED", (await Assert.ThrowsAsync<PublicationException>(() => f.Service.Get(f.Job.Id))).MessageCode);
        Assert.Empty(f.Handler.Requests);
    }

    [Theory]
    [InlineData(190, 409)] [InlineData(10, 409)] [InlineData(4, 429)] [InlineData(2, 502)]
    public async Task ServiceNormalizesProviderErrorsAndDoesNotLeakSecrets(int code, int expectedStatus)
    {
        await using var f = new Fixture(); await f.Seed(); f.Handler.Reply = _ => Error(code);
        var ex = await Assert.ThrowsAsync<PublicationException>(() => f.Service.Get(f.Job.Id));
        Assert.Equal(expectedStatus, ex.StatusCode); Assert.DoesNotContain("secret", ex.Message);
    }

    [Fact]
    public async Task MissingPublicationReturnsNotFoundWithoutProviderCall()
    {
        await using var f = new Fixture();
        await Assert.ThrowsAsync<HoaTheoMua.Service.Exceptions.NotFoundException>(() => f.Service.Get(Guid.NewGuid()));
        Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task TimeoutIsNormalizedAndCallerCancellationIsPreserved()
    {
        await using var f = new Fixture(); await f.Seed();
        f.Handler.Reply = _ => throw new OperationCanceledException();
        var ex = await Assert.ThrowsAsync<PublicationException>(() => f.Service.Get(f.Job.Id));
        Assert.Equal(504, ex.StatusCode); Assert.Equal("META_TIMEOUT", ex.MessageCode);
        using var cts = new CancellationTokenSource();
        f.Handler.Reply = _ => { cts.Cancel(); throw new OperationCanceledException(cts.Token); };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.Service.Get(f.Job.Id, cts.Token));
    }

    [Theory]
    [InlineData("wrong-name")] [InlineData("wrong-period")] [InlineData("multiple-values")]
    public async Task UnexpectedMetricShapeCannotProduceMisleadingCounts(string scenario)
    {
        var handler = new Handler { Reply = r => r.RequestUri!.AbsolutePath.EndsWith("insights")
            ? Json(new { data = new[] { new { name = scenario == "wrong-name" ? "reach" : "views",
                period = scenario == "wrong-period" ? "day" : "lifetime",
                values = scenario == "multiple-values" ? new[] { new { value = 1 }, new { value = 2 } } : new[] { new { value = 1 } } } } })
            : Json(new { id = "456" }) };
        await Assert.ThrowsAsync<MetaPublishingException>(() => Client(handler).Get(Snapshot(PlatformType.Instagram), "456", Credentials, default));
    }
}
