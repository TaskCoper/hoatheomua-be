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

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using RecruitmentRequestEntity = HoaTheoMua.Repository.Entity.RecruitmentRequest;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests.Service;

namespace HoaTheoMua.Service.Test.RecruitmentRequestTest;

public class GetRecruitmentRequestByIdTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid JobDescriptionId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid RequestId =
        Guid.Parse("77777777-7777-7777-7777-777777777777");

    private static readonly Guid NotFoundRequestId =
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
        string? coverLetter = "I am writing to apply for...",
        List<string>? attachments = null,
        bool isRead = false,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null)
    {
        var request = Activator.CreateInstance<RecruitmentRequestEntity>()!;

        var finalJobId = jobDescriptionId ?? JobDescriptionId;
        var finalJob = jobDescription ?? CreateJobDescription(id: finalJobId);

        SetPropertyIfExists(request, "Id", id ?? RequestId);
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
    public async Task GetRecruitmentRequestById_Success()
    {
        var job = CreateJobDescription(
            id: JobDescriptionId,
            title: "Senior Software Engineer");

        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
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

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetRecruitmentRequestByIdAsync(RequestId);

        var value = GetValue(result);

        Assert.NotNull(value);

        Assert.Equal(RequestId, GetGuid(value!, "id"));
        Assert.Equal("Application for Senior Developer", GetString(value!, "title"));
        Assert.Equal("I am writing to apply for...", GetString(value!, "coverLetter"));
        Assert.False(GetBool(value!, "isRead"));
        Assert.Equal(DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z"), GetDateTimeOffset(value!, "createdAt"));

        var attachments = GetStringList(GetProperty(value!, "attachments"));
        Assert.Single(attachments);
        Assert.Contains("https://example.com/cv.pdf", attachments);

        var user = GetProperty(value!, "user");
        Assert.NotNull(user);
        Assert.Equal(UserId, GetGuid(user!, "userId"));
        Assert.Equal("Nguyễn Văn A", GetString(user!, "fullName"));
        Assert.Equal("nguyenvana@example.com", GetString(user!, "email"));
        Assert.Null(GetProperty(user!, "phoneNumber"));
        Assert.Equal("https://example.com/avatar.png", GetString(user!, "avatar"));

        var jobDescription = GetProperty(value!, "jobDescription");
        Assert.NotNull(jobDescription);
        Assert.Equal(JobDescriptionId, GetGuid(jobDescription!, "id"));
        Assert.Equal("Senior Software Engineer", GetString(jobDescription!, "title"));
    }

    // TC02
    [Fact]
    public async Task GetRecruitmentRequestById_EmptyGuid_ThrowBadRequest()
    {
        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            CreateRecruitmentRequest()
        };

        var service = CreateService(recruitmentRequests, out _);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetRecruitmentRequestByIdAsync(Guid.Empty));

        Assert.Equal("INVALID_RECRUITMENT_REQUEST_ID", ex.Message);
    }

    // TC03
    [Fact]
    public async Task GetRecruitmentRequestById_NotFound_ThrowNotFound()
    {
        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(recruitmentRequests, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetRecruitmentRequestByIdAsync(NotFoundRequestId));

        Assert.Equal("RECRUITMENT_REQUEST_NOT_FOUND", ex.Message);
    }

    // TC04
    [Fact]
    public async Task GetRecruitmentRequestById_CurrentServiceDoesNotCheckAdminRole()
    {
        var job = CreateJobDescription();

        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
            jobDescription: job,
            isDeleted: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetRecruitmentRequestByIdAsync(RequestId);

        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(RequestId, GetGuid(value!, "id"));
    }

    // TC05
    [Fact]
    public async Task GetRecruitmentRequestById_SoftDeleted_ThrowNotFound()
    {
        var job = CreateJobDescription();

        var deletedRequest = CreateRecruitmentRequest(
            id: RequestId,
            jobDescription: job,
            isDeleted: true);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            deletedRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetRecruitmentRequestByIdAsync(RequestId));

        Assert.Equal("RECRUITMENT_REQUEST_NOT_FOUND", ex.Message);
    }

    // TC06
    [Fact]
    public async Task GetRecruitmentRequestById_NoCoverLetterAndEmptyAttachments_Success()
    {
        var job = CreateJobDescription();

        var recruitmentRequest = CreateRecruitmentRequest(
            id: RequestId,
            jobDescription: job,
            title: "Application without cover letter",
            coverLetter: null,
            attachments: new List<string>(),
            isRead: false,
            isDeleted: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            recruitmentRequest
        };

        var service = CreateService(recruitmentRequests, out _);

        var result = await service.GetRecruitmentRequestByIdAsync(RequestId);

        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(RequestId, GetGuid(value!, "id"));
        Assert.Equal("Application without cover letter", GetString(value!, "title"));
        Assert.Null(GetProperty(value!, "coverLetter"));

        var attachments = GetStringList(GetProperty(value!, "attachments"));
        Assert.Empty(attachments);
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