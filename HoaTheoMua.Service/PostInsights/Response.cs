using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.PostInsights;

public static class Response
{
    public record InsightPoint(DateTimeOffset EndTime, long Value);
    public record AccountMetric(string Status, string Period, long? TotalValue,
        IReadOnlyList<InsightPoint> Values, string? Reason = null);
    public record AccountInsights(Guid PlatformAccountId, string Platform, string ExternalAccountId,
        DateOnly FromDate, DateOnly ToDate, string Timezone, string Period, string? MetricType,
        string ApiVersion, DateTimeOffset FetchedAt, bool IsPartial,
        IReadOnlyDictionary<string, AccountMetric> Metrics);
    public record Metric(long? Value, string Status, string Source, string? Reason = null);
    public record Metrics(long? Views, long? Reach, long? Likes, long? Comments, long? Saved, long? Shares,
        [property: JsonPropertyName("total_interactions")] long? TotalInteractions);
    public record Insights(Guid PublicationId, Guid BasePostId, Guid PlatformAccountId, string Platform,
        string ExternalPostId, string? ExternalPostUrl, DateTimeOffset FetchedAt, string Period,
        string ApiVersion, bool IsPartial, Metrics Metrics, IReadOnlyDictionary<string, Metric> MetricDetails);
}
