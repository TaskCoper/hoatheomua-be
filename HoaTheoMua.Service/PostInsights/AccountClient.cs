using System.Globalization;
using System.Text.Json;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;

namespace HoaTheoMua.Service.PostInsights;

public sealed partial class MetaInsightsClient
{
    public async Task<IReadOnlyDictionary<string, Response.AccountMetric>> GetAccount(PublicationSnapshot snapshot,
        Credentials credentials, string[] metrics, string period, string? metricType,
        DateTimeOffset since, DateTimeOffset until, CancellationToken ct)
    {
        if (!MetaPublishingClient.Id(snapshot.ExternalAccountId)) throw Invalid();
        var target = await graph.Get(snapshot, credentials, snapshot.ExternalAccountId, "id", ct);
        if (MetaPublishingClient.Text(target, "id") != snapshot.ExternalAccountId) throw Invalid();
        var result = new Dictionary<string, Response.AccountMetric>();
        foreach (var metric in metrics)
        {
            var parameters = new Dictionary<string, string>
            {
                ["metric"] = metric, ["period"] = period, ["limit"] = "1000",
                ["since"] = since.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ["until"] = until.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)
            };
            if (metricType != null) parameters["metric_type"] = metricType;
            JsonElement json;
            try { json = await graph.Send(snapshot, credentials, snapshot.ExternalAccountId + "/insights", parameters, false, ct); }
            catch (MetaPublishingException ex) when (ex.Result(false).ErrorCode == "META_INVALID_MEDIA_OR_PARAMETER")
            {
                result[metric] = new("Unavailable", period, null, [], "META_METRIC_OR_RANGE_UNAVAILABLE");
                continue;
            }
            if (!json.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) throw Invalid();
            if (data.GetArrayLength() == 0)
            {
                result[metric] = new("Unavailable", period, null, [], "META_NO_DATA");
                continue;
            }
            if (data.GetArrayLength() != 1) throw Invalid();
            var row = data[0];
            if (MetaPublishingClient.Text(row, "name") != metric || MetaPublishingClient.Text(row, "period") != period)
                throw Invalid();
            // Insights paging links navigate adjacent time windows; do not follow them outside the requested range.
            if (row.TryGetProperty("total_value", out var total))
            {
                var count = Count(total, metric, "value");
                result[metric] = new(count.Status, period, count.Value, [], count.Reason);
                continue;
            }
            if (!row.TryGetProperty("values", out var values) || values.ValueKind != JsonValueKind.Array) throw Invalid();
            var points = new List<Response.InsightPoint>();
            foreach (var value in values.EnumerateArray())
            {
                var count = Count(value, metric, "value");
                if (count.Value == null || !DateTimeOffset.TryParse(MetaPublishingClient.Text(value, "end_time"),
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime)) throw Invalid();
                points.Add(new(endTime, count.Value.Value));
            }
            if (points.Select(x => x.EndTime).Distinct().Count() != points.Count) throw Invalid();
            result[metric] = new(points.Count == 0 ? "Unavailable" : "Available", period, null,
                points.OrderBy(x => x.EndTime).ToArray(), points.Count == 0 ? "META_NO_DATA" : null);
        }
        return result;
    }
}
