using System.Text;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PostPublishing;

public abstract class MetaPublisher(MetaPublishingClient graph, PublishingMediaClient media, IOAuthProvider accounts,
    IOptions<PlatformAccountOptions> options, TimeProvider clock) : IPublishingAdapter
{
    protected readonly MetaPublishingClient Graph = graph;
    protected readonly PublishingMediaClient Media = media;
    protected readonly TimeProvider Clock = clock;
    public abstract PlatformType Platform { get; }
    public virtual IReadOnlyList<string> Validate(PublicationSnapshot s)
    {
        List<string> errors = [];
        if (s.Platform != Platform || !MetaPublishingClient.Id(s.ExternalAccountId) || !MetaPublishingClient.Id(s.PageId)
            || (Platform == PlatformType.Facebook && s.ExternalAccountId != s.PageId)) errors.Add("META_INVALID_TARGET");
        if (!options.Value.Meta.Enabled || s.ApiVersion != options.Value.Meta.GraphApiVersion) errors.Add("META_PUBLISHING_CONFIG_INVALID");
        if (s.FormatterVersion != "postgen-text-v1" || s.Text != string.Join("\n\n", new[] { s.Title, s.Content, s.Hashtag }.Where(x => !string.IsNullOrWhiteSpace(x))))
            errors.Add("PUBLICATION_FORMATTER_MISMATCH");
        if (string.IsNullOrWhiteSpace(s.Text)) errors.Add("MISSING_CONTENT");
        if (s.Images.Length is < 1 or > 10) errors.Add("PUBLICATION_IMAGE_COUNT_INVALID"); // Facebook: product cap, not a claimed Meta limit.
        if (s.Images.Select(x => x.Order).Distinct().Count() != s.Images.Length) errors.Add("PUBLICATION_IMAGE_ORDER_INVALID");
        foreach (var image in s.Images.Where(x => !PublishingMediaClient.Url(x.Url))) errors.Add("MEDIA_URL_INVALID_" + image.Order);
        if (Platform == PlatformType.Instagram)
        {
            if (s.Text.EnumerateRunes().Count() > 2200) errors.Add("INSTAGRAM_CAPTION_TOO_LONG");
            if (Regex.Matches(s.Text, @"(?<![\p{L}\p{N}_])#[\p{L}\p{N}_]+").Count > 30) errors.Add("INSTAGRAM_TOO_MANY_HASHTAGS");
            if (Regex.Matches(s.Text, @"(?<![\p{L}\p{N}_])@[\p{L}\p{N}_.]+").Count > 20) errors.Add("INSTAGRAM_TOO_MANY_MENTIONS");
        }
        return errors;
    }
    public async Task<IReadOnlyList<string>> Preflight(PublicationSnapshot s, Credentials c, CancellationToken ct)
    {
        // Reuse the read-only Meta validation from M11; no OAuth exchange or token persistence here.
        try
        {
            var account = await accounts.Refresh(Platform, s.ExternalAccountId, s.PageId, c, ct);
            if (account.ExternalId != s.ExternalAccountId || account.PageId != s.PageId || account.ExpiresAt <= Clock.GetUtcNow())
                throw new MetaPublishingException("META_TOKEN_INVALID", true, true);
        }
        catch (AccountException ex)
        {
            throw new MetaPublishingException(ex.MessageCode == "PLATFORM_REAUTH_REQUIRED" ? "META_TOKEN_INVALID"
                : ex.MessageCode == "PLATFORM_RATE_LIMITED" ? "META_RATE_LIMITED" : "META_ACCOUNT_CHECK_FAILED", true, true);
        }
        if (Platform == PlatformType.Instagram)
        {
            var result = await Graph.Get(s, c, s.ExternalAccountId + "/content_publishing_limit", "quota_usage,config", ct);
            if (!result.TryGetProperty("data", out var data) || data.ValueKind != System.Text.Json.JsonValueKind.Array || data.GetArrayLength() != 1
                || !data[0].TryGetProperty("quota_usage", out var usage) || !usage.TryGetInt32(out var used)
                || !data[0].TryGetProperty("config", out var config) || !config.TryGetProperty("quota_total", out var total) || !total.TryGetInt32(out var limit)
                || used < 0 || limit <= 0) throw new MetaPublishingException("INSTAGRAM_QUOTA_UNVERIFIED", true, true);
            if (used >= limit) throw new MetaPublishingException("META_RATE_LIMITED", true, true, retryAfter: 3600);
        }
        return [];
    }
    public abstract Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot snapshot,
        PublicationCheckpoint checkpoint, Credentials credentials, string? candidateExternalId, CancellationToken ct);
    protected bool Expired(PublicationCheckpoint cp) => cp.PreparedAt < Clock.GetUtcNow().AddHours(-23);
    protected static PublicationResult ResetExpired() => new(PublicationOutcome.Rejected)
        { ErrorCode = "PUBLICATION_MEDIA_EXPIRED", ConfirmedNotPublished = true, Retryable = true, Checkpoint = new() };
    protected static PublicationResult Continue(PublicationCheckpoint cp) => new(PublicationOutcome.Continue) { Checkpoint = cp, RetryAfterSeconds = 1 };
    protected static PublicationResult Unknown(string code) => new(PublicationOutcome.Unknown) { ErrorCode = code, RetryAfterSeconds = 60 };
    protected static bool ValidCheckpoint(PublicationSnapshot s, PublicationCheckpoint cp) => cp.Media.Length <= s.Images.Length
        && cp.Media.All(x => MetaPublishingClient.Id(x.ProviderId) && s.Images.Any(i => i.Order == x.Order))
        && cp.Media.Select(x => x.Order).Distinct().Count() == cp.Media.Length
        && cp.Media.Select(x => x.ProviderId).Distinct().Count() == cp.Media.Length
        && (cp.ContainerId == null || MetaPublishingClient.Id(cp.ContainerId));
}
