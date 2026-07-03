using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;

using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryService = HoaTheoMua.Service.CategoryNew.Service;

namespace HoaTheoMua.Service.Test.CategoryNewTest;

public class DeleteCateTest
{
    private AppDbContext CreateMockDbContext(List<CategoryNewsEntity> categories)
    {
        var mockDbSet = categories.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.CategoryNewsList)
            .Returns(mockDbSet.Object);

        mockDbContext
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return mockDbContext.Object;
    }

    private CategoryService CreateService(AppDbContext dbContext)
    {
        var currentUserMock = new Mock<ICurrentUserService>();
        return new CategoryService(dbContext, currentUserMock.Object);
    }

    private CategoryNewsEntity CreateCategory(
        Guid id,
        string title,
        Guid userId,
        Guid? parentId = null,
        bool isDeleted = false,
        DateTimeOffset? updatedAt = null)
    {
        return new CategoryNewsEntity
        {
            Id = id,
            Title = title,
            UserId = userId,
            ParentCategoryNewsId = parentId,
            IsDeleted = isDeleted,
            UpdatedAt = updatedAt
        };
    }

    // TC01: Xóa danh mục cha, cascade children active
    [Fact]
    public async Task Delete_RootCategoryWithChildren_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId1 = Guid.NewGuid();
        var childId2 = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Parent", userId, parentId: null, isDeleted: false),
            CreateCategory(childId1, "Child 1", userId, parentId: parentId, isDeleted: false),
            CreateCategory(childId2, "Child 2", userId, parentId: parentId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        await service.DeleteCate(parentId);

        // Assert
        Assert.True(categories.First(x => x.Id == parentId).IsDeleted);
        Assert.True(categories.First(x => x.Id == childId1).IsDeleted);
        Assert.True(categories.First(x => x.Id == childId2).IsDeleted);

        Assert.NotNull(categories.First(x => x.Id == parentId).UpdatedAt);
        Assert.NotNull(categories.First(x => x.Id == childId1).UpdatedAt);
        Assert.NotNull(categories.First(x => x.Id == childId2).UpdatedAt);
    }

    // TC02: Xóa danh mục con, chỉ soft delete chính nó
    [Fact]
    public async Task Delete_ChildCategory_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Parent", userId, parentId: null, isDeleted: false),
            CreateCategory(childId, "Child", userId, parentId: parentId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        await service.DeleteCate(childId);

        // Assert
        var parent = categories.First(x => x.Id == parentId);
        var child = categories.First(x => x.Id == childId);

        Assert.False(parent.IsDeleted);
        Assert.True(child.IsDeleted);
        Assert.Null(parent.UpdatedAt);
        Assert.NotNull(child.UpdatedAt);
    }

    // TC03: Category không tồn tại
    [Fact]
    public async Task Delete_NotFound_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Category", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteCate(Guid.NewGuid())
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC05: id là Guid.Empty
    // Theo service hiện tại: Guid.Empty sẽ throw NotFoundException, không phải BadRequestException
    [Fact]
    public async Task Delete_EmptyId_ThrowNotFound()
    {
        // Arrange
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteCate(Guid.Empty)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC06: Category đã bị soft delete trước đó
    [Fact]
    public async Task Delete_AlreadyDeleted_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Deleted Category", userId, parentId: null, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteCate(categoryId)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC07: Parent có children nhưng tất cả children đã soft delete
    [Fact]
    public async Task Delete_RootCategoryWithDeletedChildren_DeleteOnlyParent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var deletedChildId1 = Guid.NewGuid();
        var deletedChildId2 = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Parent", userId, parentId: null, isDeleted: false),
            CreateCategory(deletedChildId1, "Deleted Child 1", userId, parentId: parentId, isDeleted: true),
            CreateCategory(deletedChildId2, "Deleted Child 2", userId, parentId: parentId, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        await service.DeleteCate(parentId);

        // Assert
        var parent = categories.First(x => x.Id == parentId);
        var deletedChild1 = categories.First(x => x.Id == deletedChildId1);
        var deletedChild2 = categories.First(x => x.Id == deletedChildId2);

        Assert.True(parent.IsDeleted);
        Assert.True(deletedChild1.IsDeleted);
        Assert.True(deletedChild2.IsDeleted);

        Assert.NotNull(parent.UpdatedAt);

        // Children đã bị xóa trước đó không được query lại vì service chỉ lấy children !IsDeleted
        Assert.Null(deletedChild1.UpdatedAt);
        Assert.Null(deletedChild2.UpdatedAt);
    }
}