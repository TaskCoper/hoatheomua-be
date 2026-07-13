using System.Collections;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JobDescriptions;
using HoaTheoMua.Service.JobDescriptions.Validations;
using HoaTheoMua.Service.Models;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions.Service;

namespace HoaTheoMua.Service.Test.JobDescriptionTest;

public class GetPagedJobsTest
{
    private AppDbContext CreateMockDbContext(List<JobDescriptionEntity> jobs)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var dbContext = new AppDbContext(options);

        var mockJobDbSet = jobs.BuildMockDbSet();

        SetDbSet(dbContext, "JobDescriptions", mockJobDbSet.Object);

        return dbContext;
    }

    private void SetDbSet<T>(AppDbContext dbContext, string propertyName, DbSet<T> dbSet)
        where T : class
    {
        var property = typeof(AppDbContext).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (property == null)
        {
            throw new Exception($"Không tìm thấy property {propertyName} trong AppDbContext");
        }

        property.SetValue(dbContext, dbSet);
    }

    private JobDescriptionService CreateService(AppDbContext dbContext)
    {
        return new JobDescriptionService(dbContext);
    }

    private JobDescriptionEntity CreateJob(
        Guid? id = null,
        string slug = "senior-software-engineer",
        string title = "Senior Software Engineer",
        string location = "Ho Chi Minh",
        EmploymentType employmentType = EmploymentType.FullTime,
        StatusJobDescriptions status = StatusJobDescriptions.Open,
        float? salaryMin = 30_000_000f,
        float? salaryMax = 50_000_000f,
        bool salaryNegotiable = false,
        int openings = 2,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null
    )
    {
        return new JobDescriptionEntity
        {
            Id = id ?? Guid.NewGuid(),
            slug = slug,
            title = title,
            location = location,
            avatarUrl = "https://example.com/avatar.png",
            coverImageUrl = "https://example.com/cover.png",
            minExperienceYears = 3,
            maxExperienceYears = 7,
            employmentType = employmentType,
            salaryMin = salaryMin,
            salaryMax = salaryMax,
            salaryNegotiable = salaryNegotiable,
            description = "We are looking for a senior software engineer...",
            status = status,
            openings = openings,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted
        };
    }

    private object? GetPropertyOrFieldValue(object? obj, string name)
    {
        if (obj == null)
        {
            return null;
        }

        var flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.IgnoreCase;

        var property = obj.GetType().GetProperty(name, flags);
        if (property != null)
        {
            return property.GetValue(obj);
        }

        var field = obj.GetType().GetField(name, flags);
        if (field != null)
        {
            return field.GetValue(obj);
        }

        return null;
    }

    private object GetPaginationObject(BasePaginationResponse result)
    {
        var value =
            GetPropertyOrFieldValue(result, "Value") ??
            GetPropertyOrFieldValue(result, "value");

        if (value != null)
        {
            var valueItems =
                GetPropertyOrFieldValue(value, "Items") ??
                GetPropertyOrFieldValue(value, "items");

            if (valueItems != null)
            {
                return value;
            }
        }

        return result;
    }

    private List<object> GetItems(BasePaginationResponse result)
    {
        var pagination = GetPaginationObject(result);

        var items =
            GetPropertyOrFieldValue(pagination, "Items") ??
            GetPropertyOrFieldValue(pagination, "items");

        Assert.NotNull(items);

        return ((IEnumerable)items).Cast<object>().ToList();
    }

    private int GetInt(BasePaginationResponse result, string propertyName)
    {
        var pagination = GetPaginationObject(result);

        var value = GetPropertyOrFieldValue(pagination, propertyName);

        Assert.NotNull(value);

        return Convert.ToInt32(value);
    }

    private bool GetBool(BasePaginationResponse result, string propertyName)
    {
        var pagination = GetPaginationObject(result);

        var value = GetPropertyOrFieldValue(pagination, propertyName);

        Assert.NotNull(value);

        return Convert.ToBoolean(value);
    }

    private string GetString(object obj, string propertyName)
    {
        var value = GetPropertyOrFieldValue(obj, propertyName);

        Assert.NotNull(value);

        return value.ToString()!;
    }

    private float? GetFloatNullable(object obj, string propertyName)
    {
        var value = GetPropertyOrFieldValue(obj, propertyName);

        if (value == null)
        {
            return null;
        }

        return Convert.ToSingle(value);
    }

    // TC01: Lấy danh sách thành công không filter
    [Fact]
    public async Task GetPagedJobs_Success()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                title: "Old Job",
                slug: "old-job",
                createdAt: DateTimeOffset.UtcNow.AddDays(-3)
            ),
            CreateJob(
                title: "Newest Job",
                slug: "newest-job",
                createdAt: DateTimeOffset.UtcNow.AddDays(-1)
            ),
            CreateJob(
                title: "Deleted Job",
                slug: "deleted-job",
                isDeleted: true,
                createdAt: DateTimeOffset.UtcNow
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPagedJobsAsync(
            page: 1,
            pageSize: 10,
            title: null,
            location: null,
            employmentType: null,
            salaryMin: null,
            salaryMax: null,
            status: null
        );

        // Assert
        Assert.NotNull(result);

        var items = GetItems(result);

        Assert.Equal(2, items.Count);
        Assert.Equal(2, GetInt(result, "TotalCount"));
        Assert.Equal(1, GetInt(result, "PageIndex"));
        Assert.Equal(10, GetInt(result, "PageSize"));

        Assert.Equal("Newest Job", GetString(items[0], "Title"));
        Assert.Equal("Old Job", GetString(items[1], "Title"));
    }

    // TC02 + TC08: Lọc theo title, location, employmentType, salary, status
    [Fact]
    public async Task GetPagedJobs_FilterAllConditions_Success()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                title: "Senior Software Engineer",
                slug: "senior-software-engineer",
                location: "Ho Chi Minh",
                employmentType: EmploymentType.FullTime,
                salaryMin: 30_000_000f,
                salaryMax: 50_000_000f,
                status: StatusJobDescriptions.Open
            ),
            CreateJob(
                title: "Junior Software Engineer",
                slug: "junior-software-engineer",
                location: "Ha Noi",
                employmentType: EmploymentType.FullTime,
                salaryMin: 10_000_000f,
                salaryMax: 20_000_000f,
                status: StatusJobDescriptions.Open
            ),
            CreateJob(
                title: "Senior Tester",
                slug: "senior-tester",
                location: "Ho Chi Minh",
                employmentType: EmploymentType.PartTime,
                salaryMin: 30_000_000f,
                salaryMax: 50_000_000f,
                status: StatusJobDescriptions.Open
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPagedJobsAsync(
            page: 1,
            pageSize: 10,
            title: "senior",
            location: "Ho Chi",
            employmentType: EmploymentType.FullTime,
            salaryMin: 30_000_000f,
            salaryMax: 50_000_000f,
            status: StatusJobDescriptions.Open
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(1, GetInt(result, "TotalCount"));

        var item = items[0];

        Assert.Equal("Senior Software Engineer", GetString(item, "Title"));
        Assert.Equal("Ho Chi Minh", GetString(item, "Location"));
        Assert.Equal("senior-software-engineer", GetString(item, "Slug"));
        Assert.Equal(30_000_000f, GetFloatNullable(item, "SalaryMin"));
        Assert.Equal(50_000_000f, GetFloatNullable(item, "SalaryMax"));
    }

    // TC03.1: SalaryMin âm
    [Fact]
    public async Task GetPagedJobs_NegativeSalaryMin_ThrowBadRequest()
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetPagedJobsAsync(
                page: 1,
                pageSize: 10,
                title: null,
                location: null,
                employmentType: null,
                salaryMin: -1f,
                salaryMax: null,
                status: null
            )
        );

        // Assert
        Assert.Equal("SALARY_CANNOT_BE_NEGATIVE", exception.Message);
    }

