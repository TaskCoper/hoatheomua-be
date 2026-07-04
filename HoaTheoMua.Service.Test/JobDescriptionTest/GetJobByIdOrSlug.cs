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

public class GetJobByIdOrSlugTest
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
        IValidator<Request.CreateRequest> validator = new JobDescriptionRequestValidator();

        return new JobDescriptionService(dbContext, validator);
    }

    private JobDescriptionEntity CreateJob(
        Guid? id = null,
        string slug = "senior-software-engineer",
        string title = "Senior Software Engineer",
        bool isDeleted = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null
    )
    {
        var now = createdAt ?? DateTimeOffset.UtcNow;

        return new JobDescriptionEntity
        {
            Id = id ?? Guid.NewGuid(),
            title = title,
            slug = slug,
            location = "Ho Chi Minh",
            avatarUrl = "https://example.com/avatar.png",
            coverImageUrl = "https://example.com/cover.png",
            minExperienceYears = 3,
            maxExperienceYears = 7,
            employmentType = EmploymentType.FullTime,
            salaryMin = 30_000_000f,
            salaryMax = 50_000_000f,
            salaryNegotiable = false,
            description = "We are looking for a senior software engineer...",
            status = StatusJobDescriptions.Open,
            openings = 2,
            CreatedAt = now,
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

    private Response.JobDetailResponse GetJobDetail(BaseResponse result)
    {
        var value = GetPropertyOrFieldValue(result, "Value");

        Assert.NotNull(value);

        return Assert.IsType<Response.JobDetailResponse>(value);
    }

    // TC01: Lấy theo ID thành công
    [Fact]
    public async Task GetJobByIdOrSlug_GetById_Success()
    {
        // Arrange
        var jobId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                id: jobId,
                slug: "senior-software-engineer",
                createdAt: createdAt
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetJobByIdOrSlugAsync(jobId.ToString());

        // Assert
        Assert.NotNull(result);

        var detail = GetJobDetail(result);

        Assert.Equal(jobId, detail.Id);
        Assert.Equal("senior-software-engineer", detail.Slug);
        Assert.Equal("Senior Software Engineer", detail.Title);
        Assert.Equal("Ho Chi Minh", detail.Location);
        Assert.Equal("https://example.com/avatar.png", detail.AvatarUrl);
        Assert.Equal("https://example.com/cover.png", detail.CoverImageUrl);
        Assert.Equal(3, detail.MinExperienceYears);
        Assert.Equal(7, detail.MaxExperienceYears);
        Assert.Equal("fullTime", detail.EmploymentType);
        Assert.Equal(30_000_000f, detail.SalaryMin);
        Assert.Equal(50_000_000f, detail.SalaryMax);
        Assert.False(detail.SalaryNegotiable);
        Assert.Equal(2, detail.Openings);
        Assert.Equal("We are looking for a senior software engineer...", detail.Description);
        Assert.Equal("open", detail.Status);
        Assert.Equal(createdAt, detail.CreatedAt);
        Assert.Equal(createdAt, detail.UpdatedAt);
    }

    // TC02: Lấy theo Slug thành công
    [Fact]
    public async Task GetJobByIdOrSlug_GetBySlug_Success()
    {
        // Arrange
        var jobId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                id: jobId,
                slug: "senior-software-engineer"
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var result = await service.GetJobByIdOrSlugAsync("senior-software-engineer");

        // Assert
        Assert.NotNull(result);

        var detail = GetJobDetail(result);

        Assert.Equal(jobId, detail.Id);
        Assert.Equal("senior-software-engineer", detail.Slug);
        Assert.Equal("Senior Software Engineer", detail.Title);
        Assert.Equal("open", detail.Status);
    }

    // TC03: Key rỗng thì lỗi BadRequest
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetJobByIdOrSlug_EmptyKey_ThrowBadRequest(string key)
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetJobByIdOrSlugAsync(key)
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", exception.Message);
    }

    // TC03: Key dài hơn 300 ký tự thì lỗi BadRequest
    [Fact]
    public async Task GetJobByIdOrSlug_KeyTooLong_ThrowBadRequest()
    {
        // Arrange
        var longKey = new string('a', 301);

        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetJobByIdOrSlugAsync(longKey)
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", exception.Message);
    }

    // TC03: Guid.Empty thì lỗi BadRequest
    [Fact]
    public async Task GetJobByIdOrSlug_GuidEmpty_ThrowBadRequest()
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetJobByIdOrSlugAsync(Guid.Empty.ToString())
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", exception.Message);
    }

    // TC04: Không tìm thấy bằng slug
    [Fact]
    public async Task GetJobByIdOrSlug_SlugNotFound_ThrowNotFound()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(slug: "senior-software-engineer")
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetJobByIdOrSlugAsync("backend-developer")
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND", exception.Message);
    }

    // TC06: Slug tồn tại nhưng job đã bị soft delete
    [Fact]
    public async Task GetJobByIdOrSlug_SlugSoftDeleted_ThrowNotFound()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(
                slug: "senior-software-engineer",
                isDeleted: true
            )
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetJobByIdOrSlugAsync("senior-software-engineer")
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND", exception.Message);
    }

    // TC07: Key là GUID hợp lệ nhưng không tồn tại trong DB
    [Fact]
    public async Task GetJobByIdOrSlug_ValidGuidButNotFound_ThrowNotFound()
    {
        // Arrange
        var existingJobId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var notFoundJobId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(id: existingJobId)
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetJobByIdOrSlugAsync(notFoundJobId.ToString())
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND", exception.Message);
    }

    // TC08: Key có khoảng trắng ở giữa thì sai slug format
    [Fact]
    public async Task GetJobByIdOrSlug_KeyContainsSpace_ThrowBadRequest()
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetJobByIdOrSlugAsync("senior developer")
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", exception.Message);
    }

    // TC09: Key có ký tự đặc biệt thì sai slug format
    [Fact]
    public async Task GetJobByIdOrSlug_KeyContainsSpecialCharacter_ThrowBadRequest()
    {
        // Arrange
        var dbContext = CreateMockDbContext(new List<JobDescriptionEntity>());
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetJobByIdOrSlugAsync("senior@developer!")
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", exception.Message);
    }
}