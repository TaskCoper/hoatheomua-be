using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.Extensions.Options;
using static HoaTheoMua.Service.PostPublishing.MetaPublishingClient;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class InstagramPublisher(MetaPublishingClient graph, PublishingMediaClient media, IOAuthProvider accounts,
    IOptions<PlatformAccountOptions> options, TimeProvider clock) : MetaPublisher(graph, media, accounts, options, clock)
{
    public override PlatformType Platform => PlatformType.Instagram;
    public override async Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot s,
        PublicationCheckpoint cp, Credentials c, string? candidateExternalId, CancellationToken ct)
    {
        if (!ValidCheckpoint(s, cp)) return Unknown("PUBLICATION_CHECKPOINT_INVALID");
        if (operation == PublicationOperation.Reconcile)
        {
            if (cp.ContainerId == null) return Unknown("INSTAGRAM_CONTAINER_ID_MISSING");
            var state = await Status(s, c, cp.ContainerId, ct);
            // Container and published-media IDs have no documented one-to-one lookup. Never infer failure
            // from FINISHED/ERROR/EXPIRED after a publish intent, or success from a matching caption.
            return Unknown(state == "PUBLISHED" ? "INSTAGRAM_PUBLISHED_ID_UNRESOLVED" : "INSTAGRAM_PUBLICATION_UNRESOLVED");
        }
        if (Expired(cp)) return ResetExpired();
        if (operation == PublicationOperation.Publish)
        {
            if (!cp.ReadyToPublish || cp.ContainerId == null) return new(PublicationOutcome.Rejected)
                { ErrorCode = "PUBLICATION_MEDIA_NOT_READY", ConfirmedNotPublished = true };
            var published = await Graph.Send(s, c, s.ExternalAccountId + "/media_publish", new() { ["creation_id"] = cp.ContainerId }, true, ct);
            var id = RequiredId(published);
            // Once an ID was returned, failure to read the optional permalink cannot erase success.
            string? url = null;
            try
            {
                var detail = await Graph.Get(s, c, id, "id,permalink", ct);
                var link = Text(detail, "permalink");
                if (Text(detail, "id") == id && Uri.TryCreate(link, UriKind.Absolute, out var u) && u.Scheme == "https"
                    && u.UserInfo.Length == 0 && u.Host is "www.instagram.com" or "instagram.com" && link!.Length <= 2048) url = link;
            }
            catch (Exception) { /* Known publish ID remains authoritative, including after timeout. */ }
            return new(PublicationOutcome.Succeeded) { ExternalPostId = id, ExternalPostUrl = url };
        }
        if (cp.ContainerId != null)
        {
            var state = await Status(s, c, cp.ContainerId, ct);
            return Poll(state, cp, parent: true);
        }
        var images = s.Images.OrderBy(x => x.Order).ToArray();
        var childToPoll = cp.Media.OrderBy(x => x.Order).FirstOrDefault(x => !x.Ready);
        if (childToPoll != null)
        {
            var state = await Status(s, c, childToPoll.ProviderId, ct);
            return state == "FINISHED" ? Continue(cp with { Media = cp.Media.Select(x => x.Order == childToPoll.Order ? x with { Ready = true } : x).ToArray() })
                : Poll(state, cp, parent: false);
        }
        var next = images.FirstOrDefault(x => cp.Media.All(m => m.Order != x.Order));
        if (next != null)
        {
            var info = await Media.Inspect(next, Platform, ct);
            if (images.Length > 1 && info.Width != info.Height) throw new MetaPublishingException("INSTAGRAM_CAROUSEL_SQUARE_REQUIRED_" + next.Order, definitive: true);
            Dictionary<string, string> values = new() { ["image_url"] = next.Url };
            if (images.Length == 1) values["caption"] = s.Text; else values["is_carousel_item"] = "true";
            var child = await Graph.Send(s, c, s.ExternalAccountId + "/media", values, true, ct);
            var id = RequiredId(child);
            return Continue(cp with
            {
                Media = cp.Media.Append(new MediaCheckpoint(next.Order, id, info.Width, info.Height)).OrderBy(x => x.Order).ToArray(),
                ContainerId = images.Length == 1 ? id : null, PreparedAt = cp.PreparedAt ?? Clock.GetUtcNow()
            });
        }
        var parent = await Graph.Send(s, c, s.ExternalAccountId + "/media", new()
        {
            ["media_type"] = "CAROUSEL", ["children"] = string.Join(',', cp.Media.OrderBy(x => x.Order).Select(x => x.ProviderId)), ["caption"] = s.Text
        }, true, ct);
        return Continue(cp with { ContainerId = RequiredId(parent) });
    }
    private async Task<string?> Status(PublicationSnapshot s, Credentials c, string id, CancellationToken ct)
    {
        var result = await Graph.Get(s, c, id, "status_code", ct);
        if (Text(result, "id") != id) throw new MetaPublishingException("META_INVALID_RESPONSE");
        return Text(result, "status_code");
    }
    private static PublicationResult Poll(string? state, PublicationCheckpoint cp, bool parent) => state switch
    {
        "FINISHED" when parent => Continue(cp with { ReadyToPublish = true }),
        "IN_PROGRESS" => new(PublicationOutcome.Pending) { Checkpoint = cp, RetryAfterSeconds = 60 },
        "EXPIRED" => ResetExpired(),
        "ERROR" => new(PublicationOutcome.Rejected) { ErrorCode = "INSTAGRAM_MEDIA_PROCESSING_FAILED", ConfirmedNotPublished = true },
        _ => Unknown(state == "PUBLISHED" ? "INSTAGRAM_UNEXPECTED_PUBLISHED_CONTAINER" : "META_INVALID_RESPONSE")
            with { PublishMayHaveOccurred = state == "PUBLISHED" }
    };
}
