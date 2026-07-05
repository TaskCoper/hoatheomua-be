using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using RecruitmentRequestEntity = HoaTheoMua.Repository.Entity.RecruitmentRequest;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests.Service;

namespace HoaTheoMua.Service.Test.RecruitmentRequestTest;

public class MarkAsReadTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid RequestId =
        Guid.Parse("77777777-7777-7777-7777-777777777777");

    private static readonly Guid NotFoundRequestId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly Guid JobDescriptionId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static RecruitmentRequestService CreateService(
        List<RecruitmentRequestEntity> recruitmentRequests,
        out Mock<AppDbContext> dbContextMock)
    {
        var recruitmentRequestDbSetMock = BuildMockDbSet(recruitmentRequests);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.RecruitmentRequests)
            .Returns(recruitmentRequestDbSetMock.Object);

        dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(UserId);

        return new RecruitmentRequestService(dbContextMock.Object, currentUserMock.Object);
    }

    private static Mock<DbSet<T>> BuildMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();

        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(sourceList.GetEnumerator()));

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.Expression)
            .Returns(queryable.Expression);

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.ElementType)
            .Returns(queryable.ElementType);

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => sourceList.GetEnumerator());

        dbSetMock
            .Setup(x => x.Update(It.IsAny<T>()))
            .Returns<T>(_ => null!);

        return dbSetMock;
    }

    private static RecruitmentRequestEntity CreateRecruitmentRequest(
        Guid? id = null,
        Guid? userId = null,
        Guid? jobDescriptionId = null,
        string title = "Application for Senior Developer",
        string coverLetter = "I am writing to apply for...",
        List<string>? attachments = null,
        bool isRead = false,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        var request = Activator.CreateInstance<RecruitmentRequestEntity>()!;

        SetPropertyIfExists(request, "Id", id ?? RequestId);
        SetPropertyIfExists(request, "JobDescriptionId", jobDescriptionId ?? JobDescriptionId);
        SetPropertyIfExists(request, "UserId", userId ?? UserId);
        SetPropertyIfExists(request, "AuthorName", "Nguyễn Văn A");
        SetPropertyIfExists(request, "AuthorEmail", "nguyenvana@example.com");
        SetPropertyIfExists(request, "AuthorAvatar", "https://example.com/avatar.png");
        SetPropertyIfExists(request, "title", title);
        SetPropertyIfExists(request, "coverLetter", coverLetter);
        SetPropertyIfExists(request, "attachments", attachments ?? new List<string>
        {
            "https://example.com/cv.pdf"
        });
        SetPropertyIfExists(request, "isRead", isRead);
        SetPropertyIfExists(request, "IsDeleted", isDeleted);
        SetPropertyIfExists(request, "CreatedAt", createdAt ?? DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"));
        SetPropertyIfExists(request, "UpdatedAt", updatedAt);

        return request;
    }

    // TC01
    [Fact]
    public async Task MarkAsRead_Success()
    {
        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
            isRead: false,
            isDeleted: false,
            updatedAt: null);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out var dbContextMock);

        var result = await service.MarkAsReadAsync(RequestId);

        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(RequestId, GetGuid(value!, "id"));
        Assert.True(GetBool(value!, "isRead"));
        Assert.NotNull(GetProperty(value!, "updatedAt"));

        Assert.True(GetBoolProperty(recruitmentRequest, "isRead"));
        Assert.NotNull(GetProperty(recruitmentRequest, "UpdatedAt"));

        dbContextMock.Verify(
            x => x.RecruitmentRequests.Update(It.IsAny<RecruitmentRequestEntity>()),
            Times.Once);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task MarkAsRead_NotFound_ThrowNotFound()
    {
        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(recruitmentRequests, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.MarkAsReadAsync(NotFoundRequestId));

        Assert.Equal("RECRUITMENT_REQUEST_NOT_FOUND", ex.Message);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task MarkAsRead_CurrentServiceDoesNotCheckAdminRole()
    {
        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
            isRead: false,
            isDeleted: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.MarkAsReadAsync(RequestId);

        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(RequestId, GetGuid(value!, "id"));
        Assert.True(GetBool(value!, "isRead"));
    }

    // TC04
    [Fact]
    public async Task MarkAsRead_EmptyGuid_CurrentServiceThrowNotFound()
    {
        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
            isRead: false,
            isDeleted: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.MarkAsReadAsync(Guid.Empty));

        Assert.Equal("RECRUITMENT_REQUEST_NOT_FOUND", ex.Message);

        Assert.False(GetBoolProperty(recruitmentRequest, "isRead"));
        Assert.Null(GetProperty(recruitmentRequest, "UpdatedAt"));

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC05
    [Fact]
    public async Task MarkAsRead_AlreadyRead_StillSuccessAndRefreshUpdatedAt()
    {
        var oldUpdatedAt = DateTimeOffset.Parse("2026-06-20T09:00:00Z");

        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
            isRead: true,
            isDeleted: false,
            updatedAt: oldUpdatedAt);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out var dbContextMock);

        var result = await service.MarkAsReadAsync(RequestId);

        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(RequestId, GetGuid(value!, "id"));
        Assert.True(GetBool(value!, "isRead"));
        Assert.NotNull(GetProperty(value!, "updatedAt"));

        var newUpdatedAt = GetDateTimeOffsetProperty(recruitmentRequest, "UpdatedAt");

        Assert.True(GetBoolProperty(recruitmentRequest, "isRead"));
        Assert.NotNull(newUpdatedAt);
        Assert.True(newUpdatedAt > oldUpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC06
    [Fact]
    public async Task MarkAsRead_SoftDeleted_ThrowNotFound()
    {
        var deletedRequest = CreateRecruitmentRequest(
            id: RequestId,
            isRead: false,
            isDeleted: true,
            updatedAt: null);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            deletedRequest
        };

        var service = CreateService(recruitmentRequests, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.MarkAsReadAsync(RequestId));

        Assert.Equal("RECRUITMENT_REQUEST_NOT_FOUND", ex.Message);

        Assert.False(GetBoolProperty(deletedRequest, "isRead"));
        Assert.Null(GetProperty(deletedRequest, "UpdatedAt"));

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static object? GetValue(object result)
    {
        return GetProperty(result, "Value") ?? GetProperty(result, "value");
    }

    private static object? GetProperty(object? obj, string propertyName)
    {
        if (obj == null)
        {
            return null;
        }

        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        return property?.GetValue(obj);
    }

    private static Guid GetGuid(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is Guid guid)
        {
            return guid;
        }

        return Guid.Parse(value!.ToString()!);
    }

    private static bool GetBool(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is bool boolValue)
        {
            return boolValue;
        }

        return Convert.ToBoolean(value);
    }

    private static bool GetBoolProperty(object obj, string propertyName)
    {
        return GetBool(obj, propertyName);
    }

    private static DateTimeOffset? GetDateTimeOffsetProperty(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value == null)
        {
            return null;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset;
        }

        return DateTimeOffset.Parse(value.ToString()!);
    }

    private static void SetPropertyIfExists(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null || !property.CanWrite)
        {
            return;
        }

        if (value == null)
        {
            property.SetValue(obj, null);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (targetType.IsEnum)
        {
            var enumValue = Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
            property.SetValue(obj, enumValue);
            return;
        }

        if (targetType == typeof(Guid))
        {
            property.SetValue(obj, Guid.Parse(value.ToString()!));
            return;
        }

        if (targetType == typeof(DateTimeOffset))
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                property.SetValue(obj, dateTimeOffset);
            }
            else
            {
                property.SetValue(obj, DateTimeOffset.Parse(value.ToString()!));
            }

            return;
        }

        if (targetType.IsAssignableFrom(value.GetType()))
        {
            property.SetValue(obj, value);
            return;
        }

        property.SetValue(obj, Convert.ChangeType(value, targetType));
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];

            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method =>
                    method.Name == nameof(IQueryProvider.Execute) &&
                    method.IsGenericMethod &&
                    method.GetParameters().Length == 1)
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new object[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private class TestAsyncEnumerable<T> :
        EnumerableQuery<T>,
        IAsyncEnumerable<T>,
        IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider =>
            new TestAsyncQueryProvider<T>(this);
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}