using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.CategoryNew;
using HoaTheoMua.Service.Exceptions;

using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryService = HoaTheoMua.Service.CategoryNew.Service;

namespace HoaTheoMua.Service.Test.CategoryNewTest;

public class CreateCateTest
{
    private AppDbContext CreateMockDbContext(List<CategoryNewsEntity> categories)
    {
        var mockDbSet = categories.BuildMockDbSet();

        mockDbSet
            .Setup(x => x.AddAsync(
                It.IsAny<CategoryNewsEntity>(),
                It.IsAny<CancellationToken>()))
            .Callback((CategoryNewsEntity entity, CancellationToken _) =>
            {
                categories.Add(entity);
            })
            .Returns((CategoryNewsEntity _, CancellationToken _) =>
                new ValueTask<EntityEntry<CategoryNewsEntity>>(
                    (EntityEntry<CategoryNewsEntity>)null!
                )
            );

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

    private CategoryService CreateService(AppDbContext dbContext, Guid userId)
    {
        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        return new CategoryService(dbContext, currentUserMock.Object);
    }

    private Request.CreateCategoriesRequest CreateRequest(
        string title = "Cong nghe",
        Guid? parentId = null)
    {
        return new Request.CreateCategoriesRequest
        {
            title = title,
            parentId = parentId
        };
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

    // TC01: Tạo danh mục cha thành công
    [Fact]
    public async Task Create_RootCategory_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "Cong nghe", parentId: null);

        // Act
        var result = await service.CreateCate(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.categoryId);
        Assert.Equal("Cong nghe", result.title);
        Assert.Equal(userId, result.userId);
        Assert.Null(result.parentId);

        Assert.Single(categories);
        Assert.Equal("Cong nghe", categories[0].Title);
        Assert.Equal(userId, categories[0].UserId);
        Assert.Null(categories[0].ParentCategoryNewsId);
    }

    // TC02: Tạo danh mục con thành công
    [Fact]
    public async Task Create_ChildCategory_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(
                id: parentId,
                title: "Cong nghe",
                userId: userId,
                parentId: null,
                isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "AI", parentId: parentId);

        // Act
        var result = await service.CreateCate(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AI", result.title);
        Assert.Equal(userId, result.userId);
        Assert.Equal(parentId, result.parentId);

        Assert.Equal(2, categories.Count);
        Assert.Equal(parentId, categories.Last().ParentCategoryNewsId);
    }

    // TC03: Title bị trùng trong cùng parent
    [Fact]
    public async Task Create_DuplicateTitleSameParent_ThrowConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(
                id: Guid.NewGuid(),
                title: "Cong nghe",
                userId: userId,
                parentId: null,
                isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "Cong nghe", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateCate(request)
        );

        // Assert
        Assert.Equal("CATEGORY_ALREADY_EXISTS", exception.Message);
        Assert.Single(categories);
    }

    // TC04: Title rỗng
    [Fact]
    public async Task Create_EmptyTitle_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateCate(request)
        );

        // Assert
        Assert.Equal("CATEGORY_TITLE_REQUIRED", exception.Message);
        Assert.Empty(categories);
    }

    // TC05: Parent không hợp lệ vì parent là danh mục con
    [Fact]
    public async Task Create_ParentIsChild_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(rootId, "Root", userId, parentId: null),
            CreateCategory(childId, "Child", userId, parentId: rootId)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "Sub Child", parentId: childId);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateCate(request)
        );

        // Assert
        Assert.Equal("CATEGORY_PARENT_INVALID", exception.Message);
        Assert.Equal(2, categories.Count);
    }

    // TC06: Parent không tồn tại
    [Fact]
    public async Task Create_ParentNotFound_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "AI", parentId: Guid.NewGuid());

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateCate(request)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
        Assert.Empty(categories);
    }

    // TC07: Title đã tồn tại nhưng ở parent khác
    [Fact]
    public async Task Create_DuplicateTitleDifferentParent_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId1 = Guid.NewGuid();
        var parentId2 = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId1, "Parent 1", userId, parentId: null),
            CreateCategory(parentId2, "Parent 2", userId, parentId: null),
            CreateCategory(Guid.NewGuid(), "AI", userId, parentId: parentId1)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "AI", parentId: parentId2);

        // Act
        var result = await service.CreateCate(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AI", result.title);
        Assert.Equal(parentId2, result.parentId);

        Assert.Equal(4, categories.Count);
    }

    // TC08: Title đã tồn tại cùng parent nhưng đã soft delete
    [Fact]
    public async Task Create_DuplicateTitleButDeleted_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(
                id: Guid.NewGuid(),
                title: "Cong nghe",
                userId: userId,
                parentId: null,
                isDeleted: true)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "Cong nghe", parentId: null);

        // Act
        var result = await service.CreateCate(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cong nghe", result.title);
        Assert.Null(result.parentId);

        Assert.Equal(2, categories.Count);
        Assert.Equal(1, categories.Count(x => !x.IsDeleted));
        Assert.Equal(1, categories.Count(x => x.IsDeleted));
    }

    // TC09: parentId là Guid.Empty
    // Theo service hiện tại: Guid.Empty khác null nên sẽ đi tìm parent và throw NotFoundException
    [Fact]
    public async Task Create_EmptyParentId_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "Cong nghe", parentId: Guid.Empty);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateCate(request)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
        Assert.Empty(categories);
    }

    // TC10: Title chỉ chứa khoảng trắng
    [Fact]
    public async Task Create_WhitespaceTitle_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var request = CreateRequest(title: "     ", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateCate(request)
        );

        // Assert
        Assert.Equal("CATEGORY_TITLE_REQUIRED", exception.Message);
        Assert.Empty(categories);
    }

    // TC11: Title quá dài
    // MockDB không enforce max length DB. Theo service hiện tại vẫn tạo thành công.
    [Fact]
    public async Task Create_TitleTooLong_CurrentServiceStillCreates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userId);
        var longTitle = new string('A', 257);
        var request = CreateRequest(title: longTitle, parentId: null);

        // Act
        var result = await service.CreateCate(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longTitle, result.title);
        Assert.Single(categories);
    }

    // TC12: UserId từ JWT không tồn tại trong bảng Users vẫn tạo thành công
    [Fact]
    public async Task Create_UserIdNotInUsers_Success()
    {
        // Arrange
        var userIdFromJwt = Guid.NewGuid();
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext, userIdFromJwt);
        var request = CreateRequest(title: "Cong nghe", parentId: null);

        // Act
        var result = await service.CreateCate(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userIdFromJwt, result.userId);
        Assert.Equal("Cong nghe", result.title);
        Assert.Single(categories);
    }
}