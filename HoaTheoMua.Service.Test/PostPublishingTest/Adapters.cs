using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class Adapters
{
    internal sealed record Sent(string Method, string Path, Dictionary<string, string> Form);
    internal sealed class Handler : HttpMessageHandler
    {
        public List<Sent> Requests = [];
        public Func<Sent, HttpResponseMessage> Reply = _ => Json("{}");
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("graph.facebook.com", request.RequestUri!.Host);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.DoesNotContain("test-token", request.RequestUri.ToString());
            var body = request.Content == null ? request.RequestUri.Query.TrimStart('?') : await request.Content.ReadAsStringAsync(ct);
            var form = body.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=', 2))
                .ToDictionary(x => WebUtility.UrlDecode(x[0]), x => WebUtility.UrlDecode(x.Length > 1 ? x[1] : ""));
            var sent = new Sent(request.Method.Method, request.RequestUri.AbsolutePath, form); Requests.Add(sent);
            return Reply(sent);
        }
    }
    internal sealed class MediaHandler : HttpMessageHandler
    {
        public int Calls; public string Mime = "image/jpeg"; public int Width = 1080; public int Height = 1080;
        public HttpStatusCode Status = HttpStatusCode.OK; public byte[]? Bytes; public long? Length;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++; Assert.Null(request.Headers.Authorization);
            var bytes = Bytes ?? new byte[] { 0xff, 0xd8, 0xff, 0xc0, 0, 11, 8, (byte)(Height >> 8), (byte)Height, (byte)(Width >> 8), (byte)Width, 1, 1, 0x11, 0, 0xff, 0xd9 };
            var content = new ByteArrayContent(bytes); content.Headers.ContentType = new MediaTypeHeaderValue(Mime);
            if (Length.HasValue) content.Headers.ContentLength = Length;
            return Task.FromResult(new HttpResponseMessage(Status) { Content = content });
        }
    }
    internal static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(json) };
    internal static PublicationSnapshot Snapshot(PlatformType platform = PlatformType.Facebook, int count = 2) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), platform == PlatformType.Facebook ? "123" : "456", "123", platform, "v25.0", "postgen-text-v1", Guid.NewGuid(),
        "Hoa 🌸", "Nội dung", "#hoa", "Hoa 🌸\n\nNội dung\n\n#hoa", Enumerable.Range(1, count).Reverse().Select(i => new PublicationImage(Guid.NewGuid(), $"https://example.com/{i}.jpg", i)).ToArray());
    internal static readonly Credentials Credentials = new() { AccessToken = "test-token" };
    internal static (IPublishingAdapter Adapter, Handler Graph, MediaHandler Media, Mock<IOAuthProvider> Accounts) Create(PlatformType platform = PlatformType.Facebook)
    {
        var handler = new Handler(); var media = new MediaHandler(); var accounts = new Mock<IOAuthProvider>();
        accounts.Setup(x => x.Refresh(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlatformType p, string id, string page, Credentials c, CancellationToken _) => new ProviderAccount { ExternalId = id, PageId = page, Credentials = c });
        var options = Options.Create(new PlatformAccountOptions { Meta = new() { Enabled = true, GraphApiVersion = "v25.0" } });
        var client = new MetaPublishingClient(new HttpClient(handler) { MaxResponseContentBufferSize = 1024 * 1024 });
        var files = new PublishingMediaClient(new HttpClient(media));
        return (platform == PlatformType.Facebook ? new FacebookPublisher(client, files, accounts.Object, options, new Lifecycle.Clock())
            : new InstagramPublisher(client, files, accounts.Object, options, new Lifecycle.Clock()), handler, media, accounts);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task FacebookUsesExactSnapshotOrderAndCorrectPhotoVsPostId(int count)
    {
        var (adapter, graph, media, _) = Create(); var s = Snapshot(count: count); var cp = new PublicationCheckpoint();
        graph.Reply = r => r.Path.EndsWith("/feed") ? Json("{\"id\":\"123_900\"}")
            : r.Form["published"] == "true" ? Json("{\"id\":\"800\",\"post_id\":\"123_900\"}") : Json("{\"id\":\"" + (800 + graph.Requests.Count) + "\"}");
        Assert.Empty(adapter.Validate(s)); Assert.Empty(graph.Requests);
        while (!cp.ReadyToPublish)
        {
            var result = await adapter.Execute(PublicationOperation.Prepare, s, cp, Credentials, null, default);
            Assert.Equal(PublicationOutcome.Continue, result.Outcome); cp = result.Checkpoint!;
        }
        Assert.All(graph.Requests, r => Assert.Equal("false", r.Form["published"]));
        var published = await adapter.Execute(PublicationOperation.Publish, s, cp, Credentials, null, default);
        Assert.Equal(PublicationOutcome.Succeeded, published.Outcome); Assert.Equal("123_900", published.ExternalPostId);
        Assert.Equal("https://www.facebook.com/123/posts/900", published.ExternalPostUrl);
        var final = graph.Requests.Last(); Assert.Equal(s.Text, final.Form[count == 1 ? "caption" : "message"]);
        if (count == 1) { Assert.Single(graph.Requests); Assert.Equal("800", Assert.Single(published.Checkpoint!.Media).ProviderId); }
        else
        {
            Assert.Equal(Enumerable.Range(1, count).Select(i => $"https://example.com/{i}.jpg"), graph.Requests.Take(count).Select(x => x.Form["url"]));
            var attached = JsonDocument.Parse(final.Form["attached_media"]).RootElement;
            Assert.Equal(cp.Media.Select(x => x.ProviderId), attached.EnumerateArray().Select(x => x.GetProperty("media_fbid").GetString()));
            Assert.Equal(count + 1, graph.Requests.Count);
        }
        Assert.Equal(count == 1 ? 2 : count, media.Calls);
    }
    [Fact]
    public async Task FacebookResumeSkipsSavedPhotoAndRejectsBadSecondImageWithoutPosting()
    {
        var (adapter, graph, media, _) = Create(); var s = Snapshot();
        var cp = new PublicationCheckpoint { Media = [new(1, "801")] };
        media.Status = HttpStatusCode.NotFound;
        var ex = await Assert.ThrowsAsync<MetaPublishingException>(() => adapter.Execute(PublicationOperation.Prepare, s, cp, Credentials, null, default));
        Assert.Equal("MEDIA_FETCH_FAILED_2", ex.Result(false).ErrorCode); Assert.Empty(graph.Requests);
        media.Status = HttpStatusCode.OK; graph.Reply = _ => Json("{\"id\":\"802\"}");
        var resumed = await adapter.Execute(PublicationOperation.Prepare, s, cp, Credentials, null, default);
        Assert.Equal("https://example.com/2.jpg", Assert.Single(graph.Requests).Form["url"]);
        Assert.Equal(new[] { "801", "802" }, resumed.Checkpoint!.Media.Select(x => x.ProviderId));
    }
    [Theory]
    [InlineData("{\"id\":\"800\"}")]
    [InlineData("{\"id\":\"800\",\"post_id\":\"999_900\"}")]
    public async Task FacebookMissingOrWrongPostIdRetainsPhotoForReview(string json)
    {
        var (adapter, graph, _, _) = Create(); graph.Reply = _ => Json(json);
        var result = await adapter.Execute(PublicationOperation.Publish, Snapshot(count: 1), new() { ReadyToPublish = true }, Credentials, null, default);
        Assert.Equal(PublicationOutcome.Unknown, result.Outcome); Assert.Equal("800", result.Checkpoint!.Media[0].ProviderId);
        Assert.Null(result.ExternalPostId);
    }
    [Theory]
    [InlineData("801", "802", true)]
    [InlineData("802", "801", false)]
    [InlineData("901", "902", false)]
    public async Task FacebookReviewRequiresExactPersistedMediaInOrder(string first, string second, bool matches)
    {
        var (adapter, graph, _, _) = Create(); var s = Snapshot();
        graph.Reply = _ => Json(JsonSerializer.Serialize(new { id = "123_900", from = new { id = "123" }, message = s.Text, is_published = true,
            attachments = new { data = new[] { new { subattachments = new { data = new[] { new { target = new { id = first } }, new { target = new { id = second } } } } } } } }));
        var result = await adapter.Execute(PublicationOperation.Reconcile, s, new() { Media = [new(1, "801"), new(2, "802")] }, Credentials, "123_900", default);
        Assert.Equal(matches ? PublicationOutcome.Succeeded : PublicationOutcome.Unknown, result.Outcome);
        Assert.Equal("GET", Assert.Single(graph.Requests).Method);
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task InstagramCreatesEachContainerOncePollsAndPublishesParentWithExactCaption(int count)
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram); var s = Snapshot(PlatformType.Instagram, count); var cp = new PublicationCheckpoint(); var next = 800;
        graph.Reply = r => r.Method == "GET" ? r.Form["fields"] == "status_code" ? Json($"{{\"id\":\"{r.Path.Split('/').Last()}\",\"status_code\":\"FINISHED\"}}")
            : Json("{\"id\":\"999\",\"permalink\":\"https://www.instagram.com/p/verified/\"}")
            : r.Path.EndsWith("/media_publish") ? Json("{\"id\":\"999\"}") : Json($"{{\"id\":\"{++next}\"}}");
        for (var i = 0; !cp.ReadyToPublish && i < 30; i++)
        {
            var result = await adapter.Execute(PublicationOperation.Prepare, s, cp, Credentials, null, default);
            Assert.Equal(PublicationOutcome.Continue, result.Outcome); cp = result.Checkpoint!;
        }
        Assert.True(cp.ReadyToPublish); Assert.DoesNotContain(graph.Requests, r => r.Path.EndsWith("/media_publish"));
        var create = graph.Requests.Where(x => x.Method == "POST").ToArray();
        Assert.Equal(count == 1 ? 1 : count + 1, create.Length);
        Assert.Equal(s.Text, create.Last().Form["caption"]);
        if (count > 1)
        {
            Assert.All(create.Take(count), r => { Assert.Equal("true", r.Form["is_carousel_item"]); Assert.False(r.Form.ContainsKey("caption")); });
            Assert.Equal(string.Join(',', Enumerable.Range(801, count)), create.Last().Form["children"]);
            Assert.Equal("CAROUSEL", create.Last().Form["media_type"]);
        }
        var success = await adapter.Execute(PublicationOperation.Publish, s, cp, Credentials, null, default);
        Assert.Equal("999", success.ExternalPostId); Assert.Equal("https://www.instagram.com/p/verified/", success.ExternalPostUrl);
        Assert.Equal(cp.ContainerId, Assert.Single(graph.Requests, r => r.Path.EndsWith("/media_publish")).Form["creation_id"]);
    }
    [Theory]
    [InlineData("IN_PROGRESS", PublicationOutcome.Pending)]
    [InlineData("ERROR", PublicationOutcome.Rejected)]
    [InlineData("EXPIRED", PublicationOutcome.Rejected)]
    [InlineData("PUBLISHED", PublicationOutcome.Unknown)]
    [InlineData("OTHER", PublicationOutcome.Unknown)]
    public async Task InstagramPollDoesNotRecreateContainers(string state, PublicationOutcome outcome)
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram); graph.Reply = _ => Json($"{{\"id\":\"801\",\"status_code\":\"{state}\"}}");
        var result = await adapter.Execute(PublicationOperation.Prepare, Snapshot(PlatformType.Instagram, 1), new() { ContainerId = "801" }, Credentials, null, default);
        Assert.Equal(outcome, result.Outcome); Assert.Equal("GET", Assert.Single(graph.Requests).Method);
        Assert.Equal(state == "PUBLISHED", result.PublishMayHaveOccurred);
    }
    [Theory]
    [InlineData("IN_PROGRESS")]
    [InlineData("FINISHED")]
    [InlineData("ERROR")]
    [InlineData("EXPIRED")]
    [InlineData("PUBLISHED")]
    public async Task InstagramNeverInterpretsStatusAfterTimeoutAsSafeToRepublish(string state)
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram); graph.Reply = _ => Json($"{{\"id\":\"801\",\"status_code\":\"{state}\"}}");
        var result = await adapter.Execute(PublicationOperation.Reconcile, Snapshot(PlatformType.Instagram, 1), new() { ContainerId = "801" }, Credentials, "999", default);
        Assert.Equal(PublicationOutcome.Unknown, result.Outcome); Assert.False(result.ConfirmedNotPublished);
        Assert.Equal("GET", Assert.Single(graph.Requests).Method);
    }
    [Fact]
    public async Task InstagramSuccessfulIdSurvivesPermalinkTimeout()
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram);
        graph.Reply = r => r.Method == "POST" ? Json("{\"id\":\"999\"}") : throw new TaskCanceledException("SECRET");
        var result = await adapter.Execute(PublicationOperation.Publish, Snapshot(PlatformType.Instagram, 1), new() { ReadyToPublish = true, ContainerId = "801" }, Credentials, null, default);
        Assert.Equal(PublicationOutcome.Succeeded, result.Outcome); Assert.Equal("999", result.ExternalPostId); Assert.Null(result.ExternalPostUrl);
    }
    [Theory]
    [InlineData(0, 50, true)]
    [InlineData(99, 100, true)]
    [InlineData(50, 50, false)]
    [InlineData(100, 100, false)]
    public async Task InstagramUsesProviderQuotaInsteadOfHardcodingLimit(int used, int limit, bool allowed)
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram);
        graph.Reply = _ => Json($"{{\"data\":[{{\"quota_usage\":{used},\"config\":{{\"quota_total\":{limit}}}}}]}}");
        if (allowed) Assert.Empty(await adapter.Preflight(Snapshot(PlatformType.Instagram), Credentials, default));
        else Assert.Equal("META_RATE_LIMITED", (await Assert.ThrowsAsync<MetaPublishingException>(() => adapter.Preflight(Snapshot(PlatformType.Instagram), Credentials, default))).Result(false).ErrorCode);
        Assert.Equal("/v25.0/456/content_publishing_limit", Assert.Single(graph.Requests).Path);
    }
    [Theory]
    [InlineData("{}")] [InlineData("{\"data\":[]}")] [InlineData("{\"data\":[{\"quota_usage\":1}]}")]
    public async Task InstagramMissingQuotaFailsClosed(string json)
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram); graph.Reply = _ => Json(json);
        await Assert.ThrowsAsync<MetaPublishingException>(() => adapter.Preflight(Snapshot(PlatformType.Instagram), Credentials, default));
    }
    [Theory]
    [InlineData(2200, 10, true)] [InlineData(2201, 10, false)] [InlineData(10, 11, false)] [InlineData(10, 0, false)]
    public void InstagramCaptionCountsUnicodeScalarsAndDoesNotTruncate(int length, int count, bool valid)
    {
        var (adapter, graph, _, _) = Create(PlatformType.Instagram); var text = string.Concat(Enumerable.Repeat("🌸", length));
        var s = Snapshot(PlatformType.Instagram, count) with { Title = null, Content = text, Hashtag = null, Text = text };
        Assert.Equal(valid, adapter.Validate(s).Count == 0); Assert.Equal(text, s.Text); Assert.Empty(graph.Requests);
    }
    [Fact]
    public void InstagramChecksTagsAndFormatterConsistency()
    {
        var (adapter, _, _, _) = Create(PlatformType.Instagram); var s = Snapshot(PlatformType.Instagram);
        var text = string.Join(' ', Enumerable.Repeat("#hoa", 31).Concat(Enumerable.Repeat("@shop", 21)));
        var errors = adapter.Validate(s with { Title = null, Content = text, Hashtag = null, Text = text });
        Assert.Contains("INSTAGRAM_TOO_MANY_HASHTAGS", errors); Assert.Contains("INSTAGRAM_TOO_MANY_MENTIONS", errors);
        Assert.Contains("PUBLICATION_FORMATTER_MISMATCH", adapter.Validate(s with { Text = "silently changed" }));
    }
    [Theory]
    [InlineData(400, 190, "META_TOKEN_INVALID", PublicationOutcome.Rejected)]
    [InlineData(403, 200, "META_PERMISSION_DENIED", PublicationOutcome.Rejected)]
    [InlineData(400, 100, "META_INVALID_MEDIA_OR_PARAMETER", PublicationOutcome.Rejected)]
    [InlineData(429, 4, "META_RATE_LIMITED", PublicationOutcome.Rejected)]
    [InlineData(500, 2, "META_UNAVAILABLE", PublicationOutcome.Unknown)]
    [InlineData(500, 190, "META_TOKEN_INVALID", PublicationOutcome.Unknown)]
    [InlineData(200, 190, "META_TOKEN_INVALID", PublicationOutcome.Unknown)]
    [InlineData(400, 9999, "META_REQUEST_REJECTED", PublicationOutcome.Unknown)]
    public async Task GraphErrorsAreClassifiedWithoutLeakingProviderMessage(int status, int code, string expected, PublicationOutcome outcome)
    {
        var handler = new Handler { Reply = _ => { var response = Json($"{{\"error\":{{\"code\":{code},\"error_subcode\":463,\"message\":\"SECRET\",\"fbtrace_id\":\"trace-1\"}}}}", (HttpStatusCode)status); response.Headers.RetryAfter = new(TimeSpan.FromSeconds(125)); return response; } };
        var client = new MetaPublishingClient(new HttpClient(handler));
        var ex = await Assert.ThrowsAsync<MetaPublishingException>(() => client.Send(Snapshot(), Credentials, "123/feed", new(), true, default));
        var result = ex.Result(true); Assert.Equal(expected, result.ErrorCode); Assert.Equal(outcome, result.Outcome);
        Assert.Equal(125, result.RetryAfterSeconds); Assert.Equal($"{status}:{code}:463", result.ProviderCode); Assert.Equal("trace-1", result.ProviderRequestId);
        Assert.DoesNotContain("SECRET", Store.Json(result)); Assert.DoesNotContain("test-token", ex.ToString()); Assert.Single(handler.Requests);
        Assert.False(ex.Result(true, reconcile: true).ConfirmedNotPublished);
    }
    [Theory]
    [InlineData("timeout")] [InlineData("network")] [InlineData("html")] [InlineData("array")]
    public async Task AmbiguousTransportNeverClaimsPublishWasRejected(string scenario)
    {
        var handler = new Handler { Reply = _ => scenario switch { "timeout" => throw new TaskCanceledException(), "network" => throw new HttpRequestException(), "html" => Json("<html>bad</html>"), _ => Json("[]") } };
        var client = new MetaPublishingClient(new HttpClient(handler));
        var result = (await Assert.ThrowsAsync<MetaPublishingException>(() => client.Send(Snapshot(), Credentials, "123/feed", new(), true, default))).Result(true);
        Assert.Equal(PublicationOutcome.Unknown, result.Outcome); Assert.False(result.ConfirmedNotPublished); Assert.Single(handler.Requests);
    }
    [Theory]
    [InlineData("http://127.0.0.1/a")] [InlineData("http://169.254.169.254/a")] [InlineData("https://10.0.0.1/a")]
    [InlineData("http://[::1]/a")] [InlineData("http://[::ffff:127.0.0.1]/a")] [InlineData("https://example.com:444/a")]
    [InlineData("https://user:password@example.com/a")] [InlineData("file:///tmp/a")]
    [InlineData("http://[2002:7f00:1::]/a")] [InlineData("http://[2001::1]/a")]
    public void MediaRejectsLocalAndCredentialBearingUrls(string url) => Assert.False(PublishingMediaClient.Url(url));
    [Theory]
    [InlineData("mime", "INSTAGRAM_JPEG_REQUIRED_1")]
    [InlineData("size", "MEDIA_TOO_LARGE_1")]
    [InlineData("width", "INSTAGRAM_IMAGE_DIMENSIONS_INVALID_1")]
    [InlineData("ratio", "INSTAGRAM_IMAGE_DIMENSIONS_INVALID_1")]
    [InlineData("html", "INSTAGRAM_JPEG_REQUIRED_1")]
    [InlineData("redirect", "MEDIA_FETCH_FAILED_1")]
    public async Task MediaChecksBytesMimeSizeAndDimensions(string scenario, string code)
    {
        var handler = new MediaHandler();
        switch (scenario)
        {
            case "mime": handler.Mime = "image/png"; break;
            case "size": handler.Length = 9 * 1024 * 1024; break;
            case "width": handler.Width = 2000; break;
            case "ratio": handler.Height = 1920; break;
            case "html": handler.Bytes = "<html>"u8.ToArray(); break;
            case "redirect": handler.Status = HttpStatusCode.Redirect; break;
        }
        var client = new PublishingMediaClient(new HttpClient(handler));
        Assert.Equal(code, (await Assert.ThrowsAsync<MetaPublishingException>(() => client.Inspect(Snapshot(count: 1).Images[0], PlatformType.Instagram, default))).Result(false).ErrorCode);
        Assert.Equal(1, handler.Calls);
    }
    [Fact]
    public async Task InstagramRejectsNonSquareCarouselInsteadOfSilentlyCropping()
    {
        var (adapter, graph, media, _) = Create(PlatformType.Instagram); media.Height = 1350;
        var error = await Assert.ThrowsAsync<MetaPublishingException>(() => adapter.Execute(PublicationOperation.Prepare, Snapshot(PlatformType.Instagram), new(), Credentials, null, default));
        Assert.Equal("INSTAGRAM_CAROUSEL_SQUARE_REQUIRED_1", error.Result(false).ErrorCode); Assert.Empty(graph.Requests);
    }
    [Theory]
    [InlineData(PlatformType.Facebook)] [InlineData(PlatformType.Instagram)]
    public async Task ExpiredMediaResetsCheckpointOnlyBeforeAnyPublication(PlatformType platform)
    {
        var (adapter, graph, _, _) = Create(platform); var cp = new PublicationCheckpoint { PreparedAt = new Lifecycle.Clock().Now.AddDays(-1), ContainerId = "801", Media = [new(1, "801")] };
        var result = await adapter.Execute(PublicationOperation.Prepare, Snapshot(platform), cp, Credentials, null, default);
        Assert.Equal(PublicationOutcome.Rejected, result.Outcome); Assert.True(result.ConfirmedNotPublished); Assert.True(result.Retryable);
        Assert.Empty(result.Checkpoint!.Media); Assert.Null(result.Checkpoint.ContainerId); Assert.Empty(graph.Requests);
    }
}
