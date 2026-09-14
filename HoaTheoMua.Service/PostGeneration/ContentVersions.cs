using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.PostGeneration;

public partial class Service
{
    public async Task<PostCore.Response.PostCoreDetail> RegeneratePlatformContent(Guid id, string platform, Request.RegenerateContentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = new Validate.RegenerateContentValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationExceptionError(validation.Errors[0].ErrorMessage);
        var userId = currentUser.GetRequiredUserId();
        var type = ParsePlatform(platform);
        PlatformPost target;
        BasePost post;
        GeneratedPost? parent;
        Target prompt;
        // Lock and snapshot the selected source before claiming. Manual edits use the same lock
        // and reject Generating, so the selected source cannot change during the provider call.
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null)
        {
            (post, target) = await LockContentPlatform(id, type, cancellationToken);
            parent = request.SourceGenerationId.HasValue
                ? await db.GeneratedPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.SourceGenerationId && x.PlatformPostId == target.Id, cancellationToken)
                    ?? throw new BadRequestException("Source generation does not belong to this platform post")
                : null;
            prompt = (await ReadTargets(new() { Mode = "content", Platforms = new() { new() { Platform = type } } }, cancellationToken)).Single();
            target.Status = PlatformPostStatus.Generating;
            target.LastError = null;
            target.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
        }
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(180));
        try
        {
            var output = await ai.GenerateContent(new(prompt.Prompt, type.ToString().ToLowerInvariant(),
                new(post.Title, post.Content, post.Hashtag),
                parent == null ? null : new Ai.Request.SourceText(parent.Title, parent.Content, parent.Hashtag), request.Feedback), budget.Token);
            var version = new GeneratedPost
            {
                Id = Guid.NewGuid(), PlatformPostId = target.Id, Source = ContentSource.Ai,
                ParentGeneratedPostId = parent?.Id, FeedbackInput = request.Feedback,
                Title = output.Title, Content = output.Content, Hashtag = output.Hashtag,
                SystemPromptConfigId = prompt.PromptId, PromptSnapshot = prompt.Prompt,
                AiModel = output.Model, AiRequestId = output.RequestId, DurationMs = output.DurationMs,
                UserId = userId, CreatedAt = DateTimeOffset.UtcNow
            };
            using var persist = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            persist.CancelAfter(TimeSpan.FromSeconds(10));
            await Persist(new() { target }, new() { version }, new(), new(), persist.Token);
        }
        catch (Exception ex)
        {
            await ReleaseFailed(new[] { target.Id });
            if (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
                throw new BadGatewayException("AI service is unavailable");
            throw;
        }
        return await postCore.GetPostCoreDetail(id, cancellationToken);
    }

    public async Task<PostCore.Response.PostCoreDetail> EditPlatformContent(Guid id, string platform, Request.EditContentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = new Validate.EditContentValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationExceptionError(validation.Errors[0].ErrorMessage);
        var userId = currentUser.GetRequiredUserId();
        var type = ParsePlatform(platform);
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null)
        {
            var (_, target) = await LockContentPlatform(id, type, cancellationToken);
            var latest = await db.GeneratedPosts.Where(x => x.PlatformPostId == target.Id)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            // Coalesce repeated saves only while editing the current, latest manual revision.
            // Selecting an older revision first starts a new branch instead of rewriting another branch.
            var reuse = latest?.Source == ContentSource.Manual && latest.Id == target.OfficialGenerationId
                && !await db.GeneratedPosts.AnyAsync(x => x.ParentGeneratedPostId == latest.Id, cancellationToken);
            var version = reuse ? latest! : new GeneratedPost
            {
                Id = Guid.NewGuid(), PlatformPostId = target.Id, Source = ContentSource.Manual,
                ParentGeneratedPostId = target.OfficialGenerationId, CreatedAt = DateTimeOffset.UtcNow
            };
            version.Title = request.Title; version.Content = request.Content!; version.Hashtag = request.Hashtag;
            version.UserId = userId;
            if (!reuse) db.GeneratedPosts.Add(version);
            await db.SaveChangesAsync(cancellationToken);
            target.OfficialGenerationId = version.Id;
            target.SelectedAt = target.UpdatedAt = DateTimeOffset.UtcNow;
            // Manual edits preserve processing state, including image failures and Ready.
            await db.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
        }
        return await postCore.GetPostCoreDetail(id, cancellationToken);
    }

    public async Task<Response.ContentHistory> GetContentVersions(Guid id, string platform, CancellationToken cancellationToken = default)
    {
        currentUser.GetRequiredUserId();
        var type = ParsePlatform(platform);
        if (!await db.BasePosts.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)) throw new NotFoundException("Post not found");
        var target = await db.PlatformPosts.AsNoTracking().SingleOrDefaultAsync(x => x.BasePostId == id && x.Platform == type && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Platform post not found");
        var versions = await db.GeneratedPosts.AsNoTracking().Where(x => x.PlatformPostId == target.Id)
            .Select(x => new { x.Id, x.ParentGeneratedPostId, x.Source, x.Title, x.Content, x.Hashtag, x.CreatedAt, x.FeedbackInput })
            .ToListAsync(cancellationToken);
        return new()
        {
            PlatformPostId = target.Id, Platform = type.ToString().ToLowerInvariant(), OfficialGenerationId = target.OfficialGenerationId,
            Versions = versions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select((x, i) => new Response.ContentRevision
            {
                GenerationId = x.Id, ParentGeneratedPostId = x.ParentGeneratedPostId, VersionNo = i + 1,
                Source = x.Source.ToString(), Title = x.Title, Content = x.Content, Hashtag = x.Hashtag,
                CreatedAt = x.CreatedAt, Feedback = x.FeedbackInput, IsOfficial = x.Id == target.OfficialGenerationId
            }).ToList()
        };
    }

    public async Task<PostCore.Response.PostCoreDetail> SelectOfficialContent(Guid id, string platform, Request.SelectContentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = new Validate.SelectContentValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationExceptionError(validation.Errors[0].ErrorMessage);
        currentUser.GetRequiredUserId();
        var type = ParsePlatform(platform);
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null)
        {
            var (_, target) = await LockContentPlatform(id, type, cancellationToken);
            if (!await db.GeneratedPosts.AnyAsync(x => x.Id == request.GenerationId && x.PlatformPostId == target.Id, cancellationToken))
                throw new BadRequestException("Selected generation does not belong to this platform post");
            target.OfficialGenerationId = request.GenerationId;
            target.SelectedAt = target.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
        }
        return await postCore.GetPostCoreDetail(id, cancellationToken);
    }

    private static PlatformType ParsePlatform(string platform)
    {
        if (!Enum.GetNames<PlatformType>().Contains(platform, StringComparer.OrdinalIgnoreCase))
            throw new ValidationExceptionError("Platform must be facebook, instagram or zalo");
        return Enum.Parse<PlatformType>(platform, true);
    }

    private async Task<(BasePost, PlatformPost)> LockContentPlatform(Guid id, PlatformType type, CancellationToken ct)
    {
        var post = await LockBasePost(id, ct);
        var platform = await db.PlatformPosts.SingleOrDefaultAsync(x => x.BasePostId == id && x.Platform == type && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Platform post not found");
        if (platform.Status == PlatformPostStatus.Generating) throw new ConflictException("Platform post is generating");
        if (await db.PostPublications.AnyAsync(x => x.PlatformPostId == platform.Id && PostPublishing.Store.ActiveStates.Contains(x.Status), ct))
            throw new PostPublishing.PublicationException(409, "PUBLICATION_LOCKED", "Bản chốt đang được tác vụ đăng sử dụng.");
        if (platform.Status is not (PlatformPostStatus.Draft or PlatformPostStatus.Generated or PlatformPostStatus.Ready or PlatformPostStatus.Failed or PlatformPostStatus.PublishFailed))
            throw new ConflictException("Platform post content cannot be changed in its current state");
        return (post, platform);
    }
}
