using System.Net.Http.Headers;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.EntityFrameworkCore;

static class ScheduleProbe
{
    internal sealed class Clock : TimeProvider
    {
        private long ticks = DateTimeOffset.UtcNow.Ticks / 10 * 10;
        public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref ticks), TimeSpan.Zero);
        public void Advance(TimeSpan amount) => Interlocked.Add(ref ticks, amount.Ticks);
    }
    internal static async Task Exercise(WebApplication app, HttpClient client,
        Func<string, string, int, object?, Task<JsonElement>> send, Action<bool, string, object?> check,
        Func<string, string> jwt, AdapterProbe.Provider provider, Clock clock)
    {
        provider.Scenario = "success"; provider.Caption = "Snapshot\n\nManual snapshot";
        void Check(bool value, string name) => check(value, name, null);
        async Task<JsonElement> Send(string method, string path, int expected, object? body = null) => await send(method, path, expected, body);
        async Task<(Guid Post, Guid Account)> Seed(PlatformType platform = PlatformType.Facebook)
        {
            await using var scope = app.Services.CreateAsyncScope();
            return await AdapterProbe.Seed(scope.ServiceProvider.GetRequiredService<AppDbContext>(), scope.ServiceProvider.GetRequiredService<TokenProtector>(), platform, 2);
        }
        async Task<PostPublication> Read(Guid id)
        {
            await using var scope = app.Services.CreateAsyncScope(); return await scope.ServiceProvider.GetRequiredService<AppDbContext>().PostPublications.AsNoTracking().SingleAsync(x => x.Id == id);
        }
        async Task<PostPublication> Wait(Guid id, string status, bool advance = false)
        {
            var deadline = DateTimeOffset.UtcNow.AddSeconds(25); PostPublication row;
            do
            {
                row = await Read(id); if (row.Status == status) return row;
                if (row.Status is "Failed" or "Succeeded" or "Cancelled") throw new Exception($"Expected {status}, got {row.Status}/{row.ErrorCode}");
                if (advance) clock.Advance(TimeSpan.FromSeconds(2));
                await Task.Delay(150);
            } while (DateTimeOffset.UtcNow < deadline);
            throw new Exception($"Schedule did not reach {status}; current {row.Status}/{row.ErrorCode}");
        }
        string Root(Guid post, PlatformType platform = PlatformType.Facebook) => $"/api/v1/posts/{post}/platforms/{platform.ToString().ToLowerInvariant()}";
        async Task<JsonElement> Schedule(Guid post, Guid account, string key = "one", PlatformType platform = PlatformType.Facebook)
        {
            var preview = (await Send("GET", Root(post, platform) + $"/publication-preview?accountId={account}", 200)).GetProperty("value");
            return (await Send("POST", Root(post, platform) + "/schedule", 202, new
            {
                accountId = account, idempotencyKey = key, expectedRevision = preview.GetProperty("revision").GetString(),
                scheduledAt = clock.GetUtcNow().AddMinutes(10).ToOffset(TimeSpan.FromHours(7)).ToString("O")
            })).GetProperty("value");
        }
        var (post, account) = await Seed(); var path = Root(post) + "/schedule";
        foreach (var role in new[] { "anonymous", "Staff" })
        {
            client.DefaultRequestHeaders.Authorization = role == "anonymous" ? null : new AuthenticationHeaderValue("Bearer", jwt(role));
            foreach (var method in new[] { "POST", "PUT", "DELETE" }) await Send(method, path, role == "anonymous" ? 401 : 403, method == "DELETE" ? null : new { });
        }
        client.DefaultRequestHeaders.Authorization = new("Bearer", jwt("Admin"));
        var preview = (await Send("GET", Root(post) + $"/publication-preview?accountId={account}", 200)).GetProperty("value");
        var request = new { accountId = account, idempotencyKey = "schedule-http", expectedRevision = preview.GetProperty("revision").GetString(), scheduledAt = clock.GetUtcNow().AddMinutes(10).ToOffset(TimeSpan.FromHours(7)).ToString("O") };
        foreach (var date in new[] { "2026-09-15T10:00:00", clock.GetUtcNow().AddSeconds(30).ToString("O"), clock.GetUtcNow().AddDays(31).ToString("O") })
            await Send("POST", path, 400, new { request.accountId, request.idempotencyKey, request.expectedRevision, scheduledAt = date });
        var job = (await Send("POST", path, 202, request)).GetProperty("value"); var id = job.GetProperty("publicationId").GetGuid(); var revision = job.GetProperty("revision").GetInt64();
        Check(job.GetProperty("status").GetString() == "Scheduled", "Create returns Scheduled");
        Check(job.GetProperty("scheduledAt").GetDateTimeOffset().Offset == TimeSpan.Zero, "Schedule response normalized to UTC");
        var duplicate = (await Send("POST", path, 202, request)).GetProperty("value"); Check(duplicate.GetProperty("publicationId").GetGuid() == id, "Schedule is idempotent");
        await Send("POST", Root(post) + "/publish", 409, new { request.accountId, request.idempotencyKey, request.expectedRevision });
        await Task.Delay(1100); Check(provider.Publishes == 0 && provider.Creates == 0, "No remote media before due time");
        var originalSnapshot = (await Read(id)).Snapshot;
        var moved = (await Send("PUT", path, 200, new { publicationId = id, expectedRevision = revision, scheduledAt = clock.GetUtcNow().AddMinutes(20).ToString("O") })).GetProperty("value");
        var changedRevision = moved.GetProperty("revision").GetInt64();
        await Send("PUT", path, 409, new { publicationId = id, expectedRevision = revision, scheduledAt = clock.GetUtcNow().AddMinutes(30).ToString("O") });
        await Send("DELETE", Root(Guid.NewGuid()) + $"/schedule?publicationId={id}&expectedRevision={changedRevision}", 404);
        clock.Advance(TimeSpan.FromMinutes(10));
        await using (var scope = app.Services.CreateAsyncScope()) await scope.ServiceProvider.GetRequiredService<Worker>().Process(id, revision, default);
        Check(provider.Publishes == 0 && provider.Creates == 0, "Old revision cannot execute after reschedule");
        Check((await Read(id)).Snapshot == originalSnapshot, "Reschedule preserves snapshot");
        var cancelled = (await Send("DELETE", path + $"?publicationId={id}&expectedRevision={changedRevision}", 200)).GetProperty("value");
        Check(cancelled.GetProperty("status").GetString() == "Cancelled", "Cancel schedule succeeds");
        await Send("DELETE", path + $"?publicationId={id}&expectedRevision={changedRevision}", 409);

        foreach (var platform in new[] { PlatformType.Facebook, PlatformType.Instagram })
        {
            var next = await Seed(platform); var scheduled = await Schedule(next.Post, next.Account, platform: platform);
            var nextId = scheduled.GetProperty("publicationId").GetGuid(); var publishesBefore = provider.Publishes;
            clock.Advance(TimeSpan.FromMinutes(10));
            var claimed = await Wait(nextId, "Publishing");
            await Send("DELETE", Root(next.Post, platform) + $"/schedule?publicationId={nextId}&expectedRevision={claimed.Revision}", 409);
            await Send("PUT", Root(next.Post, platform) + "/schedule", 409, new { publicationId = nextId, expectedRevision = claimed.Revision, scheduledAt = clock.GetUtcNow().AddMinutes(20).ToString("O") });
            var done = await Wait(nextId, "Succeeded", advance: true);
            Check(provider.Publishes == publishesBefore + 1, $"{platform}: one publish through RabbitMQ");
            Check(done.ScheduledAt == scheduled.GetProperty("scheduledAt").GetDateTimeOffset(), "Scheduled time preserved after success");
            await Send("GET", $"/api/v1/post-gen/publications/{nextId}", 200);
        }
        var missedSource = await Seed(); var missed = await Schedule(missedSource.Post, missedSource.Account);
        var missedId = missed.GetProperty("publicationId").GetGuid(); var countBefore = provider.Publishes;
        clock.Advance(TimeSpan.FromMinutes(25).Add(TimeSpan.FromSeconds(1)));
        var failed = await Wait(missedId, "Failed");
        Check(failed.ErrorCode == "PUBLICATION_SCHEDULE_MISSED" && !failed.CanRetry, "Overdue schedule is failed without automatic catch-up");
        Check(provider.Publishes == countBefore, "Missed schedule never publishes");
        await Send("POST", $"/api/v1/post-gen/publications/{missedId}/retry", 409, new { expectedRevision = failed.Revision });

        var expiredSource = await Seed(); var expired = await Schedule(expiredSource.Post, expiredSource.Account); var expiredId = expired.GetProperty("publicationId").GetGuid();
        await using (var scope = app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); var connected = await db.PlatformAccounts.SingleAsync(x => x.Id == expiredSource.Account);
            connected.AccessTokenExpiresAt = clock.GetUtcNow().AddMinutes(5); await db.SaveChangesAsync();
        }
        clock.Advance(TimeSpan.FromMinutes(10)); var rejected = await Wait(expiredId, "Failed");
        Check(rejected.ErrorCode == "PLATFORM_REAUTH_REQUIRED" && provider.Publishes == countBefore, "Expired account is checked when schedule becomes due");
        await Send("GET", $"/api/v1/post-gen/publications/{expiredId}", 200);
        Check((await Read(id)).Status == "Cancelled", "Cancelled schedule stayed cancelled after its original due time");
        Console.WriteLine("PASS schedule/create/reschedule/cancel/due/late/account-expired with real adapters, PostgreSQL, RabbitMQ and controllable clock");
    }
}
