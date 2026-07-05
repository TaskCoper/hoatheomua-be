using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using RecruitmentRequestEntity = HoaTheoMua.Repository.Entity.RecruitmentRequest;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests.Service;

namespace HoaTheoMua.Service.Test.RecruitmentRequestTest;

public class GetPagedRecruitmentRequestsTest
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
        out Mock<AppDbContext> dbContextMock)
    {
        var recruitmentRequestDbSetMock = BuildMockDbSet(recruitmentRequests);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.RecruitmentRequests)
            .Returns(recruitmentRequestDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(UserId);

        currentUserMock
            .SetupGet(x => x.FullName)
            .Returns("Nguyễn Văn A");

        currentUserMock
            .SetupGet(x => x.Email)
            .Returns("nguyenvana@example.com");

        currentUserMock
            .SetupGet(x => x.Avatar)
            .Returns("https://example.com/avatar.png");

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
        SetPropertyIfExists(request, "UpdatedAt", null);

        return request;
    }

    // TC01
    [Fact]
    public async Task GetPagedRecruitmentRequests_Success_NoFilter()
    {
        var job = CreateJobDescription(
            id: JobDescriptionId,
            title: "Senior Software Engineer");

        var request = CreateRecruitmentRequest(
            id: RequestId1,
            userId: UserId,
            jobDescription: job,
            title: "Application for Senior Developer",
            coverLetter: "I am writing to apply for...",
            attachments: new List<string>
            {
                "https://example.com/cv.pdf"
            },
            isRead: false,
            createdAt: DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"));

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            request
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10,
            isRead: null);

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

        var user = GetProperty(item, "user");
        Assert.NotNull(user);
        Assert.Equal(UserId, GetGuid(user!, "userId"));
        Assert.Equal("Nguyễn Văn A", GetString(user!, "fullName"));
        Assert.Equal("nguyenvana@example.com", GetString(user!, "email"));
        Assert.Null(GetProperty(user!, "phoneNumber"));
        Assert.Equal("https://example.com/avatar.png", GetString(user!, "avatar"));

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
    public async Task GetPagedRecruitmentRequests_FilterByIsReadFalse()
    {
        var job = CreateJobDescription();

        var unreadRequest = CreateRecruitmentRequest(
            id: RequestId1,
            jobDescription: job,
            title: "Unread request",
            isRead: false);

        var readRequest = CreateRecruitmentRequest(
            id: RequestId2,
            jobDescription: job,
            title: "Read request",
            isRead: true);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            unreadRequest,
            readRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10,
            isRead: false);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(RequestId1, GetGuid(items[0], "id"));
        Assert.Equal("Unread request", GetString(items[0], "title"));
        Assert.False(GetBool(items[0], "isRead"));
        Assert.Equal(1, GetPaginationInt(result, "totalCount"));
    }

    // TC03
    [Fact]
    public async Task GetPagedRecruitmentRequests_CurrentServiceDoesNotCheckAdminRole()
    {
        var job = CreateJobDescription();

        var request = CreateRecruitmentRequest(
            id: RequestId1,
            jobDescription: job,
            isRead: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            request
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10,
            isRead: null);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(RequestId1, GetGuid(items[0], "id"));
    }

    // TC04
    [Fact]
    public async Task GetPagedRecruitmentRequests_FilterByIsReadTrue()
    {
        var job = CreateJobDescription();

        var unreadRequest = CreateRecruitmentRequest(
            id: RequestId1,
            jobDescription: job,
            title: "Unread request",
            isRead: false);

        var readRequest = CreateRecruitmentRequest(
            id: RequestId2,
            jobDescription: job,
            title: "Read request",
            isRead: true);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            unreadRequest,
            readRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10,
            isRead: true);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(RequestId2, GetGuid(items[0], "id"));
        Assert.Equal("Read request", GetString(items[0], "title"));
        Assert.True(GetBool(items[0], "isRead"));
        Assert.Equal(1, GetPaginationInt(result, "totalCount"));
    }

    // TC05
    [Fact]
    public async Task GetPagedRecruitmentRequests_NoData_ReturnEmptyList()
    {
        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10,
            isRead: null);

        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetPaginationInt(result, "totalCount"));
        Assert.Equal(1, GetPaginationInt(result, "pageIndex"));
        Assert.Equal(10, GetPaginationInt(result, "pageSize"));
        Assert.False(GetPaginationBool(result, "hasNextPage"));
        Assert.False(GetPaginationBool(result, "hasPreviousPage"));
    }

    // TC06
    [Fact]
    public async Task GetPagedRecruitmentRequests_PageSizeOver100_NormalizeTo100()
    {
        var job = CreateJobDescription();

        var request = CreateRecruitmentRequest(
            id: RequestId1,
            jobDescription: job,
            isRead: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            request
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 200,
            isRead: null);

        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(100, GetPaginationInt(result, "pageSize"));
        Assert.Equal(1, GetPaginationInt(result, "pageIndex"));
        Assert.Equal(1, GetPaginationInt(result, "totalCount"));
    }

    // TC07
    [Fact]
    public async Task GetPagedRecruitmentRequests_OrderByCreatedAtDescending()
    {
        var job = CreateJobDescription();

        var newestRequest = CreateRecruitmentRequest(
            id: RequestId1,
            jobDescription: job,
            title: "Newest request",
            createdAt: DateTimeOffset.Parse("2026-06-23T10:00:00Z"));

        var middleRequest = CreateRecruitmentRequest(
            id: RequestId2,
            jobDescription: job,
            title: "Middle request",
            createdAt: DateTimeOffset.Parse("2026-06-23T09:00:00Z"));

        var oldestRequest = CreateRecruitmentRequest(
            id: RequestId3,
            jobDescription: job,
            title: "Oldest request",
            createdAt: DateTimeOffset.Parse("2026-06-23T08:00:00Z"));

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            oldestRequest,
            newestRequest,
            middleRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetPagedRecruitmentRequestsAsync(
            pageIndex: 1,
            pageSize: 10,
            isRead: null);

        var items = GetItems(result);

        Assert.Equal(3, items.Count);

        Assert.Equal(RequestId1, GetGuid(items[0], "id"));
        Assert.Equal("Newest request", GetString(items[0], "title"));

        Assert.Equal(RequestId2, GetGuid(items[1], "id"));
        Assert.Equal("Middle request", GetString(items[1], "title"));

        Assert.Equal(RequestId3, GetGuid(items[2], "id"));
        Assert.Equal("Oldest request", GetString(items[2], "title"));
    }

    private static object? ParseEnumIfExists(string enumName, string value)
    {
        var enumType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .FirstOrDefault(x => x.IsEnum && x.Name == enumName);

        if (enumType == null)
        {
            return value;
        }

        return Enum.Parse(enumType, value, ignoreCase: true);
    }

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

    private class EfExpressionCleaner : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) &&
                (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking) ||
                 node.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include) ||
                 node.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)))
            {
                return Visit(node.Arguments[0])!;
            }

            return base.VisitMethodCall(node);
        }
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        private static Expression CleanExpression(Expression expression)
        {
            return new EfExpressionCleaner().Visit(expression)!;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(CleanExpression(expression));
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(CleanExpression(expression));
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(CleanExpression(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(CleanExpression(expression));
        }

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            var cleanedExpression = CleanExpression(expression);
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];

            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method =>
                    method.Name == nameof(IQueryProvider.Execute) &&
                    method.IsGenericMethod &&
                    method.GetParameters().Length == 1)
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new object[] { cleanedExpression });

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