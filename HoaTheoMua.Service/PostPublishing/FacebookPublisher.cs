using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.Extensions.Options;
using static HoaTheoMua.Service.PostPublishing.MetaPublishingClient;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class FacebookPublisher(MetaPublishingClient graph, PublishingMediaClient media, IOAuthProvider accounts,
    IOptions<PlatformAccountOptions> options, TimeProvider clock) : MetaPublisher(graph, media, accounts, options, clock)
{
    public override PlatformType Platform => PlatformType.Facebook;
    public override async Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot s,
        PublicationCheckpoint cp, Credentials c, string? candidateExternalId, CancellationToken ct)
    {
        if (!ValidCheckpoint(s, cp)) return Unknown("PUBLICATION_CHECKPOINT_INVALID");
        if (operation == PublicationOperation.Reconcile) return await Reconcile(s, cp, c, candidateExternalId, ct);
        if (Expired(cp)) return ResetExpired();
        var images = s.Images.OrderBy(x => x.Order).ToArray();
        if (operation == PublicationOperation.Prepare)
        {
            if (images.Length == 1)
            {
                await Media.Inspect(images[0], Platform, ct);
                return Continue(cp with { ReadyToPublish = true, PreparedAt = Clock.GetUtcNow() });
            }
            var next = images.FirstOrDefault(x => cp.Media.All(m => m.Order != x.Order));
            if (next == null) return Continue(cp with { ReadyToPublish = true });
            await Media.Inspect(next, Platform, ct);
            var photo = await Graph.Send(s, c, s.ExternalAccountId + "/photos", new() { ["url"] = next.Url, ["published"] = "false", ["temporary"] = "true" }, true, ct);
            var items = cp.Media.Append(new MediaCheckpoint(next.Order, RequiredId(photo))).OrderBy(x => x.Order).ToArray();
            return Continue(cp with { Media = items, PreparedAt = cp.PreparedAt ?? Clock.GetUtcNow(), ReadyToPublish = items.Length == images.Length });
        }
        if (!cp.ReadyToPublish || (images.Length > 1 && cp.Media.Length != images.Length))
            return new(PublicationOutcome.Rejected) { ErrorCode = "PUBLICATION_MEDIA_NOT_READY", ConfirmedNotPublished = true };
        JsonElement result;
        if (images.Length == 1)
        {
            // Recheck immediately before the only request that can publish this URL.
            await Media.Inspect(images[0], Platform, ct);
            result = await Graph.Send(s, c, s.ExternalAccountId + "/photos", new() { ["url"] = images[0].Url, ["caption"] = s.Text, ["published"] = "true" }, true, ct);
            if (Id(Text(result, "id"))) cp = cp with { Media = [new(images[0].Order, Text(result, "id")!)] };
        }
        else result = await Graph.Send(s, c, s.ExternalAccountId + "/feed", new()
        {
            ["message"] = s.Text, ["published"] = "true",
            ["attached_media"] = JsonSerializer.Serialize(cp.Media.OrderBy(x => x.Order).Select(x => new { media_fbid = x.ProviderId }))
        }, true, ct);
        var id = Text(result, images.Length == 1 ? "post_id" : "id");
        return PostId(id) && id!.StartsWith(s.ExternalAccountId + "_", StringComparison.Ordinal)
            ? Success(id) with { Checkpoint = cp }
            : Unknown("FACEBOOK_POST_ID_MISSING") with { Checkpoint = cp };
    }
    private static PublicationResult Success(string id) => new(PublicationOutcome.Succeeded)
        { ExternalPostId = id, ExternalPostUrl = "https://www.facebook.com/" + id.Split('_')[0] + "/posts/" + id.Split('_')[1] };
    private async Task<PublicationResult> Reconcile(PublicationSnapshot s, PublicationCheckpoint cp, Credentials c, string? candidate, CancellationToken ct)
    {
        if (!PostId(candidate) || !candidate!.StartsWith(s.ExternalAccountId + "_", StringComparison.Ordinal)) return Unknown("FACEBOOK_POST_ID_REQUIRED");
        var post = await Graph.Get(s, c, candidate, "id,from,message,is_published,attachments{target,subattachments{target}}", ct);
        if (Text(post, "id") != candidate || !post.TryGetProperty("from", out var author) || Text(author, "id") != s.ExternalAccountId
            || Text(post, "message") != s.Text || !post.TryGetProperty("is_published", out var published) || published.ValueKind != JsonValueKind.True)
            return Unknown("PUBLICATION_CANDIDATE_MISMATCH");
        // A matching caption is not sufficient: require the exact photo IDs persisted by this job.
        var ids = new List<string>();
        if (post.TryGetProperty("attachments", out var attachments) && attachments.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            foreach (var item in data.EnumerateArray())
                if (item.TryGetProperty("subattachments", out var sub) && sub.TryGetProperty("data", out var children) && children.ValueKind == JsonValueKind.Array)
                    foreach (var child in children.EnumerateArray()) { if (child.TryGetProperty("target", out var target) && Text(target, "id") is { } id) ids.Add(id); }
                else if (item.TryGetProperty("target", out var target) && Text(target, "id") is { } id) ids.Add(id);
        return cp.Media.Length == s.Images.Length && cp.Media.OrderBy(x => x.Order).Select(x => x.ProviderId).SequenceEqual(ids)
            ? Success(candidate) : Unknown("PUBLICATION_CANDIDATE_MEDIA_UNVERIFIED");
    }
}
