using System.Data;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.PostGeneration;

public partial class Service
{
    public async Task<Response.OfficialVersion> GetOfficialVersion(Guid id, string platform, CancellationToken cancellationToken = default)
    {
        currentUser.GetRequiredUserId();
        var type = ParsePlatform(platform);
        // All reads must observe one committed snapshot, including a mutable Manual revision.
        // No locks are held against writers and this GET never changes readiness in storage.
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken) : null;
        if (!await db.BasePosts.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)) throw new NotFoundException("Post not found");
        var target = await db.PlatformPosts.AsNoTracking().Where(x => x.BasePostId == id && x.Platform == type && !x.IsDeleted)
            .Select(x => new { x.Id, x.Status, x.OfficialGenerationId, x.SelectedAt }).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Platform post not found");
        var content = target.OfficialGenerationId.HasValue
            ? await db.GeneratedPosts.AsNoTracking().Where(x => x.Id == target.OfficialGenerationId && x.PlatformPostId == target.Id)
                .Select(x => new { x.Id, x.Title, x.Content, x.Hashtag }).SingleOrDefaultAsync(cancellationToken) : null;
        var images = await (from slot in db.PlatformPostImages.AsNoTracking()
            join image in db.GeneratedPostImages.AsNoTracking() on slot.SelectedGeneratedPostImageId equals (Guid?)image.Id
            where slot.PlatformPostId == target.Id && image.BasePostId == id
                && image.SourceBasePostImageId == slot.SourceBasePostImageId && image.RatioCode == slot.RatioCode
                && image.Status == ImageStatus.Succeeded && image.ImageUrl != null && image.ImageUrl.Trim() != ""
            orderby slot.SortOrder, slot.Id
            select new PostCore.Response.OfficialImage
            {
                SourceBasePostImageId = slot.SourceBasePostImageId, RatioCode = slot.RatioCode.ToString(),
                SelectedGeneratedPostImageId = image.Id, ImageUrl = image.ImageUrl!,
                PixelWidth = image.PixelWidth, PixelHeight = image.PixelHeight, SortOrder = slot.SortOrder
            }).ToListAsync(cancellationToken);
        var reasons = new List<string>();
        var hasContent = content != null && !string.IsNullOrWhiteSpace(content.Content);
        if (!hasContent) reasons.Add("MissingContent");
        if (images.Count == 0) reasons.Add("MissingImages");
        if (target.Status == PlatformPostStatus.Generating) reasons.Add("GenerationInProgress");
        var publishing = target.Status is PlatformPostStatus.Scheduled or PlatformPostStatus.Publishing or PlatformPostStatus.Published or PlatformPostStatus.PublishFailed;
        if (publishing) reasons.Add("PublishingState");
        var isReady = reasons.Count == 0;
        // This status describes the returned payload. A prior failed AI attempt does not erase
        // usable official data; missing data can never be reported Ready based only on a stored flag.
        var status = target.Status == PlatformPostStatus.Generating || publishing ? target.Status.ToString()
            : isReady ? "Ready" : hasContent ? "Generated" : "Draft";
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return new()
        {
            BasePostId = id, PlatformPostId = target.Id, Platform = type.ToString().ToLowerInvariant(),
            Status = status, IsReady = isReady, NotReadyReasons = reasons,
            GenerationId = content?.Id, Title = content?.Title, Content = content?.Content, Hashtag = content?.Hashtag,
            SelectedAt = target.SelectedAt, Images = images
        };
    }
}
