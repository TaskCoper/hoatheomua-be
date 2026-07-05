using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;

using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryNewsDetailEntity = HoaTheoMua.Repository.Entity.CategoryNewsDetail;
using CategoryService = HoaTheoMua.Service.CategoryNew.Service;

namespace HoaTheoMua.Service.Test.CategoryNewTest;

public class GetCategoryTest
{
    private AppDbContext CreateMockDbContext(
        List<CategoryNewsEntity> categories,
        List<CategoryNewsDetailEntity> categoryNewsDetails)
    {
        var mockCategoryDbSet = categories.BuildMockDbSet();
        var mockCategoryNewsDetailDbSet = categoryNewsDetails.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.CategoryNewsList)
            .Returns(mockCategoryDbSet.Object);

        mockDbContext
            .Setup(x => x.CategoryNewsDetails)
            .Returns(mockCategoryNewsDetailDbSet.Object);

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
        bool isDeleted = false)
    {
        return new CategoryNewsEntity
        {
            Id = id,
            Title = title,
            UserId = userId,
            ParentCategoryNewsId = parentId,
            IsDeleted = isDeleted
        };
    }

    private CategoryNewsDetailEntity CreateCategoryNewsDetail(
        Guid id,
        Guid categoryId,
        bool isDeleted = false)
    {
        return new CategoryNewsDetailEntity
        {
            Id = id,
            CategoryNewsId = categoryId,
            IsDeleted = isDeleted
        };
    }

    // TC01: Lấy danh mục theo ID thành công
    [Fact]
    public async Task GetCategory_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(childId, "Software Development", userId, parentId: parentId, isDeleted: false)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            CreateCategoryNewsDetail(Guid.NewGuid(), parentId, isDeleted: false),
            CreateCategoryNewsDetail(Guid.NewGuid(), parentId, isDeleted: false),
            CreateCategoryNewsDetail(Guid.NewGuid(), parentId, isDeleted: false),
            CreateCategoryNewsDetail(Guid.NewGuid(), parentId, isDeleted: false),
            CreateCategoryNewsDetail(Guid.NewGuid(), parentId, isDeleted: false),
            CreateCategoryNewsDetail(Guid.NewGuid(), parentId, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategory(parentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentId, result.categoryId);
        Assert.Equal("Technology", result.title);
        Assert.Equal(userId, result.userId);
        Assert.Null(result.parentId);
        Assert.Null(result.Parent);

        Assert.NotNull(result.children);
        Assert.Single(result.children);
        Assert.Equal(childId, result.children.First().categoryId);
        Assert.Equal("Software Development", result.children.First().title);

        Assert.Equal(5, result.count);
    }

    // TC02: Danh mục là con thì trả về thêm thông tin parent
    [Fact]
    public async Task GetCategory_ChildCategory_ReturnParent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(childId, "Software Development", userId, parentId: parentId, isDeleted: false)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategory(childId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(childId, result.categoryId);
        Assert.Equal("Software Development", result.title);
        Assert.Equal(parentId, result.parentId);

        Assert.NotNull(result.Parent);
        Assert.Equal(parentId, result.Parent!.categoryId);
        Assert.Equal("Technology", result.Parent.title);
        Assert.Null(result.Parent.parentId);

        Assert.NotNull(result.children);
        Assert.Empty(result.children);
    }

    // TC03: Category không tồn tại
    [Fact]
    public async Task GetCategory_NotFound_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetCategory(Guid.NewGuid())
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC03: Category đã bị soft delete
    [Fact]
    public async Task GetCategory_DeletedCategory_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Deleted Category", userId, parentId: null, isDeleted: true)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetCategory(categoryId)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC04: id là Guid.Empty
    // Theo service hiện tại: Guid.Empty sẽ throw NotFoundException, không phải BadRequestException
    [Fact]
    public async Task GetCategory_EmptyId_ThrowNotFound()
    {
        // Arrange
        var categories = new List<CategoryNewsEntity>();
        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetCategory(Guid.Empty)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC05: Category không có children
    [Fact]
    public async Task GetCategory_NoChildren_ReturnEmptyChildren()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Technology", userId, parentId: null, isDeleted: false)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>
        {
            CreateCategoryNewsDetail(Guid.NewGuid(), categoryId, isDeleted: false),
            CreateCategoryNewsDetail(Guid.NewGuid(), categoryId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategory(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.categoryId);
        Assert.Equal("Technology", result.title);

        Assert.NotNull(result.children);
        Assert.Empty(result.children);

        Assert.Equal(2, result.count);
    }

    // TC06: Category không có news nào
    [Fact]
    public async Task GetCategory_NoNews_ReturnCountZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Technology", userId, parentId: null, isDeleted: false)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategory(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.categoryId);
        Assert.Equal(0, result.count);
    }

    // TC07: Category có children nhưng children đã bị soft delete
    [Fact]
    public async Task GetCategory_ChildrenDeleted_ReturnEmptyChildren()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var deletedChildId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(deletedChildId, "Deleted Child", userId, parentId: parentId, isDeleted: true)
        };

        var categoryNewsDetails = new List<CategoryNewsDetailEntity>();

        var dbContext = CreateMockDbContext(categories, categoryNewsDetails);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategory(parentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentId, result.categoryId);

        Assert.NotNull(result.children);
        Assert.Empty(result.children);
    }
}