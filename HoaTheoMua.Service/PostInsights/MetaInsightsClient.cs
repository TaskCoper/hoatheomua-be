using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;

namespace HoaTheoMua.Service.PostInsights;

public sealed class MetaInsightsClient(MetaPublishingClient graph)
{
    private static readonly string[] InstagramMetrics = ["views", "reach", "likes", "comments", "saved", "shares", "total_interactions"];

    public async Task<IReadOnlyDictionary<string, Response.Metric>> Get(PublicationSnapshot snapshot, string postId,
        Credentials credentials, CancellationToken ct)
    {
        if (snapshot.Platform == PlatformType.Instagram)
        {
            if (!MetaPublishingClient.Id(postId)) throw Invalid();
            // A missing/deleted media object must not be mistaken for seven unsupported metrics.
            var media = await graph.Get(snapshot, credentials, postId, "id", ct);
            if (MetaPublishingClient.Text(media, "id") != postId) throw Invalid();
            var results = new Dictionary<string, Response.Metric>();
            foreach (var metric in InstagramMetrics)
                results[metric] = await Insight(snapshot, credentials, postId, metric, ct);
            return results;
        }
        if (snapshot.Platform != PlatformType.Facebook || !MetaPublishingClient.PostId(postId)
            || postId.Split('_')[0] != snapshot.ExternalAccountId) throw Invalid();

        var post = await graph.Get(snapshot, credentials, postId,
            "id,likes:reactions.type(LIKE).limit(0).summary(total_count),reactions.limit(0).summary(total_count),comments.limit(0).summary(total_count),shares", ct);
        if (MetaPublishingClient.Text(post, "id") != postId) throw Invalid();
        var likes = Count(post, "reactions.type(LIKE).summary.total_count", "likes", "summary", "total_count");
        var reactions = Count(post, "reactions.summary.total_count", "reactions", "summary", "total_count");
        var comments = Count(post, "comments.summary.total_count", "comments", "summary", "total_count");
        var shares = Count(post, "shares.count", "shares", "count");
        // Facebook has no IG total_interactions equivalent here. Explicitly report our definition and source.
        var total = new Response.Metric(null, "Unavailable", "reactions + comments + shares", "DEPENDENT_METRIC_UNAVAILABLE");
        if (reactions.Value is long r && comments.Value is long c && shares.Value is long s)
        {
            try { total = new(checked(r + c + s), "Derived", "reactions + comments + shares"); }
            catch (OverflowException) { throw Invalid(); }
        }
        return new Dictionary<string, Response.Metric>
        {
            ["views"] = await Insight(snapshot, credentials, postId, "post_media_view", ct),
            ["reach"] = await Insight(snapshot, credentials, postId, "post_total_media_view_unique", ct),
            ["likes"] = likes, ["comments"] = comments, ["shares"] = shares,
            ["saved"] = new(null, "Unsupported", "", "FACEBOOK_POST_SAVED_NOT_SUPPORTED"),
            ["total_interactions"] = total
        };
    }

    private async Task<Response.Metric> Insight(PublicationSnapshot snapshot, Credentials credentials, string id,
        string metric, CancellationToken ct)
    {
        JsonElement json;
        try
        {
            // Separate requests isolate metrics unsupported for this media type/API version.
            json = await graph.Send(snapshot, credentials, id + "/insights",
                new() { ["metric"] = metric, ["period"] = "lifetime" }, false, ct);
        }
        catch (MetaPublishingException ex) when (ex.Result(false).ErrorCode == "META_INVALID_MEDIA_OR_PARAMETER")
        { return new(null, "Unavailable", metric, "META_METRIC_UNAVAILABLE"); }
        if (!json.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) throw Invalid();
        var rows = data.EnumerateArray().Where(x => MetaPublishingClient.Text(x, "name") == metric).ToArray();
        if (rows.Length == 0)
        {
            if (data.GetArrayLength() != 0) throw Invalid();
            return new(null, "Unavailable", metric, "META_NO_DATA");
        }
        if (rows.Length != 1) throw Invalid();
        var row = rows[0];
        if (row.TryGetProperty("period", out var period) && (period.ValueKind != JsonValueKind.String || period.GetString() != "lifetime"))
            throw Invalid();
        if (row.TryGetProperty("total_value", out var total)) return Count(total, metric, "value");
        if (!row.TryGetProperty("values", out var values) || values.ValueKind != JsonValueKind.Array) throw Invalid();
        if (values.GetArrayLength() == 0) return new(null, "Unavailable", metric, "META_NO_DATA");
        // A lifetime count must be a single aggregate, never a sum of daily reach or cumulative snapshots.
        if (values.GetArrayLength() != 1) throw Invalid();
        return Count(values[0], metric, "value");
    }

    private static Response.Metric Count(JsonElement json, string source, params string[] path)
    {
        foreach (var key in path)
        {
            if (json.ValueKind != JsonValueKind.Object) throw Invalid();
            if (!json.TryGetProperty(key, out json)) return new(null, "Unavailable", source, "META_NO_DATA");
        }
        if (json.ValueKind == JsonValueKind.Null) return new(null, "Unavailable", source, "META_NO_DATA");
        if (json.ValueKind != JsonValueKind.Number || !json.TryGetInt64(out var count) || count < 0) throw Invalid();
        return new(count, "Available", source);
    }
    private static MetaPublishingException Invalid() => new("META_INVALID_RESPONSE");
}
