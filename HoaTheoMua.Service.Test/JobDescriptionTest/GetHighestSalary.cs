using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.JobDescriptions;
using HoaTheoMua.Service.JobDescriptions.Validations;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions.Service;

namespace HoaTheoMua.Service.Test.JobDescriptionTest;

public class GetHighestSalaryTest
{
    private AppDbContext CreateMockDbContext(List<JobDescriptionEntity> jobs)
    {
        var mockJobDbSet = jobs.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.JobDescriptions)
            .Returns(mockJobDbSet.Object);

        return mockDbContext.Object;
    }

    private JobDescriptionService CreateService(AppDbContext dbContext)
    {
        return new JobDescriptionService(dbContext);
    }

    private JobDescriptionEntity CreateJob(float? salaryMax)
    {
        return new JobDescriptionEntity
        {
            Id = Guid.NewGuid(),
            title = "Senior Software Engineer",
            slug = "senior-software-engineer",
            location = "Ho Chi Minh",
            avatarUrl = "https://example.com/avatar.png",
            coverImageUrl = "https://example.com/cover.png",
            minExperienceYears = 3,
            maxExperienceYears = 7,
            employmentType = EmploymentType.FullTime,
            salaryMin = 30_000_000f,
            salaryMax = salaryMax,
            salaryNegotiable = false,
            description = "Job description",
            status = StatusJobDescriptions.Open,
            openings = 2,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
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

    private float? GetSalary(BaseResponse result)
    {
        var value = GetPropertyOrFieldValue(result, "Value");

        Assert.NotNull(value);

        var salary = GetPropertyOrFieldValue(value, "Salary");

        if (salary == null)
        {
            return null;
        }

        return Convert.ToSingle(salary);
    }

    // TC01: Lấy mức lương cao nhất thành công
    [Fact]
    public async Task GetHighestSalary_Success()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(50_000_000f)
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetHighestSalaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50_000_000f, GetSalary(result));
    }

    // TC02: Không có dữ liệu
    [Fact]
    public async Task GetHighestSalary_EmptyDb_ReturnNull()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>();

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetHighestSalaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(GetSalary(result));
    }

    // TC04: Có nhiều job với salaryMax khác nhau
    [Fact]
    public async Task GetHighestSalary_MultipleJobs_ReturnMax()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(30_000_000f),
            CreateJob(100_000_000f),
            CreateJob(50_000_000f)
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetHighestSalaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100_000_000f, GetSalary(result));
    }

    // TC05: Tất cả job đều có salaryMax = null
    [Fact]
    public async Task GetHighestSalary_AllSalaryMaxNull_ReturnNull()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(null),
            CreateJob(null)
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetHighestSalaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(GetSalary(result));
    }

    // TC06: salaryMax = 0 vẫn hợp lệ
    [Fact]
    public async Task GetHighestSalary_SalaryMaxZero_ReturnZero()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(0f)
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetHighestSalaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0f, GetSalary(result));
    }
}