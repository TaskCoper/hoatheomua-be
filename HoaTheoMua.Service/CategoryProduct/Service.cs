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
            page = category.Page ?? new List<string>(),
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

        var hasDesc = !string.IsNullOrWhiteSpace(request.description);
        var hasDescEn = !string.IsNullOrWhiteSpace(request.description_En);
        if (hasDesc != hasDescEn)
        {
            throw new BadRequestException("CATEGORY_DESCRIPTION_MUTUALLY_REQUIRED");
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
            Page = request.page,
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
            page = newCate.Page,
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
                page = x.Page,
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
        if (category.ParentCategoryProductId.HasValue)
        {
            var parentCategoryId = category.ParentCategoryProductId.Value;

            result.Parent = await _dbContext.CategoryProducts
                .AsNoTracking()
                .Where(x =>
                    x.Id == parentCategoryId &&
                    !x.IsDeleted)
                .Select(x => new Response.CategoryProductResponse
                {
                    categoryId = x.Id,
                    name = x.Name,
                    name_En = x.Name_En,
                    description = x.Description,
                    description_En = x.Description_En,
                    position = x.Position,
                    metaData = x.MetaData,
                    page = x.Page,
                    startTime = x.StartTime,
                    endTime = x.EndTime,
                    type = x.Type,
                    isActive = x.IsActive,
                    userId = x.UserId,
                    parentId = x.ParentCategoryProductId,
                    children = new List<Response.CategoryProductResponse>(),
                    productIds = new List<Guid>(),
                    productCount = 0
                })
                .FirstOrDefaultAsync();
        }
        
        var allCategories = await _dbContext.CategoryProducts
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive)
            .Select(x => new Response.CategoryProductResponse
            {
                categoryId = x.Id,
                name = x.Name,
                name_En = x.Name_En,
                description = x.Description,
                description_En = x.Description_En,
                position = x.Position,
                metaData = x.MetaData,
                page = x.Page,
                startTime = x.StartTime,
                endTime = x.EndTime,
                type = x.Type,
                isActive = x.IsActive,
                userId = x.UserId,
                parentId = x.ParentCategoryProductId,
                children = new List<Response.CategoryProductResponse>(),
                productIds = new List<Guid>(),
                productCount = 0
            })
            .ToListAsync();

        var categoryIds = allCategories
            .Select(x => x.categoryId)
            .ToList();

        var activeProductIds = _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => x.Id);
        
        var categoryProductDetails =
            await _dbContext.CategoryProductDetails
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    categoryIds.Contains(x.CategoryProductId) &&
                    activeProductIds.Contains(x.ProductId))
                .Select(x => new
                {
                    x.CategoryProductId,
                    x.ProductId
                })
                .ToListAsync();
        
        foreach (var categoryResponse in allCategories)
        {
            var productIds = categoryProductDetails
                .Where(x =>
                    x.CategoryProductId ==
                    categoryResponse.categoryId)
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            categoryResponse.productIds = productIds;
            categoryResponse.productCount = productIds.Count;
        }
        
        List<Response.CategoryProductResponse> GetChildren(
            Guid parentCategoryId)
        {
            var children = allCategories
                .Where(x =>
                    x.parentId == parentCategoryId)
                .OrderBy(x => x.position)
                .ToList();

            foreach (var child in children)
            {
                child.children = GetChildren(child.categoryId);
            }

            return children;
        }
        
        var currentCategoryResponse = allCategories
            .FirstOrDefault(x => x.categoryId == categoryId);

        if (currentCategoryResponse != null)
        {
            result.productIds =
                currentCategoryResponse.productIds;

            result.productCount =
                currentCategoryResponse.productCount;
        }
        else
        {
            result.productIds = new List<Guid>();
            result.productCount = 0;
        }
        
        result.children = GetChildren(categoryId);

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

        var hasDesc = !string.IsNullOrWhiteSpace(request.description);
        var hasDescEn = !string.IsNullOrWhiteSpace(request.description_En);
        if (hasDesc != hasDescEn)
        {
            throw new BadRequestException("CATEGORY_DESCRIPTION_MUTUALLY_REQUIRED");
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
        category.Page = request.page;
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

    public async Task<List<Response.CategoryWithChildrenResponse>> GetCategoryWithChild(Request.GetCategoryProductWithChild request)
    {
        if (request == null)
        {
            throw new BadRequestException("Invalid request data.");
        }

        var pages = (request.Page ?? new List<string>())
            .SelectMany(page => page.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(page => !string.IsNullOrWhiteSpace(page))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoryProducts = await _dbContext.CategoryProducts
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive)
            .Select(x => new Response.CategoryWithChildrenResponse
            {
                CategoryId = x.Id,
                Name = x.Name,
                NameEn = x.Name_En,
                Description = x.Description,
                DescriptionEn = x.Description_En,
                Position = x.Position,
                MetaData = x.MetaData,
                Page = x.Page,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Type = x.Type,
                UserId = x.UserId,
                ParentId = x.ParentCategoryProductId,
                Children = new List<Response.CategoryWithChildrenResponse>()
            })
            .ToListAsync();

        if (pages.Count > 0)
        {
            var categoryById = categoryProducts.ToDictionary(category => category.CategoryId);
            var includedCategoryIds = new HashSet<Guid>();

            foreach (var category in categoryProducts)
            {
                var isPageMatched = category.Page != null &&
                                    category.Page.Any(page =>
                                        !string.IsNullOrWhiteSpace(page) &&
                                        pages.Contains(page.Trim()));

                if (!isPageMatched)
                {
                    continue;
                }

                var currentCategory = category;
                while (includedCategoryIds.Add(currentCategory.CategoryId) &&
                       currentCategory.ParentId != null &&
                       categoryById.TryGetValue(currentCategory.ParentId.Value, out var parentCategory))
                {
                    currentCategory = parentCategory;
                }
            }

            categoryProducts = categoryProducts
                .Where(category => includedCategoryIds.Contains(category.CategoryId))
                .ToList();
        }

        List<Response.CategoryWithChildrenResponse> ProductCategoryList(Guid? parentId)
        {
            var currentCategories = categoryProducts
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Position)
                .ToList();

            foreach (var category in currentCategories)
            {
                category.Children = ProductCategoryList(category.CategoryId);
            }

            return currentCategories;
        }

        var result = ProductCategoryList(null);

        return result;
    }

    public async Task<BaseResponse> UpdateCategoryProductDetail(Request.CategoryProductDetailRequest request)
    {
        if (request == null)
        {
            throw new BadRequestException("Invalid category product detail data.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new BadRequestException("Product ID is required.");
        }

        if (request.CategoryProductId == Guid.Empty)
        {
            throw new BadRequestException("Category ID is required.");
        }

        var productExists = await _dbContext.Products
            .AnyAsync(x =>
                x.Id == request.ProductId &&
                !x.IsDeleted);

        if (!productExists)
        {
            throw new NotFoundException("Product not found.");
        }

        var categoryExists = await _dbContext.CategoryProducts
            .AnyAsync(x =>
                x.Id == request.CategoryProductId &&
                !x.IsDeleted);

        if (!categoryExists)
        {
            throw new NotFoundException("Category not found.");
        }

        var oldCategoryProductDetail =
            await _dbContext.CategoryProductDetails
                .FirstOrDefaultAsync(x =>
                    x.ProductId == request.ProductId &&
                    x.CategoryProductId == request.CategoryProductId &&
                    !x.IsDeleted);

        if (oldCategoryProductDetail == null)
        {
            throw new NotFoundException("Category product detail not found.");
        }

        if (request.IsPin && !oldCategoryProductDetail.IsPin)
        {
            var pinnedProductCount =
                await _dbContext.CategoryProductDetails
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.IsPin &&
                        x.CategoryProductId == request.CategoryProductId &&
                        x.Id != oldCategoryProductDetail.Id);

            if (pinnedProductCount >= 32)
            {
                throw new BadRequestException("Only 32 products can be pinned in this category.");
            }
        }

        var currentTime = DateTimeOffset.UtcNow;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            oldCategoryProductDetail.IsDeleted = true;
            oldCategoryProductDetail.UpdatedAt = currentTime;

            await _dbContext.SaveChangesAsync();

            var newCategoryProductDetail = new CategoryProductDetail()
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductId,
                    CategoryProductId = request.CategoryProductId,
                    IsPin = request.IsPin,
                    Position = request.Position,
                    IsDeleted = false,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime
                };

            await _dbContext.CategoryProductDetails
                .AddAsync(newCategoryProductDetail);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return ApiResponseFactory.Base("Category product detail updated successfully.");
    }
}
