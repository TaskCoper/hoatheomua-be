using System.Net;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using HoaTheoMua.Service.Test.PostGenTest;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class AdapterWorker
{
    private static async Task<(HoaTheoMua.Service.PostPublishing.Service Service, Worker Worker, Lifecycle.Clock Clock, Guid Post, Guid Account)> Setup(AppDbContext db, IPublishingAdapter adapter)
    {
        var (post, account) = await Lifecycle.Seed(db);
        var target = await db.PlatformPosts.SingleAsync(); target.Platform = adapter.Platform;
        var entity = await db.PlatformAccounts.SingleAsync(); entity.Platform = adapter.Platform; entity.PageId = "123";
        entity.ExternalAccountId = adapter.Platform == PlatformType.Facebook ? "123" : "456";
        entity.GrantedScopes = ["pages_manage_posts", "pages_read_engagement", "instagram_basic", "instagram_content_publish"];
        await db.SaveChangesAsync();
        var options = Options.Create(new PublishingOptions { FacebookEnabled = true, InstagramEnabled = true });
        var config = Options.Create(new PlatformAccountOptions { ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) }, Meta = new() { Enabled = true, GraphApiVersion = "v25.0" } });
        var clock = new Lifecycle.Clock(); var store = new Store(db, clock); var worker = new Worker(db, store, [adapter], options, new TokenProtector(config), clock);
        return (new(db, store, new GeneratePlatformPosts.User(), Mock.Of<IPublishEndpoint>(), [adapter], options, config, clock, worker), worker, clock, post, account);
    }
    [Theory]
    [InlineData(PlatformType.Facebook, "success")]
    [InlineData(PlatformType.Facebook, "timeout")]
    [InlineData(PlatformType.Facebook, "server-error")]
    [InlineData(PlatformType.Facebook, "auth")]
    [InlineData(PlatformType.Facebook, "rate-limit")]
    [InlineData(PlatformType.Instagram, "success")]
    [InlineData(PlatformType.Instagram, "timeout")]
    [InlineData(PlatformType.Instagram, "server-error")]
    [InlineData(PlatformType.Instagram, "auth")]
    [InlineData(PlatformType.Instagram, "rate-limit")]
    public async Task RealAdapterWorkerPersistsCheckpointsAndNeverRepeatsAmbiguousPublish(PlatformType platform, string scenario)
    {
        await using var db = Lifecycle.Db(); var (adapter, graph, _, _) = Adapters.Create(platform); var setup = await Setup(db, adapter);
        var creates = 0; var publishes = 0;
        graph.Reply = r =>
        {
            if (r.Path.EndsWith("/content_publishing_limit")) return Adapters.Json("{\"data\":[{\"quota_usage\":0,\"config\":{\"quota_total\":50}}]}");
            if (r.Method == "GET") return r.Form["fields"] == "status_code" ? Adapters.Json($"{{\"id\":\"{r.Path.Split('/').Last()}\",\"status_code\":\"FINISHED\"}}") : Adapters.Json("{\"id\":\"999\"}");
            if (r.Path.EndsWith("/feed") || r.Path.EndsWith("/media_publish"))
            {
                publishes++;
                var durable = db.PostPublications.AsNoTracking().Single(); Assert.True(durable.PublishMayHaveOccurred); Assert.NotNull(durable.LeaseId);
                Assert.Equal(2, Store.Read<PublicationCheckpoint>(durable.Checkpoint).Media.Length);
                return scenario switch
                {
                    "timeout" => throw new TaskCanceledException("SECRET"),
                    "server-error" => Adapters.Json("{\"error\":{\"code\":2}}", HttpStatusCode.InternalServerError),
                    "auth" => Adapters.Json("{\"error\":{\"code\":190}}", HttpStatusCode.BadRequest),
                    "rate-limit" => Adapters.Json("{\"error\":{\"code\":4}}", HttpStatusCode.TooManyRequests),
                    _ => Adapters.Json(platform == PlatformType.Facebook ? "{\"id\":\"123_999\"}" : "{\"id\":\"999\"}")
                };
            }
            return Adapters.Json($"{{\"id\":\"{800 + ++creates}\"}}");
        };
        var key = platform.ToString().ToLowerInvariant();
        var preview = await setup.Service.Preview(setup.Post, key, setup.Account);
        Assert.True(preview.CanPublish); Assert.Empty(graph.Requests);
        var job = await setup.Service.Publish(setup.Post, key, new() { AccountId = setup.Account, IdempotencyKey = "one", ExpectedRevision = preview.Revision });
        for (var i = 0; i < 20; i++)
        {
            await setup.Worker.Process(job.PublicationId, job.Revision, default);
            await setup.Worker.Process(job.PublicationId, job.Revision, default); // Duplicate delivery.
            job = (await setup.Service.Get(job.PublicationId)).Job;
            if (job.Status is "Failed" or "Succeeded" or "NeedsReview") break;
            setup.Clock.Now = setup.Clock.Now.AddMinutes(2);
        }
        Assert.Equal(scenario == "success" ? "Succeeded" : scenario is "timeout" or "server-error" ? "NeedsReview" : "Failed", job.Status);
        Assert.Equal(1, publishes); Assert.Equal(platform == PlatformType.Facebook ? 2 : 3, creates);
        if (job.Status == "NeedsReview")
        {
            await setup.Service.Recheck(job.PublicationId, new() { ExpectedRevision = job.Revision });
            Assert.Equal(1, publishes);
        }
        if (scenario == "rate-limit")
        {
            var error = await Assert.ThrowsAsync<PublicationException>(() => setup.Service.Retry(job.PublicationId, new() { ExpectedRevision = job.Revision }));
            Assert.Equal(429, error.StatusCode);
        }
        Assert.Equal(scenario == "auth" ? "ReauthRequired" : "Connected", (await db.PlatformAccounts.SingleAsync()).Status);
        Assert.DoesNotContain("SECRET", Store.Json(await setup.Service.Get(job.PublicationId)));
    }
    [Fact]
    public async Task PreflightTimeoutBeforePublishDoesNotLeaveAmbiguousIntent()
    {
        await using var db = Lifecycle.Db(); var (adapter, _, _, accounts) = Adapters.Create(); var setup = await Setup(db, adapter);
        var preview = await setup.Service.Preview(setup.Post, "facebook", setup.Account);
        var job = await setup.Service.Publish(setup.Post, "facebook", new() { AccountId = setup.Account, IdempotencyKey = "one", ExpectedRevision = preview.Revision });
        var entity = await db.PostPublications.SingleAsync(); entity.Checkpoint = Store.Json(new PublicationCheckpoint { ReadyToPublish = true }); await db.SaveChangesAsync();
        accounts.Setup(x => x.Refresh(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>())).ThrowsAsync(new TaskCanceledException());
        await setup.Worker.Process(job.PublicationId, job.Revision, default);
        var result = (await setup.Service.Get(job.PublicationId)).Job;
        Assert.Equal("Failed", result.Status); Assert.False(result.PublishMayHaveOccurred); Assert.True(result.CanRetry);
    }
    [Fact]
    public async Task StaleAuthFailureCannotInvalidateNewAccountCredentials()
    {
        await using var db = Lifecycle.Db(); var (adapter, _, _, accounts) = Adapters.Create(); var setup = await Setup(db, adapter);
        var preview = await setup.Service.Preview(setup.Post, "facebook", setup.Account);
        var job = await setup.Service.Publish(setup.Post, "facebook", new() { AccountId = setup.Account, IdempotencyKey = "one", ExpectedRevision = preview.Revision });
        accounts.Setup(x => x.Refresh(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>())).Returns(async () =>
        {
            var account = await db.PlatformAccounts.SingleAsync(); account.Revision++; account.Status = "Connected"; await db.SaveChangesAsync();
            throw new AccountException(400, "PLATFORM_REAUTH_REQUIRED", "Old token expired");
        });
        await setup.Worker.Process(job.PublicationId, job.Revision, default);
        Assert.Equal("Connected", (await db.PlatformAccounts.SingleAsync()).Status);
    }
    [Fact]
    public async Task RateLimitCooldownAlsoDefersAnotherPostOnTheSameAccount()
    {
        await using var db = Lifecycle.Db(); var (adapter, graph, _, _) = Adapters.Create(); var setup = await Setup(db, adapter);
        var preview = await setup.Service.Preview(setup.Post, "facebook", setup.Account);
        await setup.Service.Publish(setup.Post, "facebook", new() { AccountId = setup.Account, IdempotencyKey = "first", ExpectedRevision = preview.Revision });
        var first = await db.PostPublications.SingleAsync(); first.Status = "Failed"; first.ErrorCode = "META_RATE_LIMITED"; first.NextRunAt = setup.Clock.Now.AddMinutes(10); await db.SaveChangesAsync();
        var (post, _) = await Lifecycle.Seed(db);
        var next = await setup.Service.Preview(post, "facebook", setup.Account);
        var job = await setup.Service.Publish(post, "facebook", new() { AccountId = setup.Account, IdempotencyKey = "second", ExpectedRevision = next.Revision });
        await setup.Worker.Process(job.PublicationId, job.Revision, default);
        var delayed = await db.PostPublications.SingleAsync(x => x.Id == job.PublicationId);
        Assert.Equal("Queued", delayed.Status); Assert.Equal(first.NextRunAt, delayed.NextRunAt); Assert.Equal(0, delayed.AttemptCount); Assert.Empty(graph.Requests);
    }
}
