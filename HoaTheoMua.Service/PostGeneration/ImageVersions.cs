using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.PostGen;
using Microsoft.EntityFrameworkCore;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.PostGeneration;

public partial class Service
{
    public async Task<Response.ImageVersion> RegenerateImage(Guid id, Request.RegenerateImageRequest request, CancellationToken cancellationToken = default)
    {
        var validation = new Validate.RegenerateImageValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationExceptionError(validation.Errors[0].ErrorMessage);
        currentUser.GetRequiredUserId();
        BasePostImage source;
        GeneratedPostImage? parent;
        string instruction;
        // Snapshot inputs under the same lock used by Post Core writes, then release before HTTP.
        // Regeneration only appends a shared revision; it never claims or changes a platform.
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null)
        {
            await LockBasePost(id, cancellationToken);
            source = await RequireImageSource(id, request.SourceBasePostImageId, cancellationToken);
            parent = request.SourceGeneratedPostImageId.HasValue
                ? await db.GeneratedPostImages.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.SourceGeneratedPostImageId, cancellationToken) : null;
            if (request.SourceGeneratedPostImageId.HasValue)
            {
                if (parent == null || parent.BasePostId != id || parent.SourceBasePostImageId != source.Id || parent.RatioCode != request.RatioCode)
                    throw new BadRequestException("Source image does not match the requested ratio");
                if (parent.Status != ImageStatus.Succeeded || string.IsNullOrWhiteSpace(parent.ImageUrl))
                    throw new BadRequestException("Cannot regenerate from a failed image");
            }
            EnsurePostGenEnabled(await db.Configs.AsNoTracking().Where(x => x.Group == "postgen" && !x.IsDeleted).ToListAsync(cancellationToken));
            instruction = await ReadImageInstruction(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
        }
        var (width, height) = AspectRatios.Pixels(request.RatioCode!.Value);
        var version = new GeneratedPostImage
        {
            Id = Guid.NewGuid(), BasePostId = id, SourceBasePostImageId = source.Id,
            RatioCode = request.RatioCode.Value, ParentGeneratedPostImageId = parent?.Id,
            SourceImageUrl = parent?.ImageUrl ?? source.ImageUrl, FeedbackInput = request.Feedback,
            PixelWidth = width, PixelHeight = height
        };
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(180));
        try
        {
            var output = await ai.GenerateImage(new(version.SourceImageUrl, version.RatioCode.ToString(), width, height,
                instruction, parent == null ? null : source.ImageUrl, request.Feedback), budget.Token);
            version.Status = ImageStatus.Succeeded; version.ImageUrl = output.ImageUrl;
            version.AiModel = output.Model; version.AiRequestId = output.RequestId; version.DurationMs = output.DurationMs;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { version.Status = ImageStatus.Failed; version.ErrorMessage = "Image generation budget exceeded"; }
        catch (AppException ex) when (ex is BadGatewayException or BadRequestException)
        { version.Status = ImageStatus.Failed; version.ErrorMessage = "AI image generation failed"; }
        cancellationToken.ThrowIfCancellationRequested();
        using var persist = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        persist.CancelAfter(TimeSpan.FromSeconds(10));
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(persist.Token) : null)
        {
            // Source removal/deletion while AI ran must not append to a deleted post or source.
            await LockBasePost(id, persist.Token);
            await RequireImageSource(id, source.Id, persist.Token);
            await StampImageVersions(id, new() { version }, persist.Token);
            db.GeneratedPostImages.Add(version);
            await db.SaveChangesAsync(persist.Token);
            if (transaction != null) await transaction.CommitAsync(persist.Token);
        }
        var history = await GetImageVersions(id, cancellationToken);
        return history.Groups.SelectMany(x => x.Versions).Single(x => x.GeneratedPostImageId == version.Id);
    }

    private async Task<BasePostImage> RequireImageSource(Guid postId, Guid sourceId, CancellationToken ct) =>
        await db.BasePostImages.AsNoTracking().SingleOrDefaultAsync(x => x.Id == sourceId && x.BasePostId == postId && !x.IsDeleted, ct)
            ?? throw new BadRequestException("Image does not belong to this post or has been deleted");

    // Call only while holding the BasePost row lock, in the insertion transaction.
    // Stamp at persistence, not before an unpredictable AI duration: later completions cannot
    // insert ahead of revisions already returned to clients, including generate and regenerate races.
    private async Task StampImageVersions(Guid postId, List<GeneratedPostImage> versions, CancellationToken ct)
    {
        var last = await db.GeneratedPostImages.Where(x => x.BasePostId == postId)
            .Select(x => (DateTimeOffset?)x.CreatedAt).MaxAsync(ct);
        foreach (var version in versions)
        {
            var now = DateTimeOffset.UtcNow;
            // PostgreSQL timestamp precision is microseconds (10 .NET ticks).
            now = new DateTimeOffset(now.Ticks - now.Ticks % 10, TimeSpan.Zero);
            version.CreatedAt = last.HasValue && now <= last.Value ? last.Value.AddTicks(10) : now;
            last = version.CreatedAt;
        }
    }

    public async Task<Response.ImageHistory> GetImageVersions(Guid id, CancellationToken cancellationToken = default)
    {
        currentUser.GetRequiredUserId();
        if (!await db.BasePosts.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)) throw new NotFoundException("Post not found");
        var sources = await db.BasePostImages.AsNoTracking().Where(x => x.BasePostId == id).ToDictionaryAsync(x => x.Id, cancellationToken);
        var versions = await db.GeneratedPostImages.AsNoTracking().Where(x => x.BasePostId == id).ToListAsync(cancellationToken);
        var selected = await (from slot in db.PlatformPostImages.AsNoTracking()
            join platform in db.PlatformPosts on slot.PlatformPostId equals platform.Id
            where platform.BasePostId == id && !platform.IsDeleted && slot.SelectedGeneratedPostImageId != null
            select new { Id = slot.SelectedGeneratedPostImageId!.Value, platform.Platform }).ToListAsync(cancellationToken);
        var selections = selected.GroupBy(x => x.Id).ToDictionary(x => x.Key,
            x => x.Select(s => s.Platform.ToString().ToLowerInvariant()).Distinct().Order().ToList());
        return new()
        {
            BasePostId = id,
            Groups = versions.GroupBy(x => new ImagePair(x.SourceBasePostImageId, x.RatioCode))
                .OrderBy(x => sources[x.Key.SourceId].SortOrder).ThenBy(x => x.Key.SourceId).ThenBy(x => x.Key.Ratio)
                .Select(group => new Response.ImageGroup
                {
                    SourceBasePostImageId = group.Key.SourceId, RatioCode = group.Key.Ratio.ToString(),
                    OriginalImageUrl = sources[group.Key.SourceId].ImageUrl, SourceIsDeleted = sources[group.Key.SourceId].IsDeleted,
                    Versions = group.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select((x, i) => new Response.ImageVersion
                    {
                        GeneratedPostImageId = x.Id, SourceBasePostImageId = x.SourceBasePostImageId, RatioCode = x.RatioCode.ToString(),
                        ParentGeneratedPostImageId = x.ParentGeneratedPostImageId, VersionNo = i + 1,
                        SourceImageUrl = x.SourceImageUrl, ImageUrl = x.ImageUrl, PixelWidth = x.PixelWidth, PixelHeight = x.PixelHeight,
                        Status = x.Status.ToString(), ErrorMessage = x.ErrorMessage, Feedback = x.FeedbackInput, CreatedAt = x.CreatedAt,
                        SelectedPlatforms = selections.GetValueOrDefault(x.Id) ?? new()
                    }).ToList()
                }).ToList()
        };
    }

    public async Task<PostCore.Response.PostCoreDetail> UpdatePlatformImages(Guid id, string platform, Request.UpdatePlatformImagesRequest request, CancellationToken cancellationToken = default)
    {
        var validation = new Validate.UpdatePlatformImagesValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationExceptionError(validation.Errors[0].ErrorMessage);
        currentUser.GetRequiredUserId();
        var type = ParsePlatform(platform);
        var inputs = request.Images!;
        if (inputs.Select(x => x.SortOrder).Distinct().Count() != inputs.Count) throw new BadRequestException("Duplicate image order");
        if (inputs.Select(x => new ImagePair(x.SourceBasePostImageId, x.RatioCode!.Value)).Distinct().Count() != inputs.Count)
            throw new BadRequestException("Duplicate image slot");
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null)
        {
            var (_, target) = await LockContentPlatform(id, type, cancellationToken);
            var sourceIds = inputs.Select(x => x.SourceBasePostImageId).ToArray();
            var sources = await db.BasePostImages.Where(x => sourceIds.Contains(x.Id) && x.BasePostId == id && !x.IsDeleted).Select(x => x.Id).ToListAsync(cancellationToken);
            if (inputs.Any(x => !sources.Contains(x.SourceBasePostImageId))) throw new BadRequestException("Image does not belong to this post or has been deleted");
            var selectedIds = inputs.Where(x => x.SelectedGeneratedPostImageId.HasValue).Select(x => x.SelectedGeneratedPostImageId!.Value).ToArray();
            var selected = await db.GeneratedPostImages.AsNoTracking().Where(x => selectedIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
            foreach (var input in inputs.Where(x => x.SelectedGeneratedPostImageId.HasValue))
            {
                if (!selected.TryGetValue(input.SelectedGeneratedPostImageId!.Value, out var image) || image.BasePostId != id)
                    throw new BadRequestException("Selected image does not belong to this post");
                if (image.SourceBasePostImageId != input.SourceBasePostImageId || image.RatioCode != input.RatioCode)
                    throw new BadRequestException("Selected image does not match the slot");
                if (image.Status != ImageStatus.Succeeded || string.IsNullOrWhiteSpace(image.ImageUrl))
                    throw new BadRequestException("Selected image is not available");
            }
            var slots = await db.PlatformPostImages.Where(x => x.PlatformPostId == target.Id).ToListAsync(cancellationToken);
            var pairs = inputs.Select(x => new ImagePair(x.SourceBasePostImageId, x.RatioCode!.Value)).ToHashSet();
            db.PlatformPostImages.RemoveRange(slots.Where(x => !pairs.Contains(new(x.SourceBasePostImageId, x.RatioCode))));
            var now = DateTimeOffset.UtcNow;
            foreach (var input in inputs)
            {
                var slot = slots.SingleOrDefault(x => x.SourceBasePostImageId == input.SourceBasePostImageId && x.RatioCode == input.RatioCode);
                if (slot == null)
                {
                    slot = new() { Id = Guid.NewGuid(), PlatformPostId = target.Id, SourceBasePostImageId = input.SourceBasePostImageId,
                        RatioCode = input.RatioCode!.Value, CreatedAt = now };
                    db.PlatformPostImages.Add(slot);
                }
                slot.SelectedGeneratedPostImageId = input.SelectedGeneratedPostImageId;
                slot.SortOrder = input.SortOrder; slot.UpdatedAt = now;
            }
            target.SelectedAt = target.UpdatedAt = now;
            // Readiness depends on both official text and the final selected image set.
            target.Status = target.OfficialGenerationId != null && inputs.Any(x => x.SelectedGeneratedPostImageId != null)
                ? PlatformPostStatus.Ready : target.OfficialGenerationId != null ? PlatformPostStatus.Generated : PlatformPostStatus.Draft;
            target.LastError = null;
            await db.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
        }
        return await postCore.GetPostCoreDetail(id, cancellationToken);
    }
}
