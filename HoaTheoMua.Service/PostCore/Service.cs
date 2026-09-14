using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.PostCore;

public class Service(AppDbContext db) : IService
{
    private IQueryable<BasePost> ActivePosts() => db.BasePosts.Where(x => !x.IsDeleted);

    private async Task<BasePost> FindPost(Guid id, CancellationToken ct) =>
        await ActivePosts().FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Post not found");

    public async Task<Response.PostCoreDetail> CreatePostCore(Request.CreatePostCoreRequest request, CancellationToken cancellationToken = default)
    {
        var post = new BasePost
        {
            Id = Guid.NewGuid(), Title = request.Title!, Content = request.Content!, Hashtag = request.Hashtag,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.BasePosts.Add(post);
        db.BasePostImages.AddRange(request.Images!.Select(x => NewImage(post.Id, x)));
        await db.SaveChangesAsync(cancellationToken);
        return await GetPostCoreDetail(post.Id, cancellationToken);
    }

    public async Task<BasePaginationResponse> GetPostCoreList(Request.GetPostCoreListRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.PageIndex, 1);
        var size = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);
        var query = ActivePosts().AsNoTracking();
        if (request.Platform.HasValue)
            query = query.Where(x => db.PlatformPosts.Any(p => p.BasePostId == x.Id && !p.IsDeleted && p.Platform == request.Platform));
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(keyword) || x.Content.ToLower().Contains(keyword));
        }
        var count = await query.CountAsync(cancellationToken);
        // Avoid integer overflow for arbitrary page numbers from the query string.
        var offset = (int)Math.Min((long)(page - 1) * size, int.MaxValue);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip(offset).Take(size).Select(x => new Response.PostCoreSummary
            {
                BasePostId = x.Id, Title = x.Title, Content = x.Content, Hashtag = x.Hashtag,
                CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
            }).ToListAsync(cancellationToken);
        var response = ApiResponseFactory.BasePagination(items, page, size, count);
        response.Value.HasNextPage = (long)page * size < count;
        return response;
    }

    public async Task<Response.PostCoreDetail> GetPostCoreDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await ActivePosts().AsNoTracking().Where(x => x.Id == id)
            .Select(x => new Response.PostCoreDetail
            {
                BasePostId = x.Id, Title = x.Title, Content = x.Content, Hashtag = x.Hashtag,
                CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
            }).FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Post not found");
        result.BaseImages = await db.BasePostImages.AsNoTracking().Where(x => x.BasePostId == id && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).Select(x => new Response.BaseImage
            {
                Id = x.Id, ImageUrl = x.ImageUrl, SortOrder = x.SortOrder, Width = x.Width, Height = x.Height
            }).ToListAsync(cancellationToken);
        // Project only response fields; prompt snapshots and provider metadata can be large.
        var images = await db.GeneratedPostImages.AsNoTracking().Where(x => x.BasePostId == id)
            .Select(x => new
            {
                x.Id, x.SourceBasePostImageId, x.RatioCode, x.ParentGeneratedPostImageId,
                x.PixelWidth, x.PixelHeight, x.ImageUrl, x.Status, x.ErrorMessage, x.CreatedAt
            }).ToListAsync(cancellationToken);
        result.Images = images.GroupBy(x => new { x.SourceBasePostImageId, x.RatioCode })
            .OrderBy(x => x.Key.SourceBasePostImageId).ThenBy(x => x.Key.RatioCode)
            .SelectMany(group => group.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select((x, index) => new Response.GeneratedImage
            {
                Id = x.Id, SourceBasePostImageId = x.SourceBasePostImageId, RatioCode = x.RatioCode.ToString(),
                ParentGeneratedPostImageId = x.ParentGeneratedPostImageId, VersionNo = index + 1,
                PixelWidth = x.PixelWidth, PixelHeight = x.PixelHeight, ImageUrl = x.ImageUrl,
                Status = x.Status.ToString(), ErrorMessage = x.ErrorMessage, CreatedAt = x.CreatedAt
            })).ToList();
        var platforms = await db.PlatformPosts.AsNoTracking().Where(x => x.BasePostId == id && !x.IsDeleted)
            .OrderBy(x => x.Platform)
            .Select(x => new { x.Id, x.Platform, x.Status, x.LastError, x.OfficialGenerationId })
            .ToListAsync(cancellationToken);
        if (platforms.Count == 0) return result;

        // Separate collections avoid multiplying rows in a join of versions and image slots.
        var platformIds = platforms.Select(x => x.Id).ToList();
        var versions = await db.GeneratedPosts.AsNoTracking().Where(x => platformIds.Contains(x.PlatformPostId))
            .Select(x => new
            {
                x.PlatformPostId, x.Id, x.ParentGeneratedPostId, x.Source,
                x.Title, x.Content, x.Hashtag, x.CreatedAt
            }).ToListAsync(cancellationToken);
        var slots = await db.PlatformPostImages.AsNoTracking().Where(x => platformIds.Contains(x.PlatformPostId))
            .Select(x => new { x.PlatformPostId, x.SourceBasePostImageId, x.RatioCode, x.SelectedGeneratedPostImageId, x.SortOrder })
            .ToListAsync(cancellationToken);
        var versionsByPlatform = versions.ToLookup(x => x.PlatformPostId);
        var slotsByPlatform = slots.ToLookup(x => x.PlatformPostId);
        result.Platforms = platforms.Select(p =>
        {
            var contentVersions = versionsByPlatform[p.Id].OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                .Select((x, index) => new Response.ContentVersion
                {
                    GenerationId = x.Id, ParentGeneratedPostId = x.ParentGeneratedPostId, VersionNo = index + 1,
                    Source = x.Source.ToString(), Title = x.Title, Content = x.Content, Hashtag = x.Hashtag, CreatedAt = x.CreatedAt
                }).ToList();
            var platformSlots = slotsByPlatform[p.Id].OrderBy(x => x.SortOrder).ToList();
            return new Response.PlatformDetail
            {
                PlatformPostId = p.Id, Platform = p.Platform.ToString().ToLowerInvariant(),
                Status = p.Status is PlatformPostStatus.Generated or PlatformPostStatus.Ready
                    && platformSlots.Any(x => x.SelectedGeneratedPostImageId == null)
                    && platformSlots.Any(x => x.SelectedGeneratedPostImageId != null) ? "Partial" : p.Status.ToString(),
                LastError = p.LastError, Versions = contentVersions,
                Content = contentVersions.FirstOrDefault(x => x.GenerationId == p.OfficialGenerationId),
                Images = platformSlots.Select(x => new Response.ImageSlot
                {
                    SourceBasePostImageId = x.SourceBasePostImageId, RatioCode = x.RatioCode.ToString(),
                    SelectedGeneratedPostImageId = x.SelectedGeneratedPostImageId, SortOrder = x.SortOrder
                }).ToList()
            };
        }).ToList();
        return result;
    }

    public async Task<Response.PostCoreDetail> UpdatePostCore(Guid id, Request.UpdatePostCoreRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null;
        var post = await LockPost(id, cancellationToken);
        if (await db.PlatformPosts.AnyAsync(x => x.BasePostId == id && !x.IsDeleted && x.Status == PlatformPostStatus.Generating, cancellationToken))
            throw new ConflictException("Platform post is generating");
        var images = await db.BasePostImages.Where(x => x.BasePostId == id && !x.IsDeleted).ToListAsync(cancellationToken);
        foreach (var input in request.Images!.Where(x => x.Id.HasValue))
        {
            var existing = images.FirstOrDefault(x => x.Id == input.Id) ?? throw new BadRequestException("Image does not belong to this post");
            if (existing.ImageUrl != input.ImageUrl)
                throw new BadRequestException("Existing image URL cannot be changed; add a new image instead");
        }
        post.Title = request.Title!;
        post.Content = request.Content!;
        post.Hashtag = request.Hashtag;
        post.UpdatedAt = DateTimeOffset.UtcNow;
        // Release active sort orders in a separate flush, inside the same transaction.
        // This also preserves old source rows referenced by generated image history.
        foreach (var image in images) image.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);
        foreach (var input in request.Images!)
        {
            if (input.Id.HasValue)
            {
                var image = images.Single(x => x.Id == input.Id);
                image.IsDeleted = false;
                image.SortOrder = input.SortOrder;
                image.Width = input.Width;
                image.Height = input.Height;
            }
            else db.BasePostImages.Add(NewImage(id, input));
        }
        await db.SaveChangesAsync(cancellationToken);
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return await GetPostCoreDetail(id, cancellationToken);
    }

    public async Task<Response.DeletedPost> DeletePostCore(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null;
        var post = await LockPost(id, cancellationToken);
        var platforms = await db.PlatformPosts.Where(x => x.BasePostId == id && !x.IsDeleted).ToListAsync(cancellationToken);
        if (platforms.Any(x => x.Status == PlatformPostStatus.Generating)) throw new ConflictException("Platform post is generating");
        post.IsDeleted = true;
        post.UpdatedAt = DateTimeOffset.UtcNow;
        foreach (var platform in platforms) { platform.IsDeleted = true; platform.UpdatedAt = post.UpdatedAt; }
        var images = await db.BasePostImages.Where(x => x.BasePostId == id && !x.IsDeleted).ToListAsync(cancellationToken);
        foreach (var image in images) image.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return new Response.DeletedPost(id, true);
    }

    private async Task<BasePost> LockPost(Guid id, CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return await FindPost(id, ct);
        var rows = await db.BasePosts.FromSqlInterpolated($"SELECT * FROM \"BasePost\" WHERE \"Id\" = {id} AND NOT \"IsDeleted\" FOR UPDATE")
            .ToListAsync(ct);
        return rows.SingleOrDefault() ?? throw new NotFoundException("Post not found");
    }

    private static BasePostImage NewImage(Guid postId, Request.ImageInput input) => new()
    {
        Id = Guid.NewGuid(), BasePostId = postId, ImageUrl = input.ImageUrl!, SortOrder = input.SortOrder,
        Width = input.Width, Height = input.Height, CreatedAt = DateTimeOffset.UtcNow
    };
}
