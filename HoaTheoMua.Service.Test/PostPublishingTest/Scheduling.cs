using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public class Scheduling
{
    internal static async Task<Request.Schedule> Input(Lifecycle.Setup s, Guid post, Guid account, int minutes = 10) => new()
    {
        AccountId = account, IdempotencyKey = "schedule-one", ExpectedRevision = (await s.Service.Preview(post, "facebook", account)).Revision,
        ScheduledAt = s.Clock.Now.AddMinutes(minutes).ToOffset(TimeSpan.FromHours(7)).ToString("O")
    };
    [Fact]
    public async Task ScheduleFreezesSnapshotStoresUtcAndDoesNotSendEarly()
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var input = await Input(s, id, account); var job = await s.Service.Schedule(id, "facebook", input);
        Assert.Equal("Scheduled", job.Status); Assert.Equal(s.Clock.Now.AddMinutes(10), job.ScheduledAt); Assert.Equal(TimeSpan.Zero, job.ScheduledAt!.Value.Offset);
        Assert.Empty(s.Bus.Invocations); Assert.Equal(0, s.Adapter.Calls); Assert.Empty(db.PostPublicationAttempts);
        var recovery = new Recovery(db, s.Bus.Object, s.Clock); Assert.Equal(0, await recovery.Dispatch(default));
        await s.Worker.Process(job.PublicationId, job.Revision, default); Assert.Equal(0, s.Adapter.Calls);
        Assert.Equal(PlatformPostStatus.Scheduled, (await db.PlatformPosts.SingleAsync()).Status);
        Assert.Equal(job.ScheduledAt, (await db.PlatformPosts.SingleAsync()).ScheduledAt);
        var detail = await s.Service.Get(job.PublicationId); Assert.Equal("Manual before", detail.Snapshot.Content); Assert.Equal("Schedule", Assert.Single(detail.Reviews).Action);
    }
    [Fact]
    public async Task SameInstantWithDifferentOffsetIsIdempotentEvenAfterDueTime()
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var input = await Input(s, id, account); var first = await s.Service.Schedule(id, "facebook", input);
        input.ScheduledAt = s.Clock.Now.AddMinutes(10).ToString("O"); s.Clock.Now = s.Clock.Now.AddHours(1);
        var repeat = await s.Service.Schedule(id, "facebook", input); Assert.Equal(first, repeat); Assert.Single(db.PostPublications); Assert.Single(db.PostPublicationReviews);
        input.ScheduledAt = s.Clock.Now.AddMinutes(20).ToString("O");
        Assert.Equal("IDEMPOTENCY_CONFLICT", (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Schedule(id, "facebook", input))).MessageCode);
        Assert.Equal("IDEMPOTENCY_CONFLICT", (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Publish(id, "facebook", input))).MessageCode);
    }
    [Theory]
    [InlineData("2026-09-15T10:10:00")]
    [InlineData("2026-09-15")]
    [InlineData("2026-09-15T10:10:00+15:00")]
    [InlineData("2026-02-30T10:10:00Z")]
    [InlineData("")]
    public async Task RejectsMissingOffsetOrInvalidTime(string date)
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var input = await Input(s, id, account); input.ScheduledAt = date;
        Assert.Equal("PUBLICATION_SCHEDULE_OFFSET_REQUIRED", (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Schedule(id, "facebook", input))).MessageCode);
        Assert.Empty(db.PostPublications); Assert.Empty(s.Bus.Invocations);
    }
    [Theory]
    [InlineData(-1, false)] [InlineData(0, false)] [InlineData(59, false)] [InlineData(60, true)]
    [InlineData(2592000, true)] [InlineData(2592001, false)]
    public async Task LeadTimeAndHorizonHaveInclusiveBoundaries(int seconds, bool allowed)
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var input = await Input(s, id, account); input.ScheduledAt = s.Clock.Now.AddSeconds(seconds).ToString("O");
        if (allowed) Assert.Equal("Scheduled", (await s.Service.Schedule(id, "facebook", input)).Status);
        else Assert.Equal("PUBLICATION_SCHEDULE_OUT_OF_RANGE", (await Assert.ThrowsAsync<PublicationException>(() => s.Service.Schedule(id, "facebook", input))).MessageCode);
    }
    [Fact]
    public async Task RescheduleRetainsSnapshotAndInvalidatesAlreadyDispatchedMessage()
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var input = await Input(s, id, account); var first = await s.Service.Schedule(id, "facebook", input);
        var snapshot = (await db.PostPublications.SingleAsync()).Snapshot; s.Clock.Now = first.ScheduledAt!.Value;
        Assert.Equal(1, await new Recovery(db, s.Bus.Object, s.Clock).Dispatch(default));
        var changed = await s.Service.Reschedule(id, "facebook", new() { PublicationId = first.PublicationId, ExpectedRevision = first.Revision, ScheduledAt = s.Clock.Now.AddHours(1).ToString("O") });
        await s.Worker.Process(first.PublicationId, first.Revision, default); await s.Worker.Process(changed.PublicationId, changed.Revision, default);
        Assert.Equal(0, s.Adapter.Calls); Assert.Equal(snapshot, (await db.PostPublications.SingleAsync()).Snapshot);
        Assert.Equal(changed, await s.Service.Schedule(id, "facebook", input)); // Original create retry returns current job.
        await Assert.ThrowsAsync<PublicationException>(() => s.Service.Reschedule(id, "facebook", new() { PublicationId = changed.PublicationId, ExpectedRevision = first.Revision, ScheduledAt = s.Clock.Now.AddHours(2).ToString("O") }));
        Assert.Equal(new[] { "Schedule", "Reschedule" }, (await s.Service.Get(first.PublicationId)).Reviews.Select(x => x.Action));
        s.Clock.Now = changed.ScheduledAt!.Value; await s.Worker.Process(changed.PublicationId, changed.Revision, default); Assert.Equal(1, s.Adapter.Calls);
    }
    [Fact]
    public async Task CancelScheduleUnlocksSourceAndStaleMessageCannotStartIt()
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var job = await s.Service.Schedule(id, "facebook", await Input(s, id, account));
        var core = new HoaTheoMua.Service.PostCore.Service(db);
        await Assert.ThrowsAsync<PublicationException>(() => core.UpdatePostCore(id, new() { Title = "Changed", Content = "Changed", Images = [] }));
        var cancelled = await s.Service.CancelSchedule(id, "facebook", new() { PublicationId = job.PublicationId, ExpectedRevision = job.Revision });
        Assert.Equal("Cancelled", cancelled.Status); Assert.Equal(job.ScheduledAt, cancelled.ScheduledAt); Assert.Null((await db.PlatformPosts.SingleAsync()).ScheduledAt);
        s.Clock.Now = job.ScheduledAt!.Value; await s.Worker.Process(job.PublicationId, job.Revision, default);
        Assert.Equal(0, s.Adapter.Calls); Assert.Equal(0, await new Recovery(db, s.Bus.Object, s.Clock).Dispatch(default));
        await core.UpdatePostCore(id, new() { Title = "Changed", Content = "Changed", Images = [] });
    }
    [Fact]
    public async Task RoutesCannotCancelADifferentPostOrPlatform()
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var job = await s.Service.Schedule(id, "facebook", await Input(s, id, account)); var request = new Request.CancelSchedule { PublicationId = job.PublicationId, ExpectedRevision = job.Revision };
        Assert.Equal(404, (await Assert.ThrowsAsync<PublicationException>(() => s.Service.CancelSchedule(Guid.NewGuid(), "facebook", request))).StatusCode);
        Assert.Equal(404, (await Assert.ThrowsAsync<PublicationException>(() => s.Service.CancelSchedule(id, "instagram", request))).StatusCode);
        Assert.Equal("Scheduled", (await s.Service.Get(job.PublicationId)).Job.Status);
    }
    [Theory]
    [InlineData(0, true)] [InlineData(900, true)] [InlineData(901, false)]
    public async Task RestartClaimsDueScheduleOnceOrStopsAnOverdueSchedule(int secondsLate, bool starts)
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var job = await s.Service.Schedule(id, "facebook", await Input(s, id, account)); s.Clock.Now = job.ScheduledAt!.Value.AddSeconds(secondsLate);
        var restart = Lifecycle.Create(db, s.Adapter, s.Clock); Assert.Equal(1, await new Recovery(db, restart.Bus.Object, s.Clock).Dispatch(default));
        await restart.Worker.Process(job.PublicationId, job.Revision, default); await restart.Worker.Process(job.PublicationId, job.Revision, default);
        var result = (await restart.Service.Get(job.PublicationId)).Job;
        Assert.Equal(starts ? "Publishing" : "Failed", result.Status); Assert.Equal(starts ? 1 : 0, s.Adapter.Calls);
        if (!starts) { Assert.Equal("PUBLICATION_SCHEDULE_MISSED", result.ErrorCode); Assert.False(result.CanRetry); Assert.Empty(db.PostPublicationAttempts); }
        else
        {
            await Assert.ThrowsAsync<PublicationException>(() => restart.Service.CancelSchedule(id, "facebook", new() { PublicationId = job.PublicationId, ExpectedRevision = result.Revision }));
            await Assert.ThrowsAsync<PublicationException>(() => restart.Service.Reschedule(id, "facebook", new() { PublicationId = job.PublicationId, ExpectedRevision = result.Revision, ScheduledAt = s.Clock.Now.AddHours(1).ToString("O") }));
            s.Clock.Now = s.Clock.Now.AddMinutes(2); await restart.Worker.Process(job.PublicationId, result.Revision, default);
            Assert.Equal("Succeeded", (await restart.Service.Get(job.PublicationId)).Job.Status); Assert.Equal(1, s.Adapter.Publishes);
        }
    }
    [Fact]
    public async Task AccountExpiryIsRecheckedAtDueTimeWithoutCreatingMedia()
    {
        await using var db = Lifecycle.Db(); var (id, account) = await Lifecycle.Seed(db); var s = Lifecycle.Create(db);
        var job = await s.Service.Schedule(id, "facebook", await Input(s, id, account));
        (await db.PlatformAccounts.SingleAsync()).AccessTokenExpiresAt = s.Clock.Now.AddMinutes(5); await db.SaveChangesAsync();
        s.Clock.Now = job.ScheduledAt!.Value; await s.Worker.Process(job.PublicationId, job.Revision, default);
        var result = (await s.Service.Get(job.PublicationId)).Job;
        Assert.Equal("Failed", result.Status); Assert.Equal("PLATFORM_REAUTH_REQUIRED", result.ErrorCode); Assert.Equal(0, s.Adapter.Calls);
    }
}