// TC03.2: SalaryMax âm
    [Fact]
    public async Task GetPagedJobs_NegativeSalaryMax_ThrowBadRequest()
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetPagedJobsAsync(
                page: 1,
                pageSize: 10,
                title: null,
                location: null,
                employmentType: null,
                salaryMin: null,
                salaryMax: -1f,
                status: null
            )
        );

        // Assert
        Assert.Equal("SALARY_CANNOT_BE_NEGATIVE", exception.Message);
    }

    // TC04: SalaryMin > SalaryMax
    [Fact]
    public async Task GetPagedJobs_MinSalaryGreaterThanMaxSalary_ThrowBadRequest()
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetPagedJobsAsync(
                page: 1,
                pageSize: 10,
                title: null,
                location: null,
                employmentType: null,
                salaryMin: 50_000_000f,
                salaryMax: 30_000_000f,
                status: null
            )
        );

        // Assert
        Assert.Equal("MIN_SALARY_GREATER_THAN_MAX_SALARY", exception.Message);
    }

    // TC05: Page/PageSize không hợp lệ thì tự chuẩn hóa
    [Fact]
    public async Task GetPagedJobs_InvalidPaging_Normalized()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(title: "Senior Software Engineer")
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPagedJobsAsync(
            page: 0,
            pageSize: 0,
            title: null,
            location: null,
            employmentType: null,
            salaryMin: null,
            salaryMax: null,
            status: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(1, GetInt(result, "PageIndex"));
        Assert.Equal(10, GetInt(result, "PageSize"));
    }

    // TC06: Filter employmentType không có trong DB
    [Fact]
    public async Task GetPagedJobs_EmploymentTypeNotFound_ReturnEmpty()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                title: "Senior Software Engineer",
                employmentType: EmploymentType.FullTime
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPagedJobsAsync(
            page: 1,
            pageSize: 10,
            title: null,
            location: null,
            employmentType: EmploymentType.PartTime,
            salaryMin: null,
            salaryMax: null,
            status: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Empty(items);
        Assert.Equal(0, GetInt(result, "TotalCount"));
    }

    // TC07: salaryNegotiable = true vẫn lọc ra khi có salary filter
    [Fact]
    public async Task GetPagedJobs_SalaryNegotiable_ReturnWhenFilterSalary()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                title: "Negotiable Job",
                slug: "negotiable-job",
                salaryMin: null,
                salaryMax: null,
                salaryNegotiable: true
            ),
            CreateJob(
                title: "Normal Job",
                slug: "normal-job",
                salaryMin: 10_000_000f,
                salaryMax: 15_000_000f,
                salaryNegotiable: false
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPagedJobsAsync(
            page: 1,
            pageSize: 10,
            title: null,
            location: null,
            employmentType: null,
            salaryMin: 30_000_000f,
            salaryMax: 50_000_000f,
            status: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal("Negotiable Job", GetString(items[0], "Title"));
        Assert.True(Convert.ToBoolean(GetPropertyOrFieldValue(items[0], "SalaryNegotiable")));
    }

    // TC09: pageSize = 1 thì phân trang đúng
    [Fact]
    public async Task GetPagedJobs_PageSizeOne_ReturnOneItemAndHasNextPage()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                title: "Job 1",
                slug: "job-1",
                createdAt: DateTimeOffset.UtcNow.AddDays(-1)
            ),
            CreateJob(
                title: "Job 2",
                slug: "job-2",
                createdAt: DateTimeOffset.UtcNow.AddDays(-2)
            ),
            CreateJob(
                title: "Job 3",
                slug: "job-3",
                createdAt: DateTimeOffset.UtcNow.AddDays(-3)
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPagedJobsAsync(
            page: 1,
            pageSize: 1,
            title: null,
            location: null,
            employmentType: null,
            salaryMin: null,
            salaryMax: null,
            status: null
        );

        // Assert
        var items = GetItems(result);

        Assert.Single(items);
        Assert.Equal(3, GetInt(result, "TotalCount"));
        Assert.Equal(1, GetInt(result, "PageIndex"));
        Assert.Equal(1, GetInt(result, "PageSize"));
        Assert.True(GetBool(result, "HasNextPage"));
        Assert.False(GetBool(result, "HasPreviousPage"));
    }
}