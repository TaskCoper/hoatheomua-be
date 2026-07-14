using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using HoaTheoMua.Service.Test.TestSupport;
using static HoaTheoMua.Service.Test.TestSupport.ReflectionHelper;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using RecruitmentRequestEntity = HoaTheoMua.Repository.Entity.RecruitmentRequest;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests.Service;

namespace HoaTheoMua.Service.Test.RecruitmentRequestTest;

public class GetMyRecruitmentRequestsTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OtherUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid JobDescriptionId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid RequestId1 =
        Guid.Parse("77777777-7777-7777-7777-777777777777");

    private static readonly Guid RequestId2 =
        Guid.Parse("88888888-8888-8888-8888-888888888888");

    private static readonly Guid RequestId3 =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static RecruitmentRequestService CreateService(
        List<RecruitmentRequestEntity> recruitmentRequests,
        out Mock<AppDbContext> dbContextMock,
        bool throwUnauthorized = false)
    {
        var recruitmentRequestDbSetMock = BuildMockDbSet(recruitmentRequests);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.RecruitmentRequests)
            .Returns(recruitmentRequestDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUserService>();

        if (throwUnauthorized)
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Throws(new UnauthorizedException("User not found"));
        }
        else
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Returns(UserId);
        }

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

        return dbSetMock;
    }

    private static JobDescriptionEntity CreateJobDescription(
        Guid? id = null,
        string title = "Senior Software Engineer",
        StatusJobDescriptions status = StatusJobDescriptions.Open,
        bool isDeleted = false)
    {
        var job = Activator.CreateInstance<JobDescriptionEntity>()!;

        SetPropertyIfExists(job, "Id", id ?? JobDescriptionId);
        SetPropertyIfExists(job, "id", id ?? JobDescriptionId);
        SetPropertyIfExists(job, "title", title);
        SetPropertyIfExists(job, "Title", title);
        SetPropertyIfExists(job, "slug", "senior-software-engineer");
        SetPropertyIfExists(job, "location", "Ho Chi Minh");
        SetPropertyIfExists(job, "avatarUrl", "https://example.com/avatar.png");
        SetPropertyIfExists(job, "coverImageUrl", "https://example.com/cover.png");
        SetPropertyIfExists(job, "employmentType", ParseEnumIfExists("EmploymentType", "FullTime"));
        SetPropertyIfExists(job, "salaryNegotiable", false);
        SetPropertyIfExists(job, "description", "Job description...");
        SetPropertyIfExists(job, "status", status);
        SetPropertyIfExists(job, "openings", 2);
        SetPropertyIfExists(job, "IsDeleted", isDeleted);
        SetPropertyIfExists(job, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-10));
        SetPropertyIfExists(job, "UpdatedAt", null);

        return job;
    }

    private static RecruitmentRequestEntity CreateRecruitmentRequest(
        Guid? id = null,
        Guid? userId = null,
        Guid? jobDescriptionId = null,
        JobDescriptionEntity? jobDescription = null,
        string title = "Application for Senior Developer",
        string coverLetter = "I am writing to apply for...",
        List<string>? attachments = null,
        bool isRead = false,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null)
    {
        var request = Activator.CreateInstance<RecruitmentRequestEntity>()!;

        var finalJobId = jobDescriptionId ?? JobDescriptionId;
        var finalJob = jobDescription ?? CreateJobDescription(id: finalJobId);

        SetPropertyIfExists(request, "Id", id ?? Guid.NewGuid());
        SetPropertyIfExists(request, "JobDescriptionId", finalJobId);
        SetPropertyIfExists(request, "JobDescription", finalJob);
        SetPropertyIfExists(request, "UserId", userId ?? UserId);
        SetPropertyIfExists(request, "AuthorName", "Admin User");
        SetPropertyIfExists(request, "AuthorEmail", "admin@example.com");
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
        SetPropertyIfExists(request, "UpdatedAt", null);

        return request;
    }

    // TC01
    [Fact]
    public async Task GetMyRecruitmentRequests_Success()
    {
        var job = CreateJobDescription(
            id: JobDescriptionId,
            title: "Senior Software Engineer");

        var myRequest = CreateRecruitmentRequest(
            id: RequestId1,
            userId: UserId,
            jobDescriptionId: JobDescriptionId,
            jobDescription: job,
            title: "Application for Senior Developer",
            coverLetter: "I am writing to apply for...",
            attachments: new List<string>
            {
                "https://example.com/cv.pdf"
            },
            isRead: false,
            isDeleted: false,
            createdAt: DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"));

        var otherUserRequest = CreateRecruitmentRequest(
            id: RequestId2,
            userId: OtherUserId,
            jobDescriptionId: JobDescriptionId,
            jobDescription: job,
            isDeleted: false);

        var deletedRequest = CreateRecruitmentRequest(
            id: RequestId3,
            userId: UserId,
            jobDescriptionId: JobDescriptionId,
            jobDescription: job,
            isDeleted: true);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            myRequest,
            otherUserRequest,
            deletedRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetMyRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10);

        var items = GetItems(result);

        Assert.Single(items);

        var item = items[0];

        Assert.Equal(RequestId1, GetGuid(item, "id"));
        Assert.Equal("Application for Senior Developer", GetString(item, "title"));
        Assert.Equal("I am writing to apply for...", GetString(item, "coverLetter"));
        Assert.False(GetBool(item, "isRead"));
        Assert.Equal(DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"), GetDateTimeOffset(item, "createdAt"));

        var attachments = GetStringList(GetProperty(item, "attachments"));
        Assert.Single(attachments);
        Assert.Contains("https://example.com/cv.pdf", attachments);

        var jobDescription = GetProperty(item, "jobDescription");
        Assert.NotNull(jobDescription);
        Assert.Equal(JobDescriptionId, GetGuid(jobDescription!, "id"));
        Assert.Equal("Senior Software Engineer", GetString(jobDescription!, "title"));

        Assert.Equal(1, GetPaginationInt(result, "pageIndex"));
        Assert.Equal(10, GetPaginationInt(result, "pageSize"));
        Assert.Equal(1, GetPaginationInt(result, "totalCount"));
        Assert.False(GetPaginationBool(result, "hasNextPage"));
        Assert.False(GetPaginationBool(result, "hasPreviousPage"));
    }

    // TC02
    [Fact]
    public async Task GetMyRecruitmentRequests_Unauthorized_ThrowUnauthorized()
    {
        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            CreateRecruitmentRequest()
        };

        var service = CreateService(
            recruitmentRequests,
            out _,
            throwUnauthorized: true);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.GetMyRecruitmentRequestsAsync(1, 10));

        Assert.Equal("User not found", ex.Message);
    }

    // TC03
    [Fact]
    public async Task GetMyRecruitmentRequests_NoData_ReturnEmptyList()
    {
        var job = CreateJobDescription();

        var otherUserRequest = CreateRecruitmentRequest(
            id: RequestId2,
            userId: OtherUserId,
            jobDescription: job,
            isDeleted: false);

        var deletedRequest = CreateRecruitmentRequest(
            id: RequestId3,
            userId: UserId,
            jobDescription: job,
            isDeleted: true);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            otherUserRequest,
            deletedRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetMyRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10);

        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetPaginationInt(result, "totalCount"));
        Assert.Equal(1, GetPaginationInt(result, "pageIndex"));
        Assert.Equal(10, GetPaginationInt(result, "pageSize"));
    }

    // TC04
    [Fact]
    public async Task GetMyRecruitmentRequests_Pagination_Success()
    {
        var job = CreateJobDescription();

        var requestNewest = CreateRecruitmentRequest(
            id: RequestId1,
            userId: UserId,
            jobDescription: job,
            title: "Request newest",
            createdAt: DateTimeOffset.Parse("2026-06-23T10:00:00Z"));

        var requestMiddle = CreateRecruitmentRequest(
            id: RequestId2,
            userId: UserId,
            jobDescription: job,
            title: "Request middle",
            createdAt: DateTimeOffset.Parse("2026-06-23T09:00:00Z"));

        var requestOldest = CreateRecruitmentRequest(
            id: RequestId3,
            userId: UserId,
            jobDescription: job,
            title: "Request oldest",
            createdAt: DateTimeOffset.Parse("2026-06-23T08:00:00Z"));

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            requestOldest,
            requestNewest,
            requestMiddle
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetMyRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 2);

        var items = GetItems(result);

        Assert.Equal(2, items.Count);

        Assert.Equal(RequestId1, GetGuid(items[0], "id"));
        Assert.Equal("Request newest", GetString(items[0], "title"));

        Assert.Equal(RequestId2, GetGuid(items[1], "id"));
        Assert.Equal("Request middle", GetString(items[1], "title"));

        Assert.Equal(1, GetPaginationInt(result, "pageIndex"));
        Assert.Equal(2, GetPaginationInt(result, "pageSize"));
        Assert.Equal(3, GetPaginationInt(result, "totalCount"));
        Assert.True(GetPaginationBool(result, "hasNextPage"));
        Assert.False(GetPaginationBool(result, "hasPreviousPage"));
    }

    // ParseEnumIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    private static object? GetPaginationValue(object result)
    {
        return GetProperty(result, "Value") ?? GetProperty(result, "value") ?? result;
    }

    private static List<object> GetItems(object result)
    {
        var paginationValue = GetPaginationValue(result);
        var items = GetProperty(paginationValue, "items") ?? GetProperty(paginationValue, "Items");

        if (items is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().ToList();
        }

        throw new Exception("Không đọc được Items từ BasePaginationResponse");
    }

    private static int GetPaginationInt(object result, string propertyName)
    {
        var paginationValue = GetPaginationValue(result);
        return GetInt(paginationValue!, propertyName);
    }

    private static bool GetPaginationBool(object result, string propertyName)
    {
        var paginationValue = GetPaginationValue(result);
        return GetBool(paginationValue!, propertyName);
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

    private static string? GetString(object obj, string propertyName)
    {
        return GetProperty(obj, propertyName)?.ToString();
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

    private static int GetInt(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is int intValue)
        {
            return intValue;
        }

        return Convert.ToInt32(value);
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

    private static DateTimeOffset GetDateTimeOffset(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset;
        }

        return DateTimeOffset.Parse(value!.ToString()!);
    }

    private static List<string> GetStringList(object? value)
    {
        if (value is List<string> list)
        {
            return list;
        }

        if (value is IEnumerable<string> enumerable)
        {
            return enumerable.ToList();
        }

        return new List<string>();
    }

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    // Async-queryable shims (with EF operator cleaning) moved to TestSupport/MockAsyncQueryable.cs
}