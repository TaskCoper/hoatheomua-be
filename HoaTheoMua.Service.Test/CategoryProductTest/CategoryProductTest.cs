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
    public async Task CreateCateProduct_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var existing = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Mùa Đông",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        await dbContext.CategoryProducts.AddAsync(existing);
        await dbContext.SaveChangesAsync();

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Mùa Đông",
            type = CategoryProductType.Fix
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateCateProduct(request));
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
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var child = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Child Category",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = root.Id,
            IsActive = true
        };
        await dbContext.CategoryProducts.AddRangeAsync(root, child);
        await dbContext.SaveChangesAsync();

        var request = new Request.CreateCategoryProductRequest
        {
            name = "Grandchild Category",
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
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var child1 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Áo Thun",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = parent1.Id,
            IsActive = true
        };
        var parent2 = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Giày Dép",
            Type = CategoryProductType.Fix,
            IsActive = true
        };

        await dbContext.CategoryProducts.AddRangeAsync(parent1, child1, parent2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetCateProducts(1, 10, "Quần");

        // Assert
        Assert.NotNull(result);
        var items = result.Value.Items.Cast<Response.CategoryProductResponse>().ToList();
        Assert.Single(items);
        Assert.Equal("Quần Áo", items[0].name);
        Assert.Single(items[0].children);
        Assert.Equal("Áo Thun", items[0].children[0].name);
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
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        await dbContext.CategoryProducts.AddAsync(original);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Mới",
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
    public async Task AssignProductsToCategory_AssignAndRemove_Success()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
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
            Type = CategoryProductType.Fix,
            IsActive = true
        };
        var child = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Child",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = parent.Id,
            IsActive = true
        };
        var grandchild = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Grandchild",
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

        var root = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Root", Type = CategoryProductType.Fix, IsActive = true };
        var child = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Child", Type = CategoryProductType.Fix, ParentCategoryProductId = root.Id, IsActive = true };
        var grandchild = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Grandchild", Type = CategoryProductType.Fix, ParentCategoryProductId = child.Id, IsActive = true };

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

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "",
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

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Original",
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

        var cat1 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Cat 1", Type = CategoryProductType.Fix, IsActive = true };
        var cat2 = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Cat 2", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddRangeAsync(cat1, cat2);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Cat 2",
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

        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Original", Type = CategoryProductType.Fix, IsActive = true };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Original",
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

        var root = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Root", Type = CategoryProductType.Fix, IsActive = true };
        var parent = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Parent", Type = CategoryProductType.Fix, ParentCategoryProductId = root.Id, IsActive = true };
        var category = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Category", Type = CategoryProductType.Fix, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(root, parent, category);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Category Updated",
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

        var parent = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Parent", Type = CategoryProductType.Fix, IsActive = true };
        var child = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Child", Type = CategoryProductType.Fix, ParentCategoryProductId = parent.Id, IsActive = true };
        var newParent = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "New Parent", Type = CategoryProductType.Fix, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(parent, child, newParent);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Parent Updated",
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

        var root = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Root", Type = CategoryProductType.Fix, IsActive = true };
        var child = new CategoryProductEntity { Id = Guid.NewGuid(), Name = "Child", Type = CategoryProductType.Fix, ParentCategoryProductId = root.Id, IsActive = true };

        await dbContext.CategoryProducts.AddRangeAsync(root, child);
        await dbContext.SaveChangesAsync();

        var request = new Request.UpdateCategoryProductRequest
        {
            name = "Root Updated",
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
}
