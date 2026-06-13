using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.New;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BasePaginationResponse> GetNewsAsync(int pageIndex, int pageSize, string? keyword, NewsStatus? status)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = _dbContext.NewsList
            .AsNoTracking()
            .Include(news => news.CategoryNewsDetail)
            .Where(news => !news.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var searchKeyword = keyword.Trim().ToLower();
            query = query.Where(news =>
                news.Title.ToLower().Contains(searchKeyword) ||
                news.Description.ToLower().Contains(searchKeyword));
        }
    
        if (status.HasValue)
        {
            query = query.Where(news => news.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(news => news.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(news => MapToResponse(news))
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<NewResponse?> GetNewByIdAsync(Guid id)
    {
        var news = await _dbContext.NewsList
            .AsNoTracking()
            .Include(news => news.CategoryNewsDetail)
            .FirstOrDefaultAsync(news => news.Id == id && !news.IsDeleted);

        if (news is null)
        {
            throw new NotFoundException("NEW_NOT_FOUND");
        }

        return MapToResponse(news);
    }

    public async Task<NewResponse> CreateNewAsync(CreateNewRequest request)
    {
        var userId = CheckAccessToken();
        if (userId == Guid.Empty)
        {
            throw new NotFoundException("USER_NOT_FOUND");
        }
        var categoryIds = NormalizeCategoryIds(request.CategoryIds);
        await EnsureCategoriesExistAsync(categoryIds);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var news = new News
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CoverImage = request.PictureUrl?.Trim() ?? string.Empty,
            Status = request.Status,
            UserId = userId,
            ViewCount = 0,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.NewsList.Add(news);
        _dbContext.CategoryNewsDetails.AddRange(categoryIds.Select(categoryId => new CategoryNewsDetail
        {
            Id = Guid.NewGuid(),
            CategoryNewsId = categoryId,
            NewsId = news.Id,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        }));

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        news.CategoryNewsDetail = categoryIds.Select(categoryId => new CategoryNewsDetail
        {
            CategoryNewsId = categoryId,
            NewsId = news.Id
        }).ToList();

        return MapToResponse(news);
    }

    public async Task<NewResponse?> UpdateNewAsync(Guid id, UpdateNewRequest request)
    {
        var categoryIds = NormalizeCategoryIds(request.CategoryIds);
        await EnsureCategoriesExistAsync(categoryIds);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var news = await _dbContext.NewsList
            .Include(news => news.CategoryNewsDetail)
            .FirstOrDefaultAsync(news => news.Id == id && !news.IsDeleted);

        if (news is null)
        {
            throw new NotFoundException("NEW_NOT_FOUND");
        }

        news.Title = request.Title;
        news.Description = request.Description;
        news.CoverImage = request.PictureUrl?.Trim() ?? string.Empty;
        news.Status = request.Status;
        news.UpdatedAt = DateTimeOffset.UtcNow;

        var currentCategoryDetails = news.CategoryNewsDetail.ToList();
        var currentCategoryIds = currentCategoryDetails.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList();
        var categoryDetailsToRemove = currentCategoryDetails
            .Where(categoryNewsDetail => !categoryIds.Contains(categoryNewsDetail.CategoryNewsId))
            .ToList();
        var categoryIdsToAdd = categoryIds
            .Where(categoryId => !currentCategoryIds.Contains(categoryId))
            .ToList();

        _dbContext.CategoryNewsDetails.RemoveRange(categoryDetailsToRemove);
        foreach (var categoryId in categoryIdsToAdd)
        {
            news.CategoryNewsDetail.Add(new CategoryNewsDetail
            {
                Id = Guid.NewGuid(),
                CategoryNewsId = categoryId,
                NewsId = news.Id,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(news);
    }

    public async Task<bool> DeleteNewAsync(Guid id)
    {
        var news = await _dbContext.NewsList
            .FirstOrDefaultAsync(news => news.Id == id && !news.IsDeleted);

        if (news is null)
        {
            throw new NotFoundException("NEW_NOT_FOUND");
        }

        news.IsDeleted = true;
        news.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task EnsureCategoriesExistAsync(List<Guid> categoryIds)
    {
        var existingCategoryCount = await _dbContext.CategoryNewsList
            .CountAsync(category => categoryIds.Contains(category.Id) && !category.IsDeleted);

        if (existingCategoryCount != categoryIds.Count)
        {
            throw new BadRequestException("CATEGORY_NEWS_INVALID");
        }
    }

    private static List<Guid> NormalizeCategoryIds(List<Guid> categoryIds)
    {
        var normalizedCategoryIds = categoryIds
            .Where(categoryId => categoryId != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedCategoryIds.Count == 0)
        {
            throw new BadRequestException("CATEGORY_IDS_REQUIRED");
        }

        return normalizedCategoryIds;
    }

    private static NewResponse MapToResponse(News news)
    {
        return new NewResponse
        {
            Id = news.Id,
            Title = news.Title,
            Description = news.Description,
            ViewCount = news.ViewCount,
            PictureUrl = news.CoverImage,
            Status = news.Status.ToString(),
            UserId = news.UserId,
            CategoryIds = news.CategoryNewsDetail.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList(),
            CreatedAt = news.CreatedAt,
            UpdatedAt = news.UpdatedAt
        };
    }
    private Guid CheckAccessToken()
    {
        var accessToken = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (accessToken != null)
        {
            return Guid.Parse(accessToken);
        }

        return Guid.Empty;
    }
}
