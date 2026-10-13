using System.Text;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class ZaloPublisher(ZaloPublishingClient client, PublishingMediaClient media,
    TokenProtector protector, IOptions<PlatformAccountOptions> options, TimeProvider clock) : IPublishingAdapter
{
    public PlatformType Platform => PlatformType.Zalo;
    private static string Purpose(PublicationSnapshot s) => $"postgen:zalo-article:{s.PlatformPostId}:{s.AccountId}:{s.GenerationId}";
    private static bool ArticleId(string? value) => value != null && Regex.IsMatch(value, @"\A[A-Za-z0-9_-]{1,200}\z");
    public IReadOnlyList<string> Validate(PublicationSnapshot s)
    {
        List<string> errors = [];
        if (s.Platform != Platform || !MetaPublishingClient.Id(s.ExternalAccountId) || s.PageId != null) errors.Add("ZALO_INVALID_TARGET");
        if (!options.Value.Zalo.Enabled || s.ApiVersion != "v2.0") errors.Add("ZALO_PUBLISHING_CONFIG_INVALID");
        if (s.FormatterVersion != "postgen-text-v1" || s.Text != string.Join("\n\n", new[] { s.Title, s.Content, s.Hashtag }.Where(x => !string.IsNullOrWhiteSpace(x))))
            errors.Add("PUBLICATION_FORMATTER_MISMATCH");
        if (string.IsNullOrWhiteSpace(s.Title) || s.Title.EnumerateRunes().Count() > 150) errors.Add("ZALO_TITLE_INVALID");
        if (string.IsNullOrWhiteSpace(s.Content)) errors.Add("MISSING_CONTENT");
        if (s.Article == null || string.IsNullOrWhiteSpace(s.Article.Author) || s.Article.Author.EnumerateRunes().Count() > 50
            || string.IsNullOrWhiteSpace(s.Article.Description) || s.Article.Description.EnumerateRunes().Count() > 300) errors.Add("ZALO_ARTICLE_METADATA_INVALID");
        if (s.Images.Length is < 1 or > 10) errors.Add("PUBLICATION_IMAGE_COUNT_INVALID"); // Product cap.
        if (s.Images.Select(x => x.Order).Distinct().Count() != s.Images.Length) errors.Add("PUBLICATION_IMAGE_ORDER_INVALID");
        foreach (var image in s.Images.Where(x => !PublishingMediaClient.Url(x.Url))) errors.Add("MEDIA_URL_INVALID_" + image.Order);
        return errors;
    }
    public async Task<IReadOnlyList<string>> Preflight(PublicationSnapshot s, Credentials c, CancellationToken ct)
    {
        // Read identity only. Refresh rotates a one-use token and must go through the account service's lease/persistence.
        var oa = await client.Send("oa/getoa", c, null, ct);
        return MetaPublishingClient.Text(oa, "oa_id") == s.ExternalAccountId ? [] : ["PLATFORM_ACCOUNT_MISMATCH"];
    }
    public async Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot s,
        PublicationCheckpoint cp, Credentials c, string? candidateExternalId, CancellationToken ct)
    {
        if (operation == PublicationOperation.Reconcile) return await Reconcile(s, cp, c, ct);
        if (cp.ProtectedProviderReference != null) return new(PublicationOutcome.Unknown) { ErrorCode = "ZALO_CREATE_ALREADY_SENT" };
        if (operation == PublicationOperation.Prepare)
        {
            foreach (var image in s.Images.OrderBy(x => x.Order)) await media.Inspect(image, Platform, ct);
            return new(PublicationOutcome.Continue) { Checkpoint = cp with { ReadyToPublish = true, PreparedAt = clock.GetUtcNow() }, RetryAfterSeconds = 1 };
        }
        if (!cp.ReadyToPublish) return new(PublicationOutcome.Rejected) { ErrorCode = "PUBLICATION_NOT_PREPARED", ConfirmedNotPublished = true };
        // Recheck URLs before the irreversible request; URL contents may have changed since preparation.
        foreach (var image in s.Images.OrderBy(x => x.Order)) await media.Inspect(image, Platform, ct);
        var images = s.Images.OrderBy(x => x.Order).ToArray();
        var body = new List<object> { new { type = "text", content = string.Join("\n\n", new[] { s.Content, s.Hashtag }.Where(x => !string.IsNullOrWhiteSpace(x))) } };
        body.AddRange(images.Skip(1).Select(x => (object)new { type = "image", url = x.Url }));
        var created = await client.Send("article/create", c, new
        {
            type = "normal", title = s.Title, author = s.Article!.Author, description = s.Article.Description,
            cover = new { cover_type = "photo", photo_url = images[0].Url, status = "show" },
            body, status = "show", comment = "show", related_medias = Array.Empty<string>(), tracking_link = ""
        }, ct);
        var reference = MetaPublishingClient.Text(created, "token");
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 16000)
            return new(PublicationOutcome.Unknown) { ErrorCode = "ZALO_PROCESS_REFERENCE_MISSING", PublishMayHaveOccurred = true };
        return new(PublicationOutcome.Pending)
        {
            Checkpoint = cp with { ProtectedProviderReference = protector.Protect(reference, Purpose(s)) },
            PublishMayHaveOccurred = true, RetryAfterSeconds = 15
        };
    }
    private async Task<PublicationResult> Reconcile(PublicationSnapshot s, PublicationCheckpoint cp, Credentials c, CancellationToken ct)
    {
        if (cp.ProtectedProviderReference == null)
            return new(PublicationOutcome.Unknown) { ErrorCode = "ZALO_PROCESS_REFERENCE_MISSING" };
        // Verify identifies the exact request; never infer success from matching text or a candidate supplied by a user.
        var token = protector.Unprotect(cp.ProtectedProviderReference, Purpose(s));
        var verified = await client.Send("article/verify", c, new { token }, ct);
        var id = MetaPublishingClient.Text(verified, "id");
        if (!ArticleId(id)) return new(PublicationOutcome.Pending)
            { Checkpoint = cp, PublishMayHaveOccurred = true, RetryAfterSeconds = 30, ErrorCode = "ZALO_PROCESSING" };
        var detail = await client.Send("article/getdetail?id=" + Uri.EscapeDataString(id!), c, null, ct);
        if (MetaPublishingClient.Text(detail, "id") != id || MetaPublishingClient.Text(detail, "status") != "show"
            || MetaPublishingClient.Text(detail, "type") != "normal" || MetaPublishingClient.Text(detail, "title") != s.Title)
            return new(PublicationOutcome.Unknown) { ErrorCode = "ZALO_PUBLICATION_UNVERIFIED" };
        return new(PublicationOutcome.Succeeded) { ExternalPostId = id, PublishMayHaveOccurred = true };
    }
}
