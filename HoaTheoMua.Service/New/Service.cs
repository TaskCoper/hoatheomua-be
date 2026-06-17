using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
            .Select(news => new NewResponse
            {
                Id = news.Id,
                Title = news.Title,
                Description = news.Description,
                ViewCount = news.ViewCount,
                PictureUrl = news.CoverImage,
                slug = news.slug,
                Status = news.Status == NewsStatus.Published ? "Published" : "Draft",
                UserId = news.UserId,
                CategoryIds = news.CategoryNewsDetail.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList(),
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<NewResponse?> GetNewByIdAsync(string key)
    {
        var normalizedKey = NormalizeLookupKey(key);
        var query = _dbContext.NewsList
            .AsNoTracking()
            .Where(news => !news.IsDeleted);

        var newsResponse = Guid.TryParse(normalizedKey, out var id)
            ? await query.Where(news => news.Id == id)
                .Select(news => new NewResponse
                {
                    Id = news.Id,
                    Title = news.Title,
                    Description = news.Description,
                    ViewCount = news.ViewCount,
                    PictureUrl = news.CoverImage,
                    slug = news.slug,
                    Status = news.Status == NewsStatus.Published ? "Published" : "Draft",
                    UserId = news.UserId,
                    CategoryIds = news.CategoryNewsDetail.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList(),
                    CreatedAt = news.CreatedAt,
                    UpdatedAt = news.UpdatedAt
                })
                .FirstOrDefaultAsync()
            : await query.Where(news => news.slug == normalizedKey)
                .Select(news => new NewResponse
                {
                    Id = news.Id,
                    Title = news.Title,
                    Description = news.Description,
                    ViewCount = news.ViewCount,
                    PictureUrl = news.CoverImage,
                    slug = news.slug,
                    Status = news.Status == NewsStatus.Published ? "Published" : "Draft",
                    UserId = news.UserId,
                    CategoryIds = news.CategoryNewsDetail.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList(),
                    CreatedAt = news.CreatedAt,
                    UpdatedAt = news.UpdatedAt
                })
                .FirstOrDefaultAsync();

        if (newsResponse is null)
        {
            throw new NotFoundException("NEW_NOT_FOUND");
        }

        return newsResponse;
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
        var slug = await GenerateUniqueSlugAsync(request.Title);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var news = new News
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CoverImage = request.PictureUrl?.Trim() ?? string.Empty,
            Status = request.Status,
            slug = slug,
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

        return new NewResponse
        {
            Id = news.Id,
            Title = news.Title,
            Description = news.Description,
            ViewCount = news.ViewCount,
            PictureUrl = news.CoverImage,
            slug = news.slug,
            Status = news.Status.ToString(),
            UserId = news.UserId,
            CategoryIds = news.CategoryNewsDetail.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList(),
            CreatedAt = news.CreatedAt,
            UpdatedAt = news.UpdatedAt
        };
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

        return new NewResponse
        {
            Id = news.Id,
            Title = news.Title,
            Description = news.Description,
            ViewCount = news.ViewCount,
            PictureUrl = news.CoverImage,
            slug = news.slug,
            Status = news.Status.ToString(),
            UserId = news.UserId,
            CategoryIds = news.CategoryNewsDetail.Select(categoryNewsDetail => categoryNewsDetail.CategoryNewsId).ToList(),
            CreatedAt = news.CreatedAt,
            UpdatedAt = news.UpdatedAt
        };
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

    private static string NormalizeLookupKey(string key)
    {
        var normalizedKey = key.Trim();

        if (string.IsNullOrWhiteSpace(normalizedKey) || normalizedKey.Length > 100)
        {
            throw new BadRequestException("NEW_LOOKUP_KEY_INVALID");
        }

        if (Guid.TryParse(normalizedKey, out _))
        {
            return normalizedKey;
        }

        if (!Regex.IsMatch(normalizedKey, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new BadRequestException("NEW_LOOKUP_KEY_INVALID");
        }

        return normalizedKey;
    }

    private async Task<string> GenerateUniqueSlugAsync(string title)
    {
        var baseSlug = GenerateSlug(title);
        var slug = baseSlug;
        var suffix = 2;

        while (await _dbContext.NewsList.AnyAsync(news => news.slug == slug))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string GenerateSlug(string title)
    {
        var normalizedTitle = title.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalizedTitle)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character == 'đ' ? 'd' : character);
            }
        }

        var slug = Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
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