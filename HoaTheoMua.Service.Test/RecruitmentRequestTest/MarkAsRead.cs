using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using HoaTheoMua.Service.Test.TestSupport;
using static HoaTheoMua.Service.Test.TestSupport.ReflectionHelper;

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

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    // Async-queryable shims moved to TestSupport/MockAsyncQueryable.cs
}