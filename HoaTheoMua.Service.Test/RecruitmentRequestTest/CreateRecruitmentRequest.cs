using System.Linq.Expressions;
using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using RecruitmentRequestEntity = HoaTheoMua.Repository.Entity.RecruitmentRequest;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests.Service;
using RecruitmentRequestCreateRequest = HoaTheoMua.Service.RecruitmentRequests.Request.CreateRecruitmentRequest;

namespace HoaTheoMua.Service.Test.RecruitmentRequestTest;

public class CreateRecruitmentRequestTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid JobDescriptionId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid NotFoundJobDescriptionId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static RecruitmentRequestService CreateService(
        List<JobDescriptionEntity> jobDescriptions,
        List<RecruitmentRequestEntity> recruitmentRequests,
        out Mock<AppDbContext> dbContextMock,
        bool throwUnauthorized = false)
    {
        var jobDescriptionDbSetMock = BuildMockDbSet(jobDescriptions);
        var recruitmentRequestDbSetMock = BuildMockDbSet(recruitmentRequests);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.JobDescriptions)
            .Returns(jobDescriptionDbSetMock.Object);

        dbContextMock
            .Setup(x => x.RecruitmentRequests)
            .Returns(recruitmentRequestDbSetMock.Object);

        dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUserService>();

        if (throwUnauthorized)
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Throws(new UnauthorizedAccessException("Unauthorized"));
        }
        else
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Returns(UserId);
        }

        currentUserMock
            .SetupGet(x => x.FullName)
            .Returns("Admin User");

        currentUserMock
            .SetupGet(x => x.Email)
            .Returns("admin@example.com");

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

        dbSetMock
            .Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) =>
            {
                sourceList.Add(entity);
            })
            .Returns<T, CancellationToken>((_, _) =>
                new ValueTask<EntityEntry<T>>((EntityEntry<T>)null!));

        dbSetMock
            .Setup(x => x.Update(It.IsAny<T>()))
            .Returns<T>(_ => null!);

        return dbSetMock;
    }

    private static RecruitmentRequestCreateRequest CreateValidRequest(
        Guid? jobDescriptionId = null,
        string title = "Application for Senior Developer",
        string coverLetter = "I am writing to apply for the Senior Developer position...",
        List<string>? attachments = null)
    {
        return new RecruitmentRequestCreateRequest
        {
            jobDescriptionId = jobDescriptionId ?? JobDescriptionId,
            title = title,
            coverLetter = coverLetter,
            attachments = attachments ?? new List<string>
            {
                "https://example.com/cv.pdf"
            }
        };
    }

    private static JobDescriptionEntity CreateJobDescription(
        Guid? id = null,
        string title = "Senior Developer",
        StatusJobDescriptions status = StatusJobDescriptions.Open,
        bool isDeleted = false)
    {
        var job = Activator.CreateInstance<JobDescriptionEntity>()!;

        SetPropertyIfExists(job, "Id", id ?? JobDescriptionId);
        SetPropertyIfExists(job, "id", id ?? JobDescriptionId);
        SetPropertyIfExists(job, "title", title);
        SetPropertyIfExists(job, "Title", title);
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
        Guid? jobDescriptionId = null,
        Guid? userId = null,
        string title = "Old Application",
        string coverLetter = "Old cover letter",
        List<string>? attachments = null,
        bool isRead = false,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null)
    {
        var request = Activator.CreateInstance<RecruitmentRequestEntity>()!;

        SetPropertyIfExists(request, "Id", id ?? Guid.NewGuid());
        SetPropertyIfExists(request, "JobDescriptionId", jobDescriptionId ?? JobDescriptionId);
        SetPropertyIfExists(request, "UserId", userId ?? UserId);
        SetPropertyIfExists(request, "AuthorName", "Admin User");
        SetPropertyIfExists(request, "AuthorEmail", "admin@example.com");
        SetPropertyIfExists(request, "AuthorAvatar", "https://example.com/avatar.png");
        SetPropertyIfExists(request, "title", title);
        SetPropertyIfExists(request, "coverLetter", coverLetter);
        SetPropertyIfExists(request, "attachments", attachments ?? new List<string>());
        SetPropertyIfExists(request, "isRead", isRead);
        SetPropertyIfExists(request, "IsDeleted", isDeleted);
        SetPropertyIfExists(request, "CreatedAt", createdAt ?? DateTimeOffset.UtcNow.AddDays(-1));
        SetPropertyIfExists(request, "UpdatedAt", null);

        return request;
    }

    // TC01
    [Fact]
    public async Task CreateRecruitmentRequest_Success()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest();

        var result = await service.CreateRecruitmentRequestAsync(request);

        var createdRequest = Assert.Single(recruitmentRequests);

        Assert.Equal(JobDescriptionId, result.jobDescriptionId);
        Assert.Equal("Application for Senior Developer", result.title);
        Assert.Contains("https://example.com/cv.pdf", result.attachments);
        Assert.False(result.isRead);
        Assert.NotEqual(default, result.createdAt);

        Assert.Equal(JobDescriptionId, GetGuidProperty(createdRequest, "JobDescriptionId"));
        Assert.Equal(UserId, GetGuidProperty(createdRequest, "UserId"));
        Assert.Equal("Admin User", GetStringProperty(createdRequest, "AuthorName"));
        Assert.Equal("admin@example.com", GetStringProperty(createdRequest, "AuthorEmail"));
        Assert.Equal("https://example.com/avatar.png", GetStringProperty(createdRequest, "AuthorAvatar"));
        Assert.Equal("Application for Senior Developer", GetStringProperty(createdRequest, "title"));
        Assert.Equal("I am writing to apply for the Senior Developer position...", GetStringProperty(createdRequest, "coverLetter"));
        Assert.False(GetBoolProperty(createdRequest, "isRead"));
        Assert.False(GetBoolProperty(createdRequest, "IsDeleted"));

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task CreateRecruitmentRequest_JobNotFoundOrNotOpen_ThrowNotFound()
    {
        var jobDescriptions = new List<JobDescriptionEntity>();

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest(
            jobDescriptionId: NotFoundJobDescriptionId);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateRecruitmentRequestAsync(request));

        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND_OR_NOT_OPEN", ex.Message);
        Assert.Empty(recruitmentRequests);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task CreateRecruitmentRequest_AlreadySubmittedAndPending_ThrowTooManyRequest()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var oldPendingRequest = CreateRecruitmentRequest(
            jobDescriptionId: JobDescriptionId,
            userId: UserId,
            isRead: false,
            isDeleted: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            oldPendingRequest
        };

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<TooManyRequestException>(
            () => service.CreateRecruitmentRequestAsync(request));

        Assert.Equal("APPLICATION_ALREADY_SUBMITTED_AND_PENDING", ex.Message);
        Assert.Single(recruitmentRequests);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task CreateRecruitmentRequest_Unauthorized_ThrowUnauthorized()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock,
            throwUnauthorized: true);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateRecruitmentRequestAsync(request));

        Assert.Equal("Unauthorized", ex.Message);
        Assert.Empty(recruitmentRequests);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC05
    [Fact]
    public async Task CreateRecruitmentRequest_EmptyAttachments_StillSuccess()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest(
            attachments: new List<string>());

        var result = await service.CreateRecruitmentRequestAsync(request);

        var createdRequest = Assert.Single(recruitmentRequests);

        Assert.Empty(result.attachments);
        Assert.Empty(GetStringListProperty(createdRequest, "attachments"));

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC06
    [Fact]
    public async Task CreateRecruitmentRequest_CoverLetterTooLong_CurrentServiceStillCreates()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var longCoverLetter = new string('a', 2001);

        var request = CreateValidRequest(
            coverLetter: longCoverLetter);

        var result = await service.CreateRecruitmentRequestAsync(request);

        var createdRequest = Assert.Single(recruitmentRequests);

        Assert.Equal(longCoverLetter, GetStringProperty(createdRequest, "coverLetter"));
        Assert.False(result.isRead);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC07
    [Fact]
    public async Task CreateRecruitmentRequest_JobDescriptionIdEmpty_ThrowNotFound()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest(
            jobDescriptionId: Guid.Empty);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateRecruitmentRequestAsync(request));

        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND_OR_NOT_OPEN", ex.Message);
        Assert.Empty(recruitmentRequests);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC08
    [Fact]
    public async Task CreateRecruitmentRequest_OldRequestIsReadTrue_AllowApplyAgain()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var oldReadRequest = CreateRecruitmentRequest(
            jobDescriptionId: JobDescriptionId,
            userId: UserId,
            isRead: true,
            isDeleted: false);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            oldReadRequest
        };

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest(
            title: "Apply again");

        var result = await service.CreateRecruitmentRequestAsync(request);

        Assert.Equal(2, recruitmentRequests.Count);
        Assert.Equal("Apply again", result.title);
        Assert.False(result.isRead);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC09
    [Fact]
    public async Task CreateRecruitmentRequest_OldSoftDeletedPendingRequest_CurrentServiceStillBlocksSpam()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var oldSoftDeletedPendingRequest = CreateRecruitmentRequest(
            jobDescriptionId: JobDescriptionId,
            userId: UserId,
            isRead: false,
            isDeleted: true);

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            oldSoftDeletedPendingRequest
        };

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<TooManyRequestException>(
            () => service.CreateRecruitmentRequestAsync(request));

        Assert.Equal("APPLICATION_ALREADY_SUBMITTED_AND_PENDING", ex.Message);
        Assert.Single(recruitmentRequests);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC10
    [Fact]
    public async Task CreateRecruitmentRequest_EmptyTitle_CurrentServiceStillCreates()
    {
        var jobDescriptions = new List<JobDescriptionEntity>
        {
            CreateJobDescription()
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var service = CreateService(
            jobDescriptions,
            recruitmentRequests,
            out var dbContextMock);

        var request = CreateValidRequest(
            title: "");

        var result = await service.CreateRecruitmentRequestAsync(request);

        var createdRequest = Assert.Single(recruitmentRequests);

        Assert.Equal("", result.title);
        Assert.Equal("", GetStringProperty(createdRequest, "title"));

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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

    private static object? GetProperty(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        return property?.GetValue(obj);
    }

    private static string? GetStringProperty(object obj, string propertyName)
    {
        return GetProperty(obj, propertyName)?.ToString();
    }

    private static Guid GetGuidProperty(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is Guid guid)
        {
            return guid;
        }

        return Guid.Parse(value!.ToString()!);
    }

    private static bool GetBoolProperty(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is bool boolValue)
        {
            return boolValue;
        }

        return Convert.ToBoolean(value);
    }

    private static List<string> GetStringListProperty(object obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

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