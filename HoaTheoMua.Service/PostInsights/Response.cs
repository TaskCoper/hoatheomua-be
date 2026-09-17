using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.PostInsights;

public static class Response
{
    public record Metric(long? Value, string Status, string Source, string? Reason = null);
    public record Metrics(long? Views, long? Reach, long? Likes, long? Comments, long? Saved, long? Shares,
        [property: JsonPropertyName("total_interactions")] long? TotalInteractions);
    public record Insights(Guid PublicationId, Guid BasePostId, Guid PlatformAccountId, string Platform,
        string ExternalPostId, string? ExternalPostUrl, DateTimeOffset FetchedAt, string Period,
        string ApiVersion, bool IsPartial, Metrics Metrics, IReadOnlyDictionary<string, Metric> MetricDetails);
}
