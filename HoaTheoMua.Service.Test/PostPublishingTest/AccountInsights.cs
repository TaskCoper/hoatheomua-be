using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.PostInsights;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace HoaTheoMua.Service.Test.PostPublishingTest;

public partial class Insights
{
    private static AccountInsightsRequest Range() => new()
    {
        FromDate = new(2026, 1, 1), ToDate = new(2026, 1, 2), Metrics = "reach"
    };

    [Theory]
    [InlineData(PlatformType.Facebook)]
    [InlineData(PlatformType.Instagram)]
    public async Task AccountReportsUseConnectedAccountAndExactUtcRangeWithoutPublication(PlatformType platform)
    {
        await using var f = new Fixture(); await f.Seed(platform);
        f.Db.PostPublications.Remove(f.Job); await f.Db.SaveChangesAsync();
        f.Handler.Reply = r =>
        {
            Assert.StartsWith("/v26.0/123", r.RequestUri!.AbsolutePath);
            var q = QueryHelpers.ParseQuery(r.RequestUri.Query);
            if (!q.ContainsKey("metric")) return Json(new { id = "123" });
            Assert.Equal("1767225600", q["since"].ToString());
            Assert.Equal("1767398400", q["until"].ToString());
            Assert.Equal("day", q["period"].ToString());
            Assert.Equal(platform == PlatformType.Instagram, q.ContainsKey("metric_type"));
            return Json(new { data = new[] { new { name = "reach", period = "day", total_value = new { value = 0 } } } });
        };
        var report = await f.Service.GetAccount(f.Account.Id, Range());
        Assert.False(report.IsPartial); Assert.Equal(0L, report.Metrics["reach"].TotalValue);
        Assert.Equal("UTC", report.Timezone); Assert.Equal(f.Account.Id, report.PlatformAccountId);
        Assert.DoesNotContain("test-token", JsonSerializer.Serialize(report));
        Assert.Empty(f.Db.PostPublications);
    }

    [Fact]
    public async Task DailyReachIsNotSummedAndProviderPagingIsNotFollowed()
    {
        await using var f = new Fixture(); await f.Seed();
        f.Handler.Reply = r => r.RequestUri!.AbsolutePath.EndsWith("insights")
            ? Json(new { data = new[] { new { name = "reach", period = "day", values = new[] {
                new { value = 12, end_time = "2026-01-03T00:00:00+00:00" },
                new { value = 10, end_time = "2026-01-02T00:00:00+00:00" } } } },
                paging = new { next = "https://untrusted.invalid/?access_token=secret" } })
            : Json(new { id = "123" });
        var input = Range(); input.MetricType = "time_series";
        var report = await f.Service.GetAccount(f.Account.Id, input);
        Assert.Null(report.Metrics["reach"].TotalValue);
        Assert.Equal(new long[] { 10, 12 }, report.Metrics["reach"].Values.Select(x => x.Value));
        Assert.Equal(2, f.Handler.Requests.Count);
    }

    [Fact]
    public async Task UnsupportedMetricAndEmptyDataRemainUnavailableWhileOtherMetricsSucceed()
    {
        await using var f = new Fixture(); await f.Seed();
        f.Handler.Reply = r => QueryHelpers.ParseQuery(r.RequestUri!.Query).GetValueOrDefault("metric").ToString() switch
        {
            "" => Json(new { id = "123" }), "likes" => Error(100),
            "comments" => Json(new { data = Array.Empty<object>() }),
            _ => Json(new { data = new[] { new { name = "reach", period = "day", total_value = new { value = 25 } } } })
        };
        var input = Range(); input.Metrics = "reach,likes,comments";
        var report = await f.Service.GetAccount(f.Account.Id, input);
        Assert.True(report.IsPartial); Assert.Equal(25L, report.Metrics["reach"].TotalValue);
        Assert.Null(report.Metrics["likes"].TotalValue);
        Assert.Equal("META_METRIC_OR_RANGE_UNAVAILABLE", report.Metrics["likes"].Reason);
        Assert.Equal("META_NO_DATA", report.Metrics["comments"].Reason);
    }

    [Theory]
    [InlineData("missing-date")] [InlineData("reversed")] [InlineData("too-long")]
    [InlineData("future")] [InlineData("no-metrics")] [InlineData("invalid-metric")]
    [InlineData("lifetime")] [InlineData("metric-type")]
    public async Task InvalidAccountRangesDoNotCallMeta(string scenario)
    {
        await using var f = new Fixture(); await f.Seed(); var input = Range();
        switch (scenario)
        {
            case "missing-date": input.FromDate = null; break;
            case "reversed": input.FromDate = input.ToDate!.Value.AddDays(1); break;
            case "too-long": input.ToDate = input.FromDate!.Value.AddDays(30); break;
            case "future": input.ToDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1); break;
            case "no-metrics": input.Metrics = null; break;
            case "invalid-metric": input.Metrics = "reach&access_token=x"; break;
            case "lifetime": input.Period = "lifetime"; break;
            case "metric-type": input.MetricType = "bad"; break;
        }
        await Assert.ThrowsAsync<BadRequestException>(() => f.Service.GetAccount(f.Account.Id, input));
        Assert.Empty(f.Handler.Requests);
    }

    [Theory]
    [InlineData("scopes", "INSIGHTS_PERMISSION_REQUIRED")]
    [InlineData("expired", "PLATFORM_REAUTH_REQUIRED")]
    [InlineData("disconnected", "PLATFORM_REAUTH_REQUIRED")]
    [InlineData("credentials", "PLATFORM_TOKEN_UNAVAILABLE")]
    [InlineData("refresh", "PLATFORM_REFRESH_IN_PROGRESS")]
    public async Task AccountReportChecksCredentialsBeforeCallingMeta(string scenario, string code)
    {
        await using var f = new Fixture(); await f.Seed();
        switch (scenario)
        {
            case "scopes": f.Account.GrantedScopes = []; break;
            case "expired": f.Account.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1); break;
            case "disconnected": f.Account.Status = "Disconnected"; break;
            case "credentials": f.Account.ProtectedCredentials = "corrupt"; break;
            case "refresh": f.Account.RefreshLeaseId = Guid.NewGuid(); break;
        }
        await f.Db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<PublicationException>(() => f.Service.GetAccount(f.Account.Id, Range()));
        Assert.Equal(code, error.MessageCode); Assert.Empty(f.Handler.Requests);
    }

    [Theory]
    [InlineData(190, 409)] [InlineData(10, 409)] [InlineData(4, 429)] [InlineData(2, 502)]
    public async Task AccountReportNormalizesProviderFailures(int code, int status)
    {
        await using var f = new Fixture(); await f.Seed(); f.Handler.Reply = _ => Error(code);
        var error = await Assert.ThrowsAsync<PublicationException>(() => f.Service.GetAccount(f.Account.Id, Range()));
        Assert.Equal(status, error.StatusCode); Assert.DoesNotContain("secret", error.Message);
    }
}
