using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;

using CategoryNewsEntity = HoaTheoMua.Repository.Entity.CategoryNews;
using CategoryService = HoaTheoMua.Service.CategoryNew.Service;

namespace HoaTheoMua.Service.Test.CategoryNewTest;

public class GetCategoriesTest
{
    private AppDbContext CreateMockDbContext(List<CategoryNewsEntity> categories)
    {
        var mockDbSet = categories.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.CategoryNewsList)
            .Returns(mockDbSet.Object);

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

    private bool TryGetMemberValue(object obj, string memberName, out object? value)
    {
        var type = obj.GetType();

        var property = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(x => string.Equals(
                x.Name,
                memberName,
                StringComparison.OrdinalIgnoreCase));

        if (property != null)
        {
            value = property.GetValue(obj);
            return true;
        }

        var field = type
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(x => string.Equals(
                x.Name,
                memberName,
                StringComparison.OrdinalIgnoreCase));

        if (field != null)
        {
            value = field.GetValue(obj);
            return true;
        }

        value = null;
        return false;
    }

    private object GetRequiredMember(object obj, string memberName)
    {
        var found = TryGetMemberValue(obj, memberName, out var value);

        Assert.True(found, $"Không tìm thấy property/field '{memberName}' trong object {obj.GetType().Name}");
        Assert.NotNull(value);

        return value!;
    }

    private object GetValueObject(object result)
    {
        var hasValue = TryGetMemberValue(result, "value", out var value);

        if (!hasValue)
        {
            hasValue = TryGetMemberValue(result, "Value", out value);
        }

        if (hasValue)
        {
            Assert.NotNull(value);
            return value!;
        }

        return result;
    }

    private List<object> GetItems(object result)
    {
        var valueObject = GetValueObject(result);
        var items = GetRequiredMember(valueObject, "items");

        return ((IEnumerable)items).Cast<object>().ToList();
    }

    private int GetInt(object result, string propertyName)
    {
        var valueObject = GetValueObject(result);
        var value = GetRequiredMember(valueObject, propertyName);

        return Convert.ToInt32(value);
    }

    private Guid GetGuid(object obj, string propertyName)
    {
        return (Guid)GetRequiredMember(obj, propertyName);
    }

    private string GetString(object obj, string propertyName)
    {
        return (string)GetRequiredMember(obj, propertyName);
    }

    private Guid? GetNullableGuid(object obj, string propertyName)
    {
        var found = TryGetMemberValue(obj, propertyName, out var value);

        Assert.True(found, $"Không tìm thấy property/field '{propertyName}' trong object {obj.GetType().Name}");

        return (Guid?)value;
    }

    private List<object>? GetChildren(object category)
    {
        var found = TryGetMemberValue(category, "children", out var value);

        Assert.True(found, $"Không tìm thấy property/field 'children' trong object {category.GetType().Name}");

        if (value == null)
        {
            return null;
        }

        return ((IEnumerable)value).Cast<object>().ToList();
    }

    // TC01: Lấy danh sách category thành công, có parent-child
    [Fact]
    public async Task GetCategories_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId1 = Guid.NewGuid();
        var childId2 = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(childId1, "Software Development", userId, parentId: parentId, isDeleted: false),
            CreateCategory(childId2, "AI & Machine Learning", userId, parentId: parentId, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Deleted Category", userId, parentId: null, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(1, GetInt(result, "pageIndex"));
        Assert.Equal(10, GetInt(result, "pageSize"));
        Assert.Equal(1, GetInt(result, "totalCount"));

        var parent = items.First();

        Assert.Equal(parentId, GetGuid(parent, "categoryId"));
        Assert.Equal("Technology", GetString(parent, "title"));
        Assert.Null(GetNullableGuid(parent, "parentId"));

        var children = GetChildren(parent);

        Assert.NotNull(children);
        Assert.Equal(2, children!.Count);
        Assert.Contains(children, x => GetString(x, "title") == "Software Development");
        Assert.Contains(children, x => GetString(x, "title") == "AI & Machine Learning");
    }

    // TC02: Lọc theo keyword
    [Fact]
    public async Task GetCategories_FilterByKeyword_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Business", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Health", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: "Tech"
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal("Technology", GetString(items.First(), "title"));
        Assert.Equal(1, GetInt(result, "totalCount"));
    }

    // TC03: PageIndex/PageSize không hợp lệ thì tự chuẩn hóa
    [Fact]
    public async Task GetCategories_InvalidPaging_Normalized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 0,
            pageSize: 0,
            keyword: null
        );

        // Assert
        Assert.Equal(1, GetInt(result, "pageIndex"));
        Assert.Equal(10, GetInt(result, "pageSize"));
        Assert.Equal(1, GetInt(result, "totalCount"));
    }

    // TC04: DB rỗng thì trả danh sách rỗng
    [Fact]
    public async Task GetCategories_EmptyDb_ReturnEmpty()
    {
        // Arrange
        var categories = new List<CategoryNewsEntity>();

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetInt(result, "totalCount"));
    }

    // TC04: Tất cả category đều IsDeleted = true thì trả danh sách rỗng
    [Fact]
    public async Task GetCategories_AllDeleted_ReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Deleted 1", userId, parentId: null, isDeleted: true),
            CreateCategory(Guid.NewGuid(), "Deleted 2", userId, parentId: null, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetInt(result, "totalCount"));
    }

    // TC05: Keyword là khoảng trắng
    [Fact]
    public async Task GetCategories_WhitespaceKeyword_ReturnAll()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Business", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: "   "
        );

        // Assert
        var items = GetItems(result);

        Assert.Equal(2, items.Count);
        Assert.Equal(2, GetInt(result, "totalCount"));
    }

    // TC06: Category cha không có children
    [Fact]
    public async Task GetCategories_ParentWithoutChildren_ReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(parentId, "Technology", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);

        var parent = items.First();

        Assert.Equal("Technology", GetString(parent, "title"));

        // Theo service hiện tại: nếu không có children thì children = null
        var children = GetChildren(parent);

        Assert.NotNull(children);
        Assert.Empty(children!);
    }

    // TC07: PageSize > 100 thì chuẩn hóa về 100
    [Fact]
    public async Task GetCategories_PageSizeOver100_NormalizedTo100()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 200,
            keyword: null
        );

        // Assert
        Assert.Equal(1, GetInt(result, "pageIndex"));
        Assert.Equal(100, GetInt(result, "pageSize"));
        Assert.Equal(1, GetInt(result, "totalCount"));
    }

    // TC08: Tất cả category đều là root
    [Fact]
    public async Task GetCategories_AllRootCategories_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Business", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Health", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Equal(3, items.Count);
        Assert.Equal(3, GetInt(result, "totalCount"));

        foreach (var item in items)
        {
            Assert.Null(GetNullableGuid(item, "parentId"));

            // Theo service hiện tại: root không có children thì children = null
            var children = GetChildren(item);

            Assert.NotNull(children);
            Assert.Empty(children!);
        }
    }

    // TC09: Keyword không tìm thấy kết quả
    [Fact]
    public async Task GetCategories_KeywordNotFound_ReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var categories = new List<CategoryNewsEntity>
        {
            CreateCategory(Guid.NewGuid(), "Technology", userId, parentId: null, isDeleted: false),
            CreateCategory(Guid.NewGuid(), "Business", userId, parentId: null, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(categories);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetCategories(
            pageIndex: 1,
            pageSize: 10,
            keyword: "NotFoundKeyword"
        );

        // Assert
        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetInt(result, "totalCount"));
    }
}