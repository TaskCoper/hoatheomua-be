using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.FlowerJobs;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using Flower = HoaTheoMua.Service.Flower;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class FlowerJobsTests
{
    internal static HoaTheoMua.Service.FlowerJobs.Service Jobs(FlowerGeneration.Fixture f, Mock<IPublishEndpoint>? bus = null) => new(f.Db, f.Service,
        f.Current.Object, (bus ?? new()).Object, new Store(f.Db, TimeProvider.System), TimeProvider.System, Options.Create(new FlowerJobOptions()));

    internal static Worker Worker(FlowerGeneration.Fixture f, IServiceScopeFactory? scopes = null) => new(f.Db,
        new Store(f.Db, TimeProvider.System), f.Ai.Object, f.Branding.Object, f.Service,
        scopes ?? new Mock<IServiceScopeFactory>().Object, Options.Create(new FlowerJobOptions()), TimeProvider.System, NullLogger<Worker>.Instance);

    internal static void ConfigureAi(FlowerGeneration.Fixture f)
    {
        f.Ai.Setup(x => x.GenerateFlowerOnly(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ai.Response.GeneratedImage("https://example.com/flower.png", "test", "first", 10));
        f.Ai.Setup(x => x.ApplyFlowerMockup(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<Ai.Response.GeneratedImage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ai.Response.GeneratedImage("https://example.com/mockup.png", "test", "second", 10));
    }

    [Fact]
    public async Task SubmitAcknowledgesWithoutAiAndReplaysSameKeyWhileRejectingChangedInput()
    {
        using var f = new FlowerGeneration.Fixture(); var bus = new Mock<IPublishEndpoint>(); var jobs = Jobs(f, bus);
        var first = await jobs.Create(f.Request, "stable-key", default);
        Assert.Equal("Queued", first.Status); Assert.Equal(f.Request.Name, first.DisplayName);
        Assert.NotNull(first.ProductImageUrl); Assert.NotNull(first.MockupImageUrl); Assert.Empty(f.Inputs);
        var again = await jobs.Create(f.Request, "stable-key", default); Assert.Equal(first.JobId, again.JobId);
        Assert.Single(f.Db.FlowerGenerationJobs);
        bus.Verify(x => x.Publish(It.IsAny<FlowerGenerationRequested>(), It.IsAny<CancellationToken>()), Times.Once);
        f.Request.Name = "Changed";
        await Assert.ThrowsAsync<ConflictException>(() => jobs.Create(f.Request, "stable-key", default));
    }

    [Fact]
    public async Task WorkerUsesSnapshotSurvivesRequestAbortAndDuplicateDeliveryDoesNotGenerateAgain()
    {
        using var f = new FlowerGeneration.Fixture(); ConfigureAi(f); var jobs = Jobs(f);
        var job = await jobs.Create(f.Request, "snapshot", default);
        f.Db.Configs.RemoveRange(f.Db.Configs); await f.Db.SaveChangesAsync();
        f.Context.HttpContext!.RequestAborted = new CancellationToken(true);
        var worker = Worker(f); await worker.Process(job.JobId, default); await worker.Process(job.JobId, default);
        var result = await jobs.Get(job.JobId, default);
        Assert.Equal("Succeeded", result.Status); Assert.NotNull(result.Result);
        Assert.Single(f.Db.GeneratedFlowers); Assert.Single(f.Db.ClientHistories);
        f.Ai.Verify(x => x.GenerateFlowerOnly(It.Is<Ai.Request.FlowerImageInput>(i => i.CreateSystemPrompt == "create exact\n"), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(JobJson.Read<Checkpoint>((await f.Db.FlowerGenerationJobs.SingleAsync()).Checkpoint).Branded);
    }

    [Fact]
    public async Task MockupFailureKeepsFirstCheckpointAndNeverRetriesProviderOnRedelivery()
    {
        using var f = new FlowerGeneration.Fixture(); ConfigureAi(f);
        f.Ai.Setup(x => x.ApplyFlowerMockup(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<Ai.Response.GeneratedImage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("provider outcome unknown"));
        var job = await Jobs(f).Create(f.Request, "partial", default); var worker = Worker(f);
        await worker.Process(job.JobId, default); await worker.Process(job.JobId, default);
        var saved = await f.Db.FlowerGenerationJobs.SingleAsync();
        Assert.Equal("Failed", saved.Status); Assert.NotNull(JobJson.Read<Checkpoint>(saved.Checkpoint).Flower);
        Assert.Empty(f.Db.GeneratedFlowers);
        f.Ai.Verify(x => x.ApplyFlowerMockup(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<Ai.Response.GeneratedImage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelQueuedAndAuthorizationProtectBothReadsAndWrites()
    {
        using var f = new FlowerGeneration.Fixture(); var jobs = Jobs(f);
        var job = await jobs.Create(f.Request, "cancel", default);
        var owner = f.Current.Object.GetRequiredUserId(); f.Current.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(() => jobs.Get(job.JobId, default));
        await Assert.ThrowsAsync<NotFoundException>(() => jobs.Cancel(job.JobId, default));
        f.Current.Setup(x => x.GetRequiredUserId()).Returns(owner);
        Assert.Equal("Canceled", (await jobs.Cancel(job.JobId, default)).Status);
        await Worker(f).Process(job.JobId, default); Assert.Empty(f.Db.GeneratedFlowers);
    }

    [Fact]
    public async Task CancelDuringMockupRetainsCheckpointAndNeverCommitsFlower()
    {
        using var f = new FlowerGeneration.Fixture(); ConfigureAi(f); var jobs = Jobs(f);
        var job = await jobs.Create(f.Request, "race", default);
        f.Ai.Setup(x => x.ApplyFlowerMockup(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<Ai.Response.GeneratedImage>(), It.IsAny<CancellationToken>()))
            .Returns(async () => { await jobs.Cancel(job.JobId, default); return new Ai.Response.GeneratedImage("https://example.com/mockup.png", "test", "second", 10); });
        await Worker(f).Process(job.JobId, default);
        Assert.Equal("Canceled", (await jobs.Get(job.JobId, default)).Status); Assert.Empty(f.Db.GeneratedFlowers);
        Assert.NotNull(JobJson.Read<Checkpoint>((await f.Db.FlowerGenerationJobs.SingleAsync()).Checkpoint).Mockup);
    }

    [Fact]
    public async Task RecoveryFailsExpiredWorkerWithoutRepeatingPaidWorkAndRedispatchesQueuedJobs()
    {
        using var f = new FlowerGeneration.Fixture(); var jobs = Jobs(f);
        var running = await jobs.Create(f.Request, "running", default); var queued = await jobs.Create(f.Request, "queued", default);
        var stale = await f.Db.FlowerGenerationJobs.SingleAsync(x => x.Id == running.JobId);
        stale.Status = "Running"; stale.LeaseId = Guid.NewGuid(); stale.LeaseUntil = DateTimeOffset.UtcNow.AddMinutes(-1);
        (await f.Db.FlowerGenerationJobs.SingleAsync(x => x.Id == queued.JobId)).LastDispatchedAt = DateTimeOffset.UtcNow.AddMinutes(-2);
        await f.Db.SaveChangesAsync(); var bus = new Mock<IPublishEndpoint>();
        await new Recovery(f.Db, bus.Object, new Store(f.Db, TimeProvider.System), TimeProvider.System).Scan(default);
        Assert.Equal("Failed", (await jobs.Get(running.JobId, default)).Status);
        bus.Verify(x => x.Publish(It.Is<FlowerGenerationRequested>(m => m.JobId == queued.JobId), It.IsAny<CancellationToken>()), Times.Once);
        await Worker(f).Process(running.JobId, default); Assert.Empty(f.Db.GeneratedFlowers);
    }

    [Fact]
    public async Task QuotaAndRegenerationPreserveSessionAndLineage()
    {
        using var f = new FlowerGeneration.Fixture(); ConfigureAi(f); var jobs = Jobs(f);
        var first = await jobs.Create(f.Request, "first", default); await Worker(f).Process(first.JobId, default);
        var result = (await jobs.Get(first.JobId, default)).Result!;
        var regen = await jobs.Regenerate(result.Id, new(), "regen", default); await Worker(f).Process(regen.JobId, default);
        var regenerated = (await jobs.Get(regen.JobId, default)).Result!;
        Assert.Equal(result.SessionId, regenerated.SessionId); Assert.NotEqual(result.Id, regenerated.Id);
        for (var i = 0; i < 3; i++) await jobs.Create(f.Request, "quota-" + i, default);
        await Assert.ThrowsAsync<ConflictException>(() => jobs.Create(f.Request, "over-limit", default));
    }
}
