using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.PostGeneration;

public partial class Service(AppDbContext db, Ai.IService ai, ICurrentUserService currentUser,
    PostCore.IService postCore, ILogger<Service> logger) : IService
{
    private sealed record Target(PlatformType Platform, Guid PromptId, string Prompt);

    public async Task<PostCore.Response.PostCoreDetail> GeneratePlatformPosts(Guid id, Request.GenerateRequest request, CancellationToken cancellationToken = default)
    {
        var validation = new Validate.GenerateValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationExceptionError(validation.Errors[0].ErrorMessage);
        var userId = currentUser.GetRequiredUserId();
        var targets = await ReadTargets(request, cancellationToken);
        var (post, platforms, imagePlan) = await Claim(id, targets, request, userId, cancellationToken);
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(180));
        try
        {
            // All text calls must succeed before persisting any version in this request.
            var versions = new List<GeneratedPost>();
            foreach (var target in targets.Where(_ => request.Mode != "images"))
            {
                var output = await ai.GenerateContent(new(target.Prompt, target.Platform.ToString().ToLowerInvariant(),
                    new(post.Title, post.Content, post.Hashtag)), budget.Token);
                versions.Add(new GeneratedPost
                {
                    Id = Guid.NewGuid(), PlatformPostId = platforms.Single(x => x.Platform == target.Platform).Id,
                    Source = ContentSource.Ai, Title = output.Title, Content = output.Content, Hashtag = output.Hashtag,
                    SystemPromptConfigId = target.PromptId, PromptSnapshot = target.Prompt,
                    AiModel = output.Model, AiRequestId = output.RequestId, DurationMs = output.DurationMs,
                    UserId = userId, CreatedAt = DateTimeOffset.UtcNow
                });
            }
            var images = await GenerateImages(id, imagePlan, budget.Token, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            // Retain completed image results even when the AI budget expires; bound the final DB write separately.
            using var persist = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            persist.CancelAfter(TimeSpan.FromSeconds(10));
            await Persist(platforms, versions, imagePlan, images, persist.Token);
        }
        catch (Exception ex)
        {
            // Request cancellation must not leave the platform permanently Generating.
            await ReleaseFailed(platforms.Select(x => x.Id).ToArray());
            if (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
                throw new BadGatewayException("AI service is unavailable");
            throw;
        }
        return await postCore.GetPostCoreDetail(id, cancellationToken);
    }

    private async Task<List<Target>> ReadTargets(Request.GenerateRequest request, CancellationToken ct)
    {
        var configs = await db.Configs.AsNoTracking().Where(x => x.Group == "postgen" && !x.IsDeleted).ToListAsync(ct);
        EnsurePostGenEnabled(configs);
        var result = new List<Target>();
        foreach (var input in request.Platforms!)
        {
            var platform = input.Platform!.Value;
            var name = platform.ToString().ToLowerInvariant();
            var prompts = configs.Where(x => x.Key == "postgen.system-prompt." + name && x.Kind == "Setting" && !x.IsPublic).ToList();
            if (prompts.Count != 1) throw new BadRequestException($"System prompt for platform {name} is not configured");
            string prompt;
            try
            {
                using var json = JsonDocument.Parse(prompts[0].Value);
                if (!json.RootElement.GetProperty("isActive").GetBoolean()) throw new BadRequestException($"Platform {name} is disabled");
                // Images share their own instruction; platform Config is only needed for activation.
                prompt = request.Mode == "images" ? "" : json.RootElement.GetProperty("content").GetString()!;
                if (request.Mode != "images" && string.IsNullOrWhiteSpace(prompt)) throw new JsonException();
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException)
            { throw new BadRequestException($"System prompt for platform {name} is not configured"); }
            result.Add(new(platform, prompts[0].Id, prompt));
        }
        return result;
    }

    private static void EnsurePostGenEnabled(List<HoaTheoMua.Repository.Entity.Config> configs)
    {
        var enabled = configs.Where(x => x.Key == "postgen.enabled").ToList();
        if (enabled.Count > 1) throw new BadRequestException("PostGen enabled config is invalid");
        if (enabled.Count == 1)
        {
            try
            {
                using var json = JsonDocument.Parse(enabled[0].Value);
                if (!json.RootElement.GetBoolean()) throw new BadRequestException("PostGen is disabled");
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException)
            { throw new BadRequestException("PostGen enabled config is invalid"); }
        }
    }

    private async Task<(BasePost, List<PlatformPost>, List<ImageSelection>)> Claim(Guid id, List<Target> targets, Request.GenerateRequest request, Guid userId, CancellationToken ct)
    {
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        // Same lock as PostCore update/delete; release it before any outbound HTTP call.
        var post = await LockBasePost(id, ct);
        var imagePlan = request.Mode == "content" ? new List<ImageSelection>() : await ReadImagePlan(id, request, ct);
        var types = targets.Select(x => x.Platform).ToArray();
        var platforms = await db.PlatformPosts.Where(x => x.BasePostId == id && !x.IsDeleted && types.Contains(x.Platform)).ToListAsync(ct);
        if (platforms.Any(x => x.Status == PlatformPostStatus.Generating)) throw new ConflictException("Platform post is generating");
        if (platforms.Any(x => x.Status is not (PlatformPostStatus.Draft or PlatformPostStatus.Generated or PlatformPostStatus.Ready or PlatformPostStatus.Failed)))
            throw new ConflictException("Platform post cannot be generated in its current state");
        foreach (var target in targets)
        {
            var platform = platforms.SingleOrDefault(x => x.Platform == target.Platform);
            if (platform == null)
            {
                platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = id, Platform = target.Platform, UserId = userId, CreatedAt = DateTimeOffset.UtcNow };
                db.PlatformPosts.Add(platform); platforms.Add(platform);
            }
            platform.Status = PlatformPostStatus.Generating;
            platform.LastError = null; platform.UpdatedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        if (transaction != null) await transaction.CommitAsync(ct);
        return (post, platforms, imagePlan);
    }

    private async Task Persist(List<PlatformPost> platforms, List<GeneratedPost> versions, List<ImageSelection> plan, List<GeneratedPostImage> images, CancellationToken ct)
    {
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        if (images.Count > 0)
        {
            await LockBasePost(platforms[0].BasePostId, ct);
            await StampImageVersions(platforms[0].BasePostId, images, ct);
        }
        db.GeneratedPosts.AddRange(versions);
        db.GeneratedPostImages.AddRange(images);
        await db.SaveChangesAsync(ct);
        if (plan.Count > 0) await SelectImages(platforms, plan, images, ct);
        foreach (var platform in platforms)
        {
            var version = versions.SingleOrDefault(x => x.PlatformPostId == platform.Id);
            if (version != null)
            {
                platform.OfficialGenerationId = version.Id;
                platform.SelectedAt = DateTimeOffset.UtcNow;
            }
            platform.Status = PlatformPostStatus.Generated;
            platform.UpdatedAt = DateTimeOffset.UtcNow;
            var requested = plan.Where(x => x.Platform == platform.Platform).Select(x => x.Pair).ToHashSet();
            var failures = images.Count(x => requested.Contains(new(x.SourceBasePostImageId, x.RatioCode)) && x.Status == ImageStatus.Failed);
            platform.LastError = failures == 0 ? null : failures == requested.Count ? "All image generations failed" : "Some image generations failed";
            if (failures > 0 && failures == requested.Count) platform.Status = PlatformPostStatus.Failed;
        }
        await db.SaveChangesAsync(ct);
        if (transaction != null) await transaction.CommitAsync(ct);
    }

    private async Task ReleaseFailed(Guid[] ids)
    {
        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            db.ChangeTracker.Clear();
            var platforms = await db.PlatformPosts.Where(x => ids.Contains(x.Id) && x.Status == PlatformPostStatus.Generating).ToListAsync(cleanup.Token);
            foreach (var platform in platforms)
            {
                platform.Status = PlatformPostStatus.Failed;
                platform.LastError = "Post generation failed";
                platform.UpdatedAt = DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync(cleanup.Token);
        }
        catch (Exception)
        {
            logger.LogError("Could not release PostGen generation state for platform posts {PlatformIds}", ids);
        }
    }
    private async Task<BasePost> LockBasePost(Guid id, CancellationToken ct)
    {
        var post = db.Database.IsRelational()
            ? (await db.BasePosts.FromSqlInterpolated($"SELECT * FROM \"BasePost\" WHERE \"Id\" = {id} AND NOT \"IsDeleted\" FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
            : await db.BasePosts.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (post == null) throw new NotFoundException("Post not found");
        return post;
    }
}
