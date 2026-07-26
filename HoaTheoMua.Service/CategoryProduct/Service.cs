using HoaTheoMua.Repository;
using CategoryProductEntity = HoaTheoMua.Repository.Entity.CategoryProduct;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CategoryProduct;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public Service(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    private async Task<CategoryProductEntity?> GetActiveCategory(Guid categoryId)
    {
        return await _dbContext.CategoryProducts.FirstOrDefaultAsync(x => x.Id == categoryId && !x.IsDeleted);
    }

    private static Response.CategoryProductResponse MapCategoryResponse(CategoryProductEntity category)
    {
        return new Response.CategoryProductResponse()
        {
            categoryId = category.Id,
            name = category.Name,
            name_En = category.Name_En,
            description = category.Description,
            description_En = category.Description_En,
            position = category.Position,
            metaData = category.MetaData,
            startTime = category.StartTime,
            endTime = category.EndTime,
            type = category.Type,
            isActive = category.IsActive,
            userId = category.UserId,
            parentId = category.ParentCategoryProductId,
        };
    }

    public async Task<Response.CreateCategoryProductResponse> CreateCateProduct(Request.CreateCategoryProductRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        if (string.IsNullOrWhiteSpace(request.name))
        {
            throw new BadRequestException("CATEGORY_NAME_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.name_En))
        {
            throw new BadRequestException("CATEGORY_NAME_EN_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.description))
        {
            throw new BadRequestException("CATEGORY_DESCRIPTION_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.description_En))
        {
            throw new BadRequestException("CATEGORY_DESCRIPTION_EN_REQUIRED");
        }

        var isDuplicateName = await _dbContext.CategoryProducts.AnyAsync(x =>
            !x.IsDeleted &&
            x.ParentCategoryProductId == request.parentId &&
            x.Name == request.name);
        if (isDuplicateName)
        {
            throw new ConflictException("CATEGORY_ALREADY_EXISTS");
        }

        var newCate = new CategoryProductEntity()
        {
            Id = Guid.NewGuid(),
            Name = request.name,
            Name_En = request.name_En,
            Description = request.description,
            Description_En = request.description_En,
            Position = request.position,
            MetaData = request.metaData,
            StartTime = request.startTime,
            EndTime = request.endTime,
            Type = request.type,
            IsActive = request.isActive,
            UserId = userId,
        };

        if (request.parentId != null)
        {
            var parentCategory = await GetActiveCategory(request.parentId.Value);
            if (parentCategory == null)
            {
                throw new NotFoundException("CATEGORY_NOT_FOUND");
            }

            newCate.ParentCategoryProductId = request.parentId;
        }

        await _dbContext.CategoryProducts.AddAsync(newCate);
        await _dbContext.SaveChangesAsync();

        return new Response.CreateCategoryProductResponse()
        {
            categoryId = newCate.Id,
            name = newCate.Name,
            name_En = newCate.Name_En,
            description = newCate.Description,
            description_En = newCate.Description_En,
            position = newCate.Position,
            metaData = newCate.MetaData,
            startTime = newCate.StartTime,
            endTime = newCate.EndTime,
            type = newCate.Type,
            isActive = newCate.IsActive,
            parentId = newCate.ParentCategoryProductId,
            userId = newCate.UserId,
        };
    }

    public async Task<BasePaginationResponse> GetCateProducts(int pageIndex, int pageSize, string? keyword)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var cateResponse = await _dbContext.CategoryProducts
            .Where(x => !x.IsDeleted)
            .Select(x => new Response.CategoryProductResponse()
            {
                categoryId = x.Id,
                name = x.Name,
                name_En = x.Name_En,
                description = x.Description,
                description_En = x.Description_En,
                position = x.Position,
                metaData = x.MetaData,
                startTime = x.StartTime,
                endTime = x.EndTime,
                type = x.Type,
                isActive = x.IsActive,
                userId = x.UserId,
                parentId = x.ParentCategoryProductId,
            }).ToListAsync();

        var listCateId = cateResponse.ToDictionary(x => x.categoryId);
        var parentCate = new List<Response.CategoryProductResponse>();

        foreach (var item in cateResponse)
        {
            if (item.parentId == null)
            {
                parentCate.Add(item);
            }
            else
            {
                if (listCateId.TryGetValue(item.parentId.Value, out var parentItem))
                {
                    parentItem.children ??= new List<Response.CategoryProductResponse>();
                    parentItem.children.Add(item);
                }
            }
        }

        if (keyword != null)
        {
            var kw = keyword.Trim().ToLower();
            parentCate = parentCate.Where(x => x.name.Trim().ToLower().Contains(kw)).ToList();
        }

        var count = parentCate.Count;
        var catePageResponse = parentCate
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return ApiResponseFactory.BasePagination(
            items: catePageResponse,
            pageIndex: pageIndex,
            pageSize: pageSize,
            totalCount: count);
    }

    public async Task<Response.CategoryProductResponse> GetCateProduct(Guid categoryId)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        var result = MapCategoryResponse(category);
        if (category.ParentCategoryProductId != null)
        {
            var parentCateId = category.ParentCategoryProductId.Value;
            result.Parent = await _dbContext.CategoryProducts
                .Where(x => x.Id == parentCateId && !x.IsDeleted)
                .Select(x => new Response.CategoryProductResponse()
                {
                    categoryId = x.Id,
                    name = x.Name,
                    name_En = x.Name_En,
                    description = x.Description,
                    description_En = x.Description_En,
                    position = x.Position,
                    metaData = x.MetaData,
                    startTime = x.StartTime,
                    endTime = x.EndTime,
                    type = x.Type,
                    isActive = x.IsActive,
                    userId = x.UserId,
                    parentId = x.ParentCategoryProductId,
                    children = new List<Response.CategoryProductResponse>(),
                }).FirstOrDefaultAsync();
        }

        var productDetails = await _dbContext.CategoryProductDetails
            .Where(x => x.CategoryProductId == categoryId && !x.IsDeleted)
            .Select(x => x.ProductId)
            .ToListAsync();

        result.productCount = productDetails.Count;
        result.productIds = productDetails;

        var allActive = await _dbContext.CategoryProducts
            .Where(x => !x.IsDeleted)
            .Select(c => new Response.CategoryProductResponse()
            {
                categoryId = c.Id,
                name = c.Name,
                name_En = c.Name_En,
                description = c.Description,
                description_En = c.Description_En,
                position = c.Position,
                metaData = c.MetaData,
                startTime = c.StartTime,
                endTime = c.EndTime,
                type = c.Type,
                isActive = c.IsActive,
                parentId = c.ParentCategoryProductId,
                userId = c.UserId,
                children = new List<Response.CategoryProductResponse>()
            })
            .ToListAsync();

        var listCateId = allActive.ToDictionary(x => x.categoryId);

        foreach (var item in allActive)
        {
            if (item.parentId != null && listCateId.TryGetValue(item.parentId.Value, out var parentItem))
            {
                parentItem.children ??= new List<Response.CategoryProductResponse>();
                parentItem.children.Add(item);
            }
        }

        if (listCateId.TryGetValue(categoryId, out var targetResponse))
        {
            result.children = targetResponse.children;
        }

        return result;
    }

    public async Task<Response.CategoryProductResponse> UpdateCateProduct(Guid categoryId, Request.UpdateCategoryProductRequest request)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        if (string.IsNullOrWhiteSpace(request.name))
        {
            throw new BadRequestException("CATEGORY_NAME_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.name_En))
        {
            throw new BadRequestException("CATEGORY_NAME_EN_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.description))
        {
            throw new BadRequestException("CATEGORY_DESCRIPTION_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.description_En))
        {
            throw new BadRequestException("CATEGORY_DESCRIPTION_EN_REQUIRED");
        }

        if (request.parentId == categoryId)
        {
            throw new BadRequestException("CATEGORY_PARENT_INVALID");
        }

        var isDuplicateName = await _dbContext.CategoryProducts.AnyAsync(x =>
            !x.IsDeleted &&
            x.Id != categoryId &&
            x.ParentCategoryProductId == request.parentId &&
            x.Name == request.name);
        if (isDuplicateName)
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

            var currentParentId = request.parentId;
            while (currentParentId != null)
            {
                if (currentParentId == categoryId)
                {
                    throw new BadRequestException("CATEGORY_PARENT_INVALID");
                }
                var p = await GetActiveCategory(currentParentId.Value);
                currentParentId = p?.ParentCategoryProductId;
            }
        }

        category.Name = request.name;
        category.Name_En = request.name_En;
        category.Description = request.description;
        category.Description_En = request.description_En;
        category.Position = request.position;
        category.MetaData = request.metaData;
        category.StartTime = request.startTime;
        category.EndTime = request.endTime;
        category.Type = request.type;
        category.IsActive = request.isActive;
        category.ParentCategoryProductId = request.parentId;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return await GetCateProduct(categoryId);
    }

    public async Task AssignProductsToCategory(Guid categoryId, Request.AssignProductsToCategoryRequest request)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var existingDetails = await _dbContext.CategoryProductDetails
                .Where(x => x.CategoryProductId == categoryId && !x.IsDeleted)
                .ToListAsync();

            var oldProductIds = existingDetails.Select(x => x.ProductId).ToList();
            var newProductIds = request.productIds ?? new List<Guid>();

            var toDelete = existingDetails.Where(x => !newProductIds.Contains(x.ProductId)).ToList();
            var toAddIds = newProductIds.Where(id => !oldProductIds.Contains(id)).ToList();

            if (toDelete.Any())
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var detail in toDelete)
                {
                    detail.IsDeleted = true;
                    detail.UpdatedAt = now;
                }
            }

            if (toAddIds.Any())
            {
                var validAddIds = await _dbContext.Products
                    .Where(p => !p.IsDeleted && toAddIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var newJunctions = validAddIds.Select(productId => new CategoryProductDetail
                {
                    Id = Guid.NewGuid(),
                    CategoryProductId = categoryId,
                    ProductId = productId,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                await _dbContext.CategoryProductDetails.AddRangeAsync(newJunctions);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteCateProduct(Guid categoryId)
    {
        var category = await GetActiveCategory(categoryId);
        if (category == null)
        {
            throw new NotFoundException("CATEGORY_NOT_FOUND");
        }

        var allActive = await _dbContext.CategoryProducts.Where(x => !x.IsDeleted).ToListAsync();
        var categories = new List<CategoryProductEntity> { category };

        void AddDescendants(Guid parentId)
        {
            var children = allActive.Where(x => x.ParentCategoryProductId == parentId).ToList();
            foreach (var child in children)
            {
                categories.Add(child);
                AddDescendants(child.Id);
            }
        }
        AddDescendants(categoryId);

        var now = DateTimeOffset.UtcNow;
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in categories)
            {
                item.IsDeleted = true;
                item.UpdatedAt = now;

                var details = await _dbContext.CategoryProductDetails
                    .Where(x => x.CategoryProductId == item.Id && !x.IsDeleted)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    detail.IsDeleted = true;
                    detail.UpdatedAt = now;
                }
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Response.CategoryWithChildrenResponse>> GetCategoryWithChild()
    {
        var queryCategoryProducts = _dbContext.CategoryProducts.Where(x => !x.IsDeleted && x.IsActive);

        var selectedQueryCate = queryCategoryProducts
            .Select(x => new Response.CategoryWithChildrenResponse()
            {
                CategoryId = x.Id,
                Name = x.Name,
                NameEn = x.Name_En,
                Description = x.Description,
                DescriptionEn = x.Description_En,
                Position = x.Position,
                MetaData = x.MetaData,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Type = x.Type,
                UserId = x.UserId,
                ParentId = x.ParentCategoryProductId,
                Children = new List<Response.CategoryWithChildrenResponse>()
            });

        var listCategoryProducts = await selectedQueryCate.ToListAsync();
        
        var parentCategories = listCategoryProducts
            .Where(x => x.ParentId == null)
            .OrderBy(x => x.Position)
            .ToList();
        
        var childCategories = listCategoryProducts
            .Where(x => x.ParentId != null)
            .OrderBy(x => x.Position)
            .ToList();

        foreach (var parentCategory in parentCategories)
        {
            var children = childCategories
                .Where(x => x.ParentId == parentCategory.CategoryId)
                .OrderBy(x => x.Position)
                .ToList();

            parentCategory.Children = children;
        }

        return parentCategories;
    }
}
