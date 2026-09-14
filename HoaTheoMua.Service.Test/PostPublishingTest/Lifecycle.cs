using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using HoaTheoMua.Service.Test.PostGenTest;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using PublishingService = HoaTheoMua.Service.PostPublishing.Service;
using PublishingRequest = HoaTheoMua.Service.PostPublishing.Request;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class Lifecycle
{
    internal sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.Parse("2026-09-15T10:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }
    internal sealed class Adapter : IPublishingAdapter
    {
        public PlatformType Platform => PlatformType.Facebook;
        public int Publishes; public int Calls;
        public Func<PublicationOperation, PublicationSnapshot, PublicationCheckpoint, Task<PublicationResult>>? Handle;
        public IReadOnlyList<string> Validate(PublicationSnapshot snapshot) => [];
        public Task<IReadOnlyList<string>> Preflight(PublicationSnapshot snapshot, Credentials credentials, CancellationToken ct)
        { Assert.Equal("test-token", credentials.AccessToken); return Task.FromResult<IReadOnlyList<string>>([]); }
        public Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot snapshot,
            PublicationCheckpoint checkpoint, Credentials credentials, string? candidate, CancellationToken ct)
        {
            Calls++; if (operation == PublicationOperation.Publish) Publishes++;
            return Handle?.Invoke(operation, snapshot, checkpoint) ?? Task.FromResult(operation == PublicationOperation.Prepare
                ? new PublicationResult(PublicationOutcome.Continue) { Checkpoint = new() { ReadyToPublish = true, Media = snapshot.Images.Select(x => new MediaCheckpoint(x.Order, "fake-photo-" + x.Order)).ToArray() }, RetryAfterSeconds = 1 }
                : new PublicationResult(PublicationOutcome.Succeeded) { ExternalPostId = "page_post", ExternalPostUrl = "https://www.facebook.com/page_post" });
        }
    }
    internal sealed record Setup(PublishingService Service, Worker Worker, Clock Clock, Adapter Adapter, Mock<IPublishEndpoint> Bus, PublishingOptions Options);
    internal static Setup Create(AppDbContext db, Adapter? adapter = null, Clock? clock = null, IPublishEndpoint? endpoint = null)
    {
        adapter ??= new(); clock ??= new(); var bus = new Mock<IPublishEndpoint>();
        var config = Options.Create(new PlatformAccountOptions { ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) }, Meta = new() { GraphApiVersion = "v25.0" } });
        var opts = new PublishingOptions { FacebookEnabled = true }; var settings = Options.Create(opts);
        var store = new Store(db, clock); var worker = new Worker(db, store, [adapter], settings, new TokenProtector(config), clock);
        return new(new(db, store, new GeneratePlatformPosts.User(), endpoint ?? bus.Object, [adapter], settings, config, clock, worker), worker, clock, adapter, bus, opts);
    }
    internal static async Task<(Guid PostId, Guid AccountId)> Seed(AppDbContext db)
    {
        var post = new BasePost { Id = Guid.NewGuid(), Title = "Title", Content = "Source" }; db.BasePosts.Add(post);
        var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Ready };
        db.PlatformPosts.Add(platform); await db.SaveChangesAsync();
        var content = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = platform.Id, Source = ContentSource.Manual, Title = "Title", Content = "Manual before", Hashtag = "#hoa", CreatedAt = DateTimeOffset.UtcNow };
        db.GeneratedPosts.Add(content); await db.SaveChangesAsync(); platform.OfficialGenerationId = content.Id;
        for (var order = 1; order <= 2; order++)
        {
            var original = new BasePostImage { Id = Guid.NewGuid(), BasePostId = post.Id, ImageUrl = $"https://example.com/original-{order}.jpg", SortOrder = order };
            db.BasePostImages.Add(original);
            var image = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.Id, SourceBasePostImageId = original.Id, RatioCode = AspectRatioType.Square1x1,
                Status = ImageStatus.Succeeded, ImageUrl = $"https://example.com/generated-{order}.jpg", SourceImageUrl = original.ImageUrl };
            db.GeneratedPostImages.Add(image);
            db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id, SourceBasePostImageId = original.Id,
                RatioCode = image.RatioCode, SelectedGeneratedPostImageId = image.Id, SortOrder = order });
        }
        var pageId = "page-" + Guid.NewGuid().ToString("N");
        var account = new Repository.Entity.PlatformAccount { Id = Guid.NewGuid(), Platform = PlatformType.Facebook, ExternalAccountId = pageId, PageId = pageId,
            DisplayName = "Test Page", GrantedScopes = ["pages_manage_posts", "pages_read_engagement"], Revision = 1 };
        var protector = new TokenProtector(Options.Create(new PlatformAccountOptions { ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) } }));
        account.ProtectedCredentials = protector.Protect(Store.Json(new Credentials { AccessToken = "test-token" }), $"postgen:account:{account.Id}");
        db.PlatformAccounts.Add(account); await db.SaveChangesAsync(); return (post.Id, account.Id);
    }
    internal static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    internal static async Task<PublishingRequest.Publish> Input(Setup setup, Guid id, Guid account) => new()
        { AccountId = account, IdempotencyKey = "publish-one", ExpectedRevision = (await setup.Service.Preview(id, "facebook", account)).Revision };

    [Fact]
    public async Task PreviewIsReadOnlyAndPublishIsIdempotentWithoutCallingProvider()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var input = await Input(s, id, account); Assert.Empty(db.PostPublications); Assert.Equal(0, s.Adapter.Calls);
        var job = await s.Service.Publish(id, "facebook", input);
        var again = await s.Service.Publish(id, "facebook", input);
        Assert.Equal(job, again); Assert.Single(db.PostPublications); Assert.Equal(0, s.Adapter.Calls);
        input.ExpectedRevision = new string('A', 64);
        Assert.Equal("IDEMPOTENCY_CONFLICT", (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Publish(id, "facebook", input))).MessageCode);
        Assert.Equal(PlatformPostStatus.Publishing, (await db.PlatformPosts.SingleAsync()).Status);
    }
    [Fact]
    public async Task ManualEditAndAccountRevisionInvalidatePreview()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db); var input = await Input(s, id, account);
        (await db.GeneratedPosts.SingleAsync()).Content = "After"; await db.SaveChangesAsync();
        Assert.Equal("PUBLICATION_PREVIEW_CHANGED", (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Publish(id, "facebook", input))).MessageCode);
        input = await Input(s, id, account); (await db.PlatformAccounts.SingleAsync()).Revision++; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<PublicationException>(() => s.Service.Publish(id, "facebook", input)); Assert.Empty(db.PostPublications);
    }
    [Fact]
    public async Task WorkerUsesSnapshotAndCheckpointsThenIgnoresDuplicateMessages()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        // Simulate a legacy writer: the durable snapshot must still reproduce the requested content.
        (await db.GeneratedPosts.SingleAsync()).Content = "Changed outside service"; await db.SaveChangesAsync();
        await s.Worker.Process(job.PublicationId, job.Revision, default);
        await s.Worker.Process(job.PublicationId, job.Revision, default); Assert.Equal(1, s.Adapter.Calls);
        var prepared = (await s.Service.Get(job.PublicationId)).Job; s.Clock.Now = s.Clock.Now.AddMinutes(2);
        s.Adapter.Handle = (op, snapshot, checkpoint) =>
        {
            Assert.Equal(PublicationOperation.Publish, op); Assert.Equal("Manual before", snapshot.Content);
            Assert.Equal(new[] { 1, 2 }, checkpoint.Media.Select(x => x.Order));
            return Task.FromResult(new PublicationResult(PublicationOutcome.Succeeded) { ExternalPostId = "page_post" });
        };
        await s.Worker.Process(job.PublicationId, prepared.Revision, default);
        await s.Worker.Process(job.PublicationId, prepared.Revision, default);
        var detail = await s.Service.Get(job.PublicationId);
        Assert.Equal("Succeeded", detail.Job.Status); Assert.Equal(1, s.Adapter.Publishes); Assert.Equal(2, detail.Attempts.Count);
        Assert.DoesNotContain("test-token", Store.Json(detail));
        var repeatPreview = await s.Service.Preview(id, "facebook", account);
        await Assert.ThrowsAsync<PublicationException>(() => s.Service.Publish(id, "facebook", new() { AccountId = account, IdempotencyKey = "new", ExpectedRevision = repeatPreview.Revision }));
    }
    [Theory]
    [InlineData("timeout")]
    [InlineData("missing-id")]
    [InlineData("unknown")]
    public async Task AmbiguousPublishNeverAutomaticallyPublishesAgain(string scenario)
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        await s.Worker.Process(job.PublicationId, job.Revision, default); s.Clock.Now = s.Clock.Now.AddMinutes(2);
        s.Adapter.Handle = (op, _, _) =>
        {
            if (op == PublicationOperation.Reconcile) return Task.FromResult(new PublicationResult(PublicationOutcome.Unknown));
            if (scenario == "timeout") throw new TimeoutException("token=SECRET");
            return Task.FromResult(new PublicationResult(scenario == "missing-id" ? PublicationOutcome.Succeeded : PublicationOutcome.Unknown));
        };
        await s.Worker.Process(job.PublicationId, (await s.Service.Get(job.PublicationId)).Job.Revision, default);
        var unknown = (await s.Service.Get(job.PublicationId)).Job; Assert.Equal("NeedsReview", unknown.Status); Assert.False(unknown.CanRetry);
        await Assert.ThrowsAsync<PublicationException>(() => s.Service.Retry(job.PublicationId, new() { ExpectedRevision = unknown.Revision }));
        await s.Service.Recheck(job.PublicationId, new() { ExpectedRevision = unknown.Revision });
        Assert.Equal(1, s.Adapter.Publishes); Assert.Equal("NeedsReview", (await s.Service.Get(job.PublicationId)).Job.Status);
        Assert.DoesNotContain("SECRET", Store.Json(await s.Service.Get(job.PublicationId)));
    }
    [Fact]
    public async Task ConfirmPublishedRequiresProviderEvidenceAndRecordsActor()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        var entity = await db.PostPublications.SingleAsync(); entity.Status = "NeedsReview"; entity.PublishMayHaveOccurred = true; await db.SaveChangesAsync();
        s.Adapter.Handle = (_, _, _) => Task.FromResult(new PublicationResult(PublicationOutcome.Unknown));
        var first = await s.Service.Resolve(job.PublicationId, new() { ExpectedRevision = job.Revision, Action = "ConfirmPublished", Note = "Found candidate", ExternalPostId = "candidate" });
        Assert.Equal("NeedsReview", first.Status);
        s.Adapter.Handle = (op, _, _) => { Assert.Equal(PublicationOperation.Reconcile, op); return Task.FromResult(new PublicationResult(PublicationOutcome.Succeeded) { ExternalPostId = "verified" }); };
        var resolved = await s.Service.Resolve(job.PublicationId, new() { ExpectedRevision = first.Revision, Action = "ConfirmPublished", Note = "Verified candidate", ExternalPostId = "verified" });
        Assert.Equal("Succeeded", resolved.Status); Assert.Equal("verified", resolved.ExternalPostId); Assert.Equal(0, s.Adapter.Publishes);
        Assert.Equal(2, await db.PostPublicationReviews.CountAsync());
        Assert.All(await db.PostPublicationReviews.ToListAsync(), r => Assert.Equal(new GeneratePlatformPosts.User().GetRequiredUserId(), r.ActorId));
    }
    [Fact]
    public async Task ClosingUnresolvedDoesNotAllowReposting()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        var entity = await db.PostPublications.SingleAsync(); entity.Status = "NeedsReview"; entity.PublishMayHaveOccurred = true; await db.SaveChangesAsync();
        var closed = await s.Service.Resolve(job.PublicationId, new() { ExpectedRevision = job.Revision, Action = "Close", Note = "Stop investigation" });
        Assert.Equal("Closed", closed.Status); Assert.True(closed.PublishMayHaveOccurred); Assert.False(closed.CanRetry);
        var preview = await s.Service.Preview(id, "facebook", account); Assert.False(preview.CanPublish); Assert.Contains("PUBLICATION_EXISTS", preview.Reasons);
    }
    [Fact]
    public async Task CancellationInvalidatesQueuedMessageAndUnlocksDraft()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        await s.Service.Cancel(job.PublicationId, new() { ExpectedRevision = job.Revision });
        await s.Worker.Process(job.PublicationId, job.Revision, default);
        Assert.Equal(0, s.Adapter.Calls); Assert.Equal(PlatformPostStatus.Ready, (await db.PlatformPosts.SingleAsync()).Status);
    }
    [Theory]
    [InlineData("Queued")]
    [InlineData("Publishing")]
    [InlineData("Scheduled")]
    [InlineData("NeedsReview")]
    public async Task ActiveJobLocksSourceAndOfficialContentButNotReadHistory(string status)
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        (await db.PostPublications.SingleAsync()).Status = status; await db.SaveChangesAsync();
        var core = new HoaTheoMua.Service.PostCore.Service(db);
        await Assert.ThrowsAsync<PublicationException>(() => core.UpdatePostCore(id, new() { Title = "Changed", Content = "Changed", Images = [] }));
        await Assert.ThrowsAsync<PublicationException>(() => core.DeletePostCore(id));
        var gen = GeneratePlatformPosts.Service(db, Mock.Of<HoaTheoMua.Service.AiService.IService>());
        await Assert.ThrowsAsync<PublicationException>(() => gen.EditPlatformContent(id, "facebook", new() { Content = "Changed" }));
        Assert.NotNull(await gen.GetContentVersions(id, "facebook")); Assert.NotNull(await s.Service.Get(job.PublicationId));
    }
    [Fact]
    public async Task ExpiredPublishLeaseBecomesNeedsReviewWithoutSending()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        var entity = await db.PostPublications.SingleAsync(); entity.Status = "Publishing"; entity.PublishMayHaveOccurred = true;
        entity.LeaseId = Guid.NewGuid(); entity.LeaseUntil = s.Clock.Now.AddSeconds(-1); await db.SaveChangesAsync();
        await s.Worker.Process(job.PublicationId, job.Revision, default);
        Assert.Equal("NeedsReview", (await s.Service.Get(job.PublicationId)).Job.Status); Assert.Equal(0, s.Adapter.Calls);
    }
    [Fact]
    public async Task DisabledChannelCreatesNoJobAndMakesNoProviderCalls()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db); s.Options.FacebookEnabled = false;
        var input = await Input(s, id, account);
        Assert.Equal(503, (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Publish(id, "facebook", input))).StatusCode);
        Assert.Empty(db.PostPublications); Assert.Equal(0, s.Adapter.Calls);
    }

    [Fact]
    public async Task RetryRequiresPositiveAbsenceEvidenceAndPreservesOriginalSnapshot()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        var entity = await db.PostPublications.SingleAsync(); entity.Status = "NeedsReview"; entity.PublishMayHaveOccurred = true;
        entity.Checkpoint = Store.Json(new PublicationCheckpoint { ReadyToPublish = true }); await db.SaveChangesAsync();
        s.Adapter.Handle = (op, _, _) =>
        {
            Assert.Equal(PublicationOperation.Reconcile, op);
            return Task.FromResult(new PublicationResult(PublicationOutcome.Rejected) { ConfirmedNotPublished = true, Retryable = true });
        };
        var failed = await s.Service.Resolve(job.PublicationId, new() { ExpectedRevision = job.Revision, Action = "ConfirmNotPublished", Note = "Provider confirms no post" });
        Assert.Equal("Failed", failed.Status); Assert.True(failed.CanRetry); Assert.False(failed.PublishMayHaveOccurred);
        (await db.GeneratedPosts.SingleAsync()).Content = "New draft"; await db.SaveChangesAsync();
        var retry = await s.Service.Retry(job.PublicationId, new() { ExpectedRevision = failed.Revision });
        s.Adapter.Handle = (op, snapshot, _) =>
        {
            Assert.Equal("Manual before", snapshot.Content); Assert.Equal(PublicationOperation.Publish, op);
            return Task.FromResult(new PublicationResult(PublicationOutcome.Succeeded) { ExternalPostId = "verified" });
        };
        await s.Worker.Process(job.PublicationId, retry.Revision, default);
        Assert.Equal("Succeeded", (await s.Service.Get(job.PublicationId)).Job.Status); Assert.Single(db.PostPublications); Assert.Equal(1, s.Adapter.Publishes);
    }

    [Fact]
    public async Task StepBudgetDoesNotLeavePreparationStuckOrPretendPublished()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db); s.Options.MaxSteps = 1;
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        await s.Worker.Process(job.PublicationId, job.Revision, default);
        var stopped = (await s.Service.Get(job.PublicationId)).Job;
        Assert.Equal("Failed", stopped.Status); Assert.Equal("PUBLICATION_BUDGET_EXCEEDED", stopped.ErrorCode);
        Assert.False(stopped.PublishMayHaveOccurred); Assert.Equal(0, s.Adapter.Publishes);
    }

    [Fact]
    public async Task CannotCancelOrOverrideReviewWhileLeaseIsHeld()
    {
        await using var db = Db(); var (id, account) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, account));
        var entity = await db.PostPublications.SingleAsync(); entity.Status = "NeedsReview"; entity.PublishMayHaveOccurred = true;
        entity.LeaseId = Guid.NewGuid(); entity.LeaseUntil = s.Clock.Now.AddMinutes(2); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<PublicationException>(() => s.Service.Resolve(job.PublicationId, new() { ExpectedRevision = job.Revision, Action = "Close", Note = "Close" }));
        await s.Worker.Process(job.PublicationId, job.Revision, default);
        Assert.Equal(0, s.Adapter.Calls); Assert.Empty(db.PostPublicationReviews);
    }

    [Fact]
    public async Task CorruptedCredentialPayloadStopsBeforeProviderAndDoesNotLoopQueued()
    {
        await using var db = Db(); var (id, accountId) = await Seed(db); var s = Create(db);
        var job = await s.Service.Publish(id, "facebook", await Input(s, id, accountId));
        var protector = new TokenProtector(Options.Create(new PlatformAccountOptions { ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) } }));
        (await db.PlatformAccounts.SingleAsync()).ProtectedCredentials = protector.Protect("null", $"postgen:account:{accountId}");
        await db.SaveChangesAsync(); await s.Worker.Process(job.PublicationId, job.Revision, default);
        var stopped = (await s.Service.Get(job.PublicationId)).Job;
        Assert.Equal("Failed", stopped.Status); Assert.Equal("PLATFORM_TOKEN_UNAVAILABLE", stopped.ErrorCode);
        Assert.True(stopped.CanRetry); Assert.Equal(0, s.Adapter.Calls); Assert.Null((await db.PostPublications.SingleAsync()).NextRunAt);
    }
}
