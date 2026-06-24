using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CategoryNew;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public Service(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    private async Task<bool> checkExistedCategory(Guid categoryId)
    {
        var checkCategoryNews = await _dbContext.CategoryNewsList.FirstOrDefaultAsync(x => x.Id == categoryId);
        if (checkCategoryNews != null)
        {
            return true;
        }

        return false;
    }

    private async Task<CategoryNews?> GetActiveCategory(Guid categoryId)
    {
        return await _dbContext.CategoryNewsList.FirstOrDefaultAsync(x => x.Id == categoryId && !x.IsDeleted);
    }

    private static Response.CategoryResponse MapCategoryResponse(CategoryNews category)
    {
        return new Response.CategoryResponse()
        {
            categoryId = category.Id,
            title = category.Title,
            userId = category.UserId,
            parentId = category.ParentCategoryNewsId,
        };
    }

    public async Task<Response.CreateCategoryResponse> CreateCate(Request.CreateCategoriesRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        if (string.IsNullOrWhiteSpace(request.title))
        {
            throw new BadRequestException("CATEGORY_TITLE_REQUIRED");
        }

        var isDuplicateTitle = await _dbContext.CategoryNewsList.AnyAsync(x =>
            !x.IsDeleted &&
            x.ParentCategoryNewsId == request.parentId &&
            x.Title == request.title);
        if (isDuplicateTitle)
        {
            throw new ConflictException("CATEGORY_ALREADY_EXISTS");
        }

        var newCate = new CategoryNews()
        {
            Id = Guid.NewGuid(),
            Title = request.title,
            UserId = userId,
        };
        if (request.parentId != null)
        {
            var parentCategory = await GetActiveCategory(request.parentId.Value);
            if (parentCategory == null)
            {
                throw new NotFoundException("CATEGORY_NOT_FOUND");
            }

            if (parentCategory.ParentCategoryNewsId != null)
            {
                throw new BadRequestException("CATEGORY_PARENT_INVALID");
            }

            newCate.ParentCategoryNewsId = request.parentId;
        }

        var result = new Response.CreateCategoryResponse()
        {
            categoryId = newCate.Id,
            title = newCate.Title,
            userId = newCate.UserId,
            parentId = newCate.ParentCategoryNewsId,
        };
        await _dbContext.CategoryNewsList.AddAsync(newCate);
        await _dbContext.SaveChangesAsync();
        return result;
    }


    public async Task<BasePaginationResponse> GetCategories(int pageIndex, int pageSize, string? keyword)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var rootQuery = _dbContext.CategoryNewsList
            .Where(x => !x.IsDeleted && x.ParentCategoryNewsId == null);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();

            // Giữ root nếu chính nó khớp HOẶC có con khớp keyword
            rootQuery = rootQuery.Where(root =>
                root.Title.ToLower().Contains(kw)
                || root.ChildCategoryNews.Any(c => !c.IsDeleted && c.Title.ToLower().Contains(kw)));
        }

        var totalRootCount = await rootQuery.CountAsync();

        var pageRoots = await rootQuery
            .OrderBy(x => x.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.ChildCategoryNews.Where(c => !c.IsDeleted))
            .ToListAsync();

        var response = pageRoots.Select(MapToResponse).ToList();

        return ApiResponseFactory.BasePagination(
            items: response,
            pageIndex: pageIndex,
            pageSize: pageSize,
            totalCount: totalRootCount);
    }

    private static Response.CategoryResponse MapToResponse(CategoryNews x) => new()
    {
        categoryId = x.Id,
        title = x.Title,
        userId = x.UserId,
        parentId = x.ParentCategoryNewsId,
        children = x.ChildCategoryNews?
            .Select(MapToResponse)
            .ToList() ?? new List<Response.CategoryResponse>()
    };


    public async Task<Response.CategoryResponse> GetCategory(Guid categoryId)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        var result = MapCategoryResponse(category);
        if (category.ParentCategoryNewsId != null)
        {
            var parentCateId = category.ParentCategoryNewsId;
            result.Parent = await _dbContext.CategoryNewsList.Where(x => x.Id == parentCateId && !x.IsDeleted)
                .Select(x => new Response.CategoryResponse()
                {
                    categoryId = x.Id,
                    title = x.Title,
                    userId = x.UserId,
                    parentId = x.ParentCategoryNewsId,
                    children =  new List<Response.CategoryResponse>(),
                }).FirstOrDefaultAsync();
        }

        var childCategories = await _dbContext.CategoryNewsList
            .Where(x => x.ParentCategoryNewsId == categoryId && !x.IsDeleted)
            .Select(c => new Response.CategoryResponse(){
                categoryId = c.Id,
                title = c.Title,
                parentId = c.ParentCategoryNewsId,
                userId = c.UserId
            })
            .ToListAsync();

        var newsCount = await _dbContext.CategoryNewsDetails
            .Where(x => x.CategoryNewsId == categoryId && !x.IsDeleted).CountAsync();

        result.children = childCategories;
        result.count = newsCount;

        return result;
    }

    public async Task<Response.CategoryResponse> UpdateCate(Guid categoryId, Request.UpdateCategoriesRequest request)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        if (string.IsNullOrWhiteSpace(request.title))
        {
            throw new BadRequestException("CATEGORY_TITLE_REQUIRED");
        }

        if (request.parentId == categoryId)
        {
            throw new BadRequestException("CATEGORY_PARENT_INVALID");
        }

        var isDuplicateTitle = await _dbContext.CategoryNewsList.AnyAsync(x =>
            !x.IsDeleted &&
            x.Id != categoryId &&
            x.ParentCategoryNewsId == request.parentId &&
            x.Title == request.title);
        if (isDuplicateTitle)
        {
            throw new ConflictException("CATEGORY_ALREADY_EXISTS");
        }

        if (request.parentId != null)
        {
            var parentCategory = await GetActiveCategory(request.parentId.Value);
            if (parentCategory == null)
            {
                throw new NotFoundException("PARENT_CATEGORY_NOT_FOUND");
            }

            if (parentCategory.ParentCategoryNewsId != null)
            {
                throw new BadRequestException("CATEGORY_PARENT_INVALID");
            }

            var hasChildren = await _dbContext.CategoryNewsList.AnyAsync(x =>
                !x.IsDeleted && x.ParentCategoryNewsId == categoryId);
            if (hasChildren)
            {
                throw new BadRequestException("CATEGORY_PARENT_INVALID");
            }
        }

        category.Title = request.title;
        category.ParentCategoryNewsId = request.parentId;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return MapCategoryResponse(category);
    }

    public async Task DeleteCate(Guid categoryId)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        var categories = new List<CategoryNews> { category };
        if (category.ParentCategoryNewsId == null)
        {
            var childCategories = await _dbContext.CategoryNewsList
                .Where(x => !x.IsDeleted && x.ParentCategoryNewsId == categoryId)
                .ToListAsync();
            categories.AddRange(childCategories);
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var item in categories)
        {
            item.IsDeleted = true;
            item.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();
    }
}