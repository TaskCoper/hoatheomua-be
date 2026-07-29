using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using CategoryProductEntity = HoaTheoMua.Repository.Entity.CategoryProduct;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.CategoryProduct;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using CategoryProductService = HoaTheoMua.Service.CategoryProduct.Service;

namespace HoaTheoMua.Service.Test.CategoryProductTest;

public class CategoryProductTest
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static CategoryProductService CreateService(AppDbContext dbContext)
    {
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(UserId);

        return new CategoryProductService(dbContext, currentUserMock.Object);
    }

    [Fact]
    public async Task CreateCateProduct_ValidRequest_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Sale Mùa Hè",
            name_En = "Summer Sale",
            description = "Mô tả sale mùa hè",
            description_En = "Description summer sale",
            type = CategoryProductType.Campaign,
            startTime = DateTimeOffset.UtcNow,
            endTime = DateTimeOffset.UtcNow.AddDays(7),
            isActive = true
        };

        // Act
        var result = await service.CreateCateProduct(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sale Mùa Hè", result.name);
        Assert.Equal(CategoryProductType.Campaign, result.type);
        Assert.True(result.isActive);

        var dbCategory = await dbContext.CategoryProducts.FirstOrDefaultAsync(x => x.Id == result.categoryId);
        Assert.NotNull(dbCategory);
        Assert.Equal("Sale Mùa Hè", dbCategory.Name);
    }

    [Fact]
    public async Task CreateCateProduct_ParentNestingMoreThanOneLevel_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var root = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Root Category",
            Name_En = "Root Category",
            Description = "Root Category",
            Description_En = "Root Category",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var child = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Child Category",
            Name_En = "Child Category",
            Description = "Child Category",
            Description_En = "Child Category",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = root.Id,
            IsActive = true
        };
        await dbContext.CategoryProducts.AddRangeAsync(root, child);
        await dbContext.SaveChangesAsync();

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Grandchild Category",
            name_En = "Grandchild Category",
            description = "Grandchild Category",
            description_En = "Grandchild Category",
            type = CategoryProductType.Fix,
            parentId = child.Id
        };

        // Act
        var result = await service.CreateCateProduct(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Grandchild Category", result.name);
        Assert.Equal(child.Id, result.parentId);
    }

    [Fact]
    public async Task GetCateProducts_TreeStructureAndKeyword_ReturnsFilteredTree()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent1 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Quần Áo",
            Name_En = "Clothes",
            Description = "Quần áo",
            Description_En = "Clothes",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var child1 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Áo Thun",
            Name_En = "T-Shirt",
            Description = "Áo thun",
            Description_En = "T-Shirt",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = parent1.Id,
            IsActive = true
        };
        var parent2 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Giày Dép",
            Name_En = "Shoes",
            Description = "Giày dép",
            Description_En = "Shoes",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Quần Jeans" };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Áo Polo" };

        await dbContext.CategoryProducts.AddRangeAsync(parent1, child1, parent2);
        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.SaveChangesAsync();

        await service.AssignProductsToCategory(parent1.Id, new Request.AssignProductsToCategoryRequest
        {
            productIds = new List<Guid> { p1.Id }
        });

        await service.AssignProductsToCategory(child1.Id, new Request.AssignProductsToCategoryRequest
        {
            productIds = new List<Guid> { p2.Id }
        });

        // Act
        var result = await service.GetCateProducts(1, 10, "Quần");

        // Assert
        Assert.NotNull(result);
        var items = result.Value.Items.Cast<Response.CategoryProductResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("Quần Áo", items[0].name);
        Assert.Equal(1, items[0].productCount);
        Assert.NotNull(items[0].productIds);
        Assert.Contains(p1.Id, items[0].productIds!);
        Assert.Single(items[0].children);
        Assert.Equal("Áo Thun", items[0].children[0].name);
        Assert.Equal(1, items[0].children[0].productCount);
        Assert.NotNull(items[0].children[0].productIds);
        Assert.Contains(p2.Id, items[0].children[0].productIds!);
    }

    [Fact]
    public async Task UpdateCateProduct_BasicFields_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var original = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Cũ",
            Name_En = "Old",
            Description = "Cũ",
            Description_En = "Old",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        await dbContext.CategoryProducts.AddAsync(original);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Mới",
            name_En = "New",
            description = "Mới",
            description_En = "New",
            type = CategoryProductType.Campaign,
            isActive = false,
            startTime = DateTimeOffset.UtcNow,
            endTime = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act
        var result = await service.UpdateCateProduct(original.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mới", result.name);
        Assert.Equal(CategoryProductType.Campaign, result.type);
        Assert.False(result.isActive);

        var dbCategory = await dbContext.CategoryProducts.FindAsync(original.Id);
        Assert.Equal("Mới", dbCategory.Name);
    }

    [Fact]
    public async Task UpdateCateProduct_InactiveCategory_KeepsProductIdsAndCount()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Name_En = "Electronics",
            Description = "Electronics",
            Description_En = "Electronics",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Phone" };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Laptop" };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.SaveChangesAsync();

        await service.AssignProductsToCategory(category.Id, new Request.AssignProductsToCategoryRequest
        {
            productIds = new List<Guid> { p1.Id, p2.Id }
        });

        var request = new Request.UpdateCategoryProductRequest
        {
            name = category.Name,
            name_En = category.Name_En,
            description = category.Description,
            description_En = category.Description_En,
            type = category.Type,
            isActive = false
        };

        // Act
        var result = await service.UpdateCateProduct(category.Id, request);

        // Assert
        Assert.False(result.isActive);
        Assert.Equal(2, result.productCount);
        Assert.NotNull(result.productIds);
        Assert.Contains(p1.Id, result.productIds!);
        Assert.Contains(p2.Id, result.productIds!);
    }

    [Fact]
    public async Task AssignProductsToCategory_AssignAndRemove_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Name_En = "Electronics",
            Description = "Electronics",
            Description_En = "Electronics",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var p1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Phone" };
        var p2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Laptop" };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.SaveChangesAsync();

        var assignRequest = new Request.AssignProductsToCategoryRequest
        {
            productIds = new List<Guid> { p1.Id, p2.Id }
        };

        // Act - Assign
        await service.AssignProductsToCategory(category.Id, assignRequest);

        // Assert - Assigned
        var junctions = await dbContext.CategoryProductDetails.Where(x => x.CategoryProductId == category.Id && !x.IsDeleted).ToListAsync();
        Assert.Equal(2, junctions.Count);
        Assert.Contains(junctions, x => x.ProductId == p1.Id);

        // Act - Remove p1, keep p2
        var updateRequest = new Request.AssignProductsToCategoryRequest
        {
            productIds = new List<Guid> { p2.Id }
        };
        await service.AssignProductsToCategory(category.Id, updateRequest);

        // Assert - Removed
        var updatedJunctions = await dbContext.CategoryProductDetails.Where(x => x.CategoryProductId == category.Id && !x.IsDeleted).ToListAsync();
        Assert.Single(updatedJunctions);
        Assert.Equal(p2.Id, updatedJunctions[0].ProductId);
    }

    [Fact]
    public async Task DeleteCateProduct_SoftDeletesParentAndChildren_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Parent",
            Name_En = "Parent",
            Description = "Parent",
            Description_En = "Parent",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var child = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Child",
            Name_En = "Child",
            Description = "Child",
            Description_En = "Child",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = parent.Id,
            IsActive = true
        };
        var grandchild = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Grandchild",
            Name_En = "Grandchild",
            Description = "Grandchild",
            Description_En = "Grandchild",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = child.Id,
            IsActive = true
        };
        var product = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Test" };
        var junction = new CategoryProductDetail
        {
            Id = Guid.NewGuid(),
            CategoryProductId = grandchild.Id,
            ProductId = product.Id
        };

        await dbContext.CategoryProducts.AddRangeAsync(parent, child, grandchild);
        await dbContext.Products.AddAsync(product);
        await dbContext.CategoryProductDetails.AddAsync(junction);
        await dbContext.SaveChangesAsync();

        // Act
        await service.DeleteCateProduct(parent.Id);

        // Assert
        var dbParent = await dbContext.CategoryProducts.FindAsync(parent.Id);
        Assert.True(dbParent.IsDeleted);

        var dbChild = await dbContext.CategoryProducts.FindAsync(child.Id);
        Assert.True(dbChild.IsDeleted);

        var dbGrandchild = await dbContext.CategoryProducts.FindAsync(grandchild.Id);
        Assert.True(dbGrandchild.IsDeleted);

        var dbJunction = await dbContext.CategoryProductDetails.FindAsync(junction.Id);
        Assert.True(dbJunction.IsDeleted);
    }

    [Fact]
    public async Task GetCateProduct_RecursiveChildren_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var root = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Root", Name_En = "Root", Description = "Root", Description_En = "Root", Type = CategoryProductType.Fix, IsActive = true };
        var child = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Child", Name_En = "Child", Description = "Child", Description_En = "Child", Type = CategoryProductType.Fix, ParentCategoryProductId = root.Id, IsActive = true };
        var grandchild = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Grandchild", Name_En = "Grandchild", Description = "Grandchild", Description_En = "Grandchild", Type = CategoryProductType.Fix, ParentCategoryProductId = child.Id, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(root, child, grandchild);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetCateProduct(root.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Root", result.name);
        Assert.Single(result.children);

        var childResponse = result.children[0];
        Assert.Equal("Child", childResponse.name);
        Assert.Single(childResponse.children);

        var grandchildResponse = childResponse.children[0];
        Assert.Equal("Grandchild", grandchildResponse.name);
        Assert.Empty(grandchildResponse.children);
    }

    [Fact]
    public async Task CreateCateProduct_EmptyName_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CreateCategoryProductRequest
        {
            name = "   ",
            name_En = "Space",
            description = "Space",
            description_En = "Space",
            type = CategoryProductType.Fix
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateCateProduct(request));
        Assert.Equal("CATEGORY_NAME_REQUIRED", ex.Message);
    }

    [Fact]
    public async Task CreateCateProduct_ParentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Test Category",
            name_En = "Test Category",
            description = "Test Category",
            description_En = "Test Category",
            type = CategoryProductType.Fix,
            parentId = Guid.NewGuid()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.CreateCateProduct(request));
        Assert.Equal("CATEGORY_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task GetCateProduct_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.GetCateProduct(Guid.NewGuid()));
        Assert.Equal("CATEGORY_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Updated Name",
            name_En = "Updated Name",
            description = "Updated Name",
            description_En = "Updated Name",
            type = CategoryProductType.Fix
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateCateProduct(Guid.NewGuid(), request));
        Assert.Equal("CATEGORY_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_EmptyName_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Name_En = "Original", Description = "Original", Description_En = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "",
            name_En = "Empty",
            description = "Empty",
            description_En = "Empty",
            type = CategoryProductType.Fix
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(category.Id, request));
        Assert.Equal("CATEGORY_NAME_REQUIRED", ex.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_SelfParent_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Name_En = "Original", Description = "Original", Description_En = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Original",
            name_En = "Original",
            description = "Original",
            description_En = "Original",
            type = CategoryProductType.Fix,
            parentId = category.Id
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(category.Id, request));
        Assert.Equal("CATEGORY_PARENT_INVALID", ex.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var cat1 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Cat 1", Name_En = "Cat 1", Description = "Cat 1", Description_En = "Cat 1", Type = CategoryProductType.Fix, IsActive = true };
        var cat2 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Cat 2", Name_En = "Cat 2", Description = "Cat 2", Description_En = "Cat 2", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddRangeAsync(cat1, cat2);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Cat 2",
            name_En = "Cat 2",
            description = "Cat 2",
            description_En = "Cat 2",
            type = CategoryProductType.Fix
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.UpdateCateProduct(cat1.Id, request));
        Assert.Equal("CATEGORY_ALREADY_EXISTS", ex.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_ParentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Name_En = "Original", Description = "Original", Description_En = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Original",
            name_En = "Original",
            description = "Original",
            description_En = "Original",
            type = CategoryProductType.Fix,
            parentId = Guid.NewGuid()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateCateProduct(category.Id, request));
        Assert.Equal("PARENT_CATEGORY_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_ParentHasParent_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var root = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Root", Name_En = "Root", Description = "Root", Description_En = "Root", Type = CategoryProductType.Fix, IsActive = true };
        var parent = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Parent", Name_En = "Parent", Description = "Parent", Description_En = "Parent", Type = CategoryProductType.Fix, ParentCategoryProductId = root.Id, IsActive = true };
        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Category", Name_En = "Category", Description = "Category", Description_En = "Category", Type = CategoryProductType.Fix, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(root, parent, category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Category Updated",
            name_En = "Category Updated",
            description = "Category Updated",
            description_En = "Category Updated",
            type = CategoryProductType.Fix,
            parentId = parent.Id
        };

        // Act
        var result = await service.UpdateCateProduct(category.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Category Updated", result.name);
        Assert.Equal(parent.Id, result.parentId);
    }

    [Fact]
    public async Task UpdateCateProduct_HasChildrenAndSetsParent_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Parent", Name_En = "Parent", Description = "Parent", Description_En = "Parent", Type = CategoryProductType.Fix, IsActive = true };
        var child = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Child", Name_En = "Child", Description = "Child", Description_En = "Child", Type = CategoryProductType.Fix, ParentCategoryProductId = parent.Id, IsActive = true };
        var newParent = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "New Parent", Name_En = "New Parent", Description = "New Parent", Description_En = "New Parent", Type = CategoryProductType.Fix, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(parent, child, newParent);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Parent Updated",
            name_En = "Parent Updated",
            description = "Parent Updated",
            description_En = "Parent Updated",
            type = CategoryProductType.Fix,
            parentId = newParent.Id
        };

        // Act
        var result = await service.UpdateCateProduct(parent.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Parent Updated", result.name);
        Assert.Equal(newParent.Id, result.parentId);
    }

    [Fact]
    public async Task UpdateCateProduct_CycleDetected_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var root = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Root", Name_En = "Root", Description = "Root", Description_En = "Root", Type = CategoryProductType.Fix, IsActive = true };
        var child = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Child", Name_En = "Child", Description = "Child", Description_En = "Child", Type = CategoryProductType.Fix, ParentCategoryProductId = root.Id, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(root, child);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Root Updated",
            name_En = "Root Updated",
            description = "Root Updated",
            description_En = "Root Updated",
            type = CategoryProductType.Fix,
            parentId = child.Id // This creates a cycle: Root -> Child -> Root
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(root.Id, request));
        Assert.Equal("CATEGORY_PARENT_INVALID", ex.Message);
    }

    [Fact]
    public async Task AssignProductsToCategory_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.AssignProductsToCategoryRequest { productIds = new List<Guid>() };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.AssignProductsToCategory(Guid.NewGuid(), request));
        Assert.Equal("CATEGORY_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task DeleteCateProduct_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteCateProduct(Guid.NewGuid()));
        Assert.Equal("CATEGORY_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task CreateAndGetCategoryProduct_MultilingualFields_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Hoa Hồng",
            name_En = "Rose",
            description = "Mô tả tiếng Việt",
            description_En = "Description in English",
            metaData = "{\"key\": \"val\"}",
            type = CategoryProductType.Fix,
            isActive = true
        };

        // Act
        var createResult = await service.CreateCateProduct(request);

        // Assert
        Assert.NotNull(createResult);
        Assert.Equal("Rose", createResult.name_En);
        Assert.Equal("Mô tả tiếng Việt", createResult.description);
        Assert.Equal("Description in English", createResult.description_En);
        Assert.Equal("{\"key\": \"val\"}", createResult.metaData);

        // Act - Get
        var getResult = await service.GetCateProduct(createResult.categoryId);

        // Assert - Get
        Assert.NotNull(getResult);
        Assert.Equal("Rose", getResult.name_En);
        Assert.Equal("Mô tả tiếng Việt", getResult.description);
        Assert.Equal("Description in English", getResult.description_En);
        Assert.Equal("{\"key\": \"val\"}", getResult.metaData);

        // Act - Update
        var updateRequest = new Request.UpdateCategoryProductRequest
        {
            name = "Hoa Cúc",
            name_En = "Daisy",
            description = "Mô tả cúc",
            description_En = "Desc Daisy",
            position = -5.5,
            metaData = "{\"new\": \"val\"}",
            type = CategoryProductType.Fix,
            isActive = true
        };

        var updateResult = await service.UpdateCateProduct(createResult.categoryId, updateRequest);

        // Assert - Update
        Assert.NotNull(updateResult);
        Assert.Equal("Daisy", updateResult.name_En);
        Assert.Equal("Mô tả cúc", updateResult.description);
        Assert.Equal("Desc Daisy", updateResult.description_En);
        Assert.Equal(-5.5, updateResult.position);
        Assert.Equal("{\"new\": \"val\"}", updateResult.metaData);
    }

    [Fact]
    public async Task CreateCateProduct_MissingRequiredFields_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var req1 = new Request.CreateCategoryProductRequest { name = "", name_En = "En", description = "Desc", description_En = "DescEn", type = CategoryProductType.Fix };
        var req2 = new Request.CreateCategoryProductRequest { name = "Name", name_En = "", description = "Desc", description_En = "DescEn", type = CategoryProductType.Fix };

        // Act & Assert
        var ex1 = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateCateProduct(req1));
        Assert.Equal("CATEGORY_NAME_REQUIRED", ex1.Message);

        var ex2 = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateCateProduct(req2));
        Assert.Equal("CATEGORY_NAME_EN_REQUIRED", ex2.Message);
    }

    [Fact]
    public async Task UpdateCateProduct_MissingRequiredFields_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Name_En = "Original", Description = "Original", Description_En = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var req1 = new Request.UpdateCategoryProductRequest { name = "", name_En = "En", description = "Desc", description_En = "DescEn", type = CategoryProductType.Fix };
        var req2 = new Request.UpdateCategoryProductRequest { name = "Name", name_En = "", description = "Desc", description_En = "DescEn", type = CategoryProductType.Fix };

        // Act & Assert
        var ex1 = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(category.Id, req1));
        Assert.Equal("CATEGORY_NAME_REQUIRED", ex1.Message);

        var ex2 = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(category.Id, req2));
        Assert.Equal("CATEGORY_NAME_EN_REQUIRED", ex2.Message);
    }

    [Fact]
    public async Task CreateAndUpdateCategoryProduct_NullDescriptions_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Hoa Hồng",
            name_En = "Rose",
            description = null,
            description_En = null,
            type = CategoryProductType.Fix,
            isActive = true
        };

        // Act - Create
        var createResult = await service.CreateCateProduct(request);

        // Assert - Create
        Assert.NotNull(createResult);
        Assert.Null(createResult.description);
        Assert.Null(createResult.description_En);

        // Act - Update
        var updateRequest = new Request.UpdateCategoryProductRequest
        {
            name = "Hoa Hồng Cập Nhật",
            name_En = "Rose Updated",
            description = null,
            description_En = null,
            type = CategoryProductType.Fix,
            isActive = true
        };
        var updateResult = await service.UpdateCateProduct(createResult.categoryId, updateRequest);

        // Assert - Update
        Assert.NotNull(updateResult);
        Assert.Null(updateResult.description);
        Assert.Null(updateResult.description_En);
    }

    [Fact]
    public async Task CreateAndUpdateCategoryProduct_MutuallyRequiredDescription_ThrowsBadRequestException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var req1 = new Request.CreateCategoryProductRequest
        {
            name = "Hoa Hồng",
            name_En = "Rose",
            description = "Vietnamese description only",
            description_En = null,
            type = CategoryProductType.Fix,
            isActive = true
        };

        var req2 = new Request.CreateCategoryProductRequest
        {
            name = "Hoa Hồng",
            name_En = "Rose",
            description = null,
            description_En = "English description only",
            type = CategoryProductType.Fix,
            isActive = true
        };

        // Act & Assert - Create (Only description)
        var ex1 = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateCateProduct(req1));
        Assert.Equal("CATEGORY_DESCRIPTION_MUTUALLY_REQUIRED", ex1.Message);

        // Act & Assert - Create (Only description_En)
        var ex2 = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateCateProduct(req2));
        Assert.Equal("CATEGORY_DESCRIPTION_MUTUALLY_REQUIRED", ex2.Message);

        // Set up an existing category for update tests
        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            Name_En = "Original",
            Description = "Original",
            Description_En = "Original",
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var reqUpdate1 = new Request.UpdateCategoryProductRequest
        {
            name = "Original Updated",
            name_En = "Original Updated",
            description = "Vietnamese description only",
            description_En = null,
            type = CategoryProductType.Fix,
            isActive = true
        };

        var reqUpdate2 = new Request.UpdateCategoryProductRequest
        {
            name = "Original Updated",
            name_En = "Original Updated",
            description = null,
            description_En = "English description only",
            type = CategoryProductType.Fix,
            isActive = true
        };

        // Act & Assert - Update (Only description)
        var ex3 = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(category.Id, reqUpdate1));
        Assert.Equal("CATEGORY_DESCRIPTION_MUTUALLY_REQUIRED", ex3.Message);

        // Act & Assert - Update (Only description_En)
        var ex4 = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateCateProduct(category.Id, reqUpdate2));
        Assert.Equal("CATEGORY_DESCRIPTION_MUTUALLY_REQUIRED", ex4.Message);
    }
}
