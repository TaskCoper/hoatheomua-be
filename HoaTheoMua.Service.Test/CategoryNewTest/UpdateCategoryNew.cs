using Microsoft.EntityFrameworkCore;
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

public class UpdateCateTest
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
        bool isDeleted = false)
    {
        return new CategoryNewsEntity
        {
            Id = id,
            Title = title,
            UserId = userId,
            ParentCategoryNewsId = parentId,
            IsDeleted = isDeleted,
            UpdatedAt = null
        };
    }

    private Request.UpdateCategoriesRequest CreateRequest(
        string title = "Cong Nghe Moi",
        Guid? parentId = null)
    {
        return new Request.UpdateCategoriesRequest
        {
            title = title,
            parentId = parentId
        };
    }

    // TC01: Cập nhật danh mục thành công
    [Fact]
    public async Task Update_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: null);

        // Act
        var result = await service.UpdateCate(categoryId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.categoryId);
        Assert.Equal("Cong Nghe Moi", result.title);
        Assert.Equal(userId, result.userId);
        Assert.Null(result.parentId);

        var updatedCategory = categories.First(x => x.Id == categoryId);

        Assert.Equal("Cong Nghe Moi", updatedCategory.Title);
        Assert.Null(updatedCategory.ParentCategoryNewsId);
        Assert.NotNull(updatedCategory.UpdatedAt);
    }

    // TC02: Title bị trùng trong cùng parent
    [Fact]
    public async Task Update_DuplicateTitle_ThrowConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Tin Tuc", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Tin Tuc", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("CATEGORY_ALREADY_EXISTS", exception.Message);
    }

    // TC03: Gán parent là chính nó
    [Fact]
    public async Task Update_ParentIsSelf_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: categoryId);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("CATEGORY_PARENT_INVALID", exception.Message);
    }

    // TC04: Gán parent cho category đang có con
    [Fact]
    public async Task Update_CategoryHasChildren_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false),
            CreateCategory(childId, "AI", userId, parentId: categoryId, isDeleted: false),
            CreateCategory(newParentId, "Tin Tuc", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: newParentId);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("CATEGORY_PARENT_INVALID", exception.Message);
    }

    // TC05: Category không tồn tại
    [Fact]
    public async Task Update_NotFound_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateCate(Guid.NewGuid(), request)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC06: Cập nhật title giống hệt title cũ
    [Fact]
    public async Task Update_SameTitle_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe", parentId: null);

        // Act
        var result = await service.UpdateCate(categoryId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.categoryId);
        Assert.Equal("Cong Nghe", result.title);
        Assert.Null(result.parentId);

        var updatedCategory = categories.First(x => x.Id == categoryId);

        Assert.Equal("Cong Nghe", updatedCategory.Title);
        Assert.NotNull(updatedCategory.UpdatedAt);
    }

    // TC07: Chuyển category từ root thành child
    [Fact]
    public async Task Update_RootToChild_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false),
            CreateCategory(parentId, "Tin Tuc", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: parentId);

        // Act
        var result = await service.UpdateCate(categoryId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.categoryId);
        Assert.Equal("Cong Nghe Moi", result.title);
        Assert.Equal(parentId, result.parentId);

        var updatedCategory = categories.First(x => x.Id == categoryId);

        Assert.Equal(parentId, updatedCategory.ParentCategoryNewsId);
        Assert.NotNull(updatedCategory.UpdatedAt);
    }

    // TC08: Chỉ cập nhật title, request không gửi parentId
    // Theo service hiện tại: parentId mặc định null, nên category sẽ bị chuyển về root
    [Fact]
    public async Task Update_TitleOnly_CurrentServiceSetParentToNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Parent", userId, parentId: null, isDeleted: false),
            CreateCategory(categoryId, "Child", userId, parentId: parentId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        var request = new Request.UpdateCategoriesRequest
        {
            title = "Child Moi"
            // parentId không set, mặc định là null
        };

        // Act
        var result = await service.UpdateCate(categoryId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Child Moi", result.title);

        // Theo service hiện tại: parentId bị set về null
        Assert.Null(result.parentId);

        var updatedCategory = categories.First(x => x.Id == categoryId);

        Assert.Null(updatedCategory.ParentCategoryNewsId);
        Assert.NotNull(updatedCategory.UpdatedAt);
    }

    // TC09: Category đã bị soft delete
    [Fact]
    public async Task Update_DeletedCategory_ThrowNotFound()
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
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("CATEGORY_NOT_FOUND", exception.Message);
    }

    // TC10: Title chỉ chứa khoảng trắng
    [Fact]
    public async Task Update_WhitespaceTitle_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "    ", parentId: null);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("CATEGORY_TITLE_REQUIRED", exception.Message);
    }

    // TC11: Gán parentId là Guid.Empty
    // Theo service hiện tại: Guid.Empty != null nên service tìm parent và throw PARENT_CATEGORY_NOT_FOUND
    [Fact]
    public async Task Update_EmptyParentId_ThrowParentNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: Guid.Empty);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("PARENT_CATEGORY_NOT_FOUND", exception.Message);
    }

    // Parent không tồn tại
    [Fact]
    public async Task Update_ParentNotFound_ThrowNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: Guid.NewGuid());

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("PARENT_CATEGORY_NOT_FOUND", exception.Message);
    }

    // Parent là category con thì không hợp lệ
    [Fact]
    public async Task Update_ParentIsChild_ThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var rootParentId = Guid.NewGuid();
        var childParentId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(categoryId, "Cong Nghe", userId, parentId: null, isDeleted: false),
            CreateCategory(rootParentId, "Root Parent", userId, parentId: null, isDeleted: false),
            CreateCategory(childParentId, "Child Parent", userId, parentId: rootParentId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);
        var request = CreateRequest(title: "Cong Nghe Moi", parentId: childParentId);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateCate(categoryId, request)
        );

        // Assert
        Assert.Equal("CATEGORY_PARENT_INVALID", exception.Message);
    }
}