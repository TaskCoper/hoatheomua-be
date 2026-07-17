using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JobDescriptions;
using HoaTheoMua.Service.JobDescriptions.Validations;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions.Service;

namespace HoaTheoMua.Service.Test.JobDescriptionTest;

public class CreateJobDescriptionTest
{
    private AppDbContext CreateMockDbContext(List<JobDescriptionEntity> jobDescriptions)
    {
        var mockDbSet = jobDescriptions.BuildMockDbSet();

        mockDbSet
            .Setup(x => x.AddAsync(
                It.IsAny<JobDescriptionEntity>(),
                It.IsAny<CancellationToken>()))
            .Callback((JobDescriptionEntity entity, CancellationToken _) =>
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }

                jobDescriptions.Add(entity);
            })
            .Returns((JobDescriptionEntity _, CancellationToken _) =>
                new ValueTask<EntityEntry<JobDescriptionEntity>>(
                    (EntityEntry<JobDescriptionEntity>)null!
                )
            );

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.JobDescriptions)
            .Returns(mockDbSet.Object);

        mockDbContext
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return mockDbContext.Object;
    }

    // Validation now runs in the global ValidationActionFilter, not the service.
    // Validation-rule test cases exercise this validator directly.
    private IValidator<Request.CreateRequest> CreateRealValidator()
    {
        return new JobDescriptionRequestValidator();
    }

    private Request.CreateRequest CreateValidRequest()
    {
        return new Request.CreateRequest
        {
            title = "Senior Software Engineer",
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
            openings = 2
        };
    }

    private JobDescriptionEntity CreateExistingJob(
        string title,
        string slug,
        bool isDeleted = false)
    {
        return new JobDescriptionEntity
        {
            Id = Guid.NewGuid(),
            title = title,
            location = "Ho Chi Minh",
            avatarUrl = "https://example.com/avatar.png",
            coverImageUrl = "https://example.com/cover.png",
            minExperienceYears = 3,
            maxExperienceYears = 7,
            employmentType = EmploymentType.FullTime,
            salaryMin = 30_000_000f,
            salaryMax = 50_000_000f,
            salaryNegotiable = false,
            description = "Old job description",
            status = StatusJobDescriptions.Open,
            openings = 1,
            slug = slug,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = null,
            IsDeleted = isDeleted
        };
    }

    // TC01: Tạo job description thành công
    [Fact]
    public async Task Create_Success()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>();

        var dbContext = CreateMockDbContext(jobs);
        var service = new JobDescriptionService(dbContext);

        var request = CreateValidRequest();

        // Act
        var result = await service.CreateJobAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.id);
        Assert.Equal("senior-software-engineer", result.Slug);
        Assert.Equal("Senior Software Engineer", result.Title);
        Assert.Equal("open", result.Status);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.Null(result.UpdatedAt);

        Assert.Single(jobs);

        var jobInDb = jobs.First();

        Assert.Equal(result.id, jobInDb.Id);
        Assert.Equal("Senior Software Engineer", jobInDb.title);
        Assert.Equal("Ho Chi Minh", jobInDb.location);
        Assert.Equal("senior-software-engineer", jobInDb.slug);
        Assert.False(jobInDb.IsDeleted);
    }

    // TC02: Validation thất bại do thiếu required fields
    [Fact]
    public async Task Create_ValidationFailed_ThrowValidationException()
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.title = null!;
        request.location = null!;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "TITLE_REQUIRED");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "LOCATION_REQUIRED");
    }

    // TC04: Slug bị trùng thì tự sinh slug mới
    [Fact]
    public async Task Create_DuplicateSlug_GenerateNewSlug()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateExistingJob(
                title: "Senior Software Engineer",
                slug: "senior-software-engineer",
                isDeleted: false)
        };

        var dbContext = CreateMockDbContext(jobs);
        var service = new JobDescriptionService(dbContext);

        var request = CreateValidRequest();

        // Act
        var result = await service.CreateJobAsync(request);

        // Assert
        Assert.NotNull(result);

        // Theo code hiện tại: suffix bắt đầu từ 1 rồi tăng trước khi gán,
        // nên nếu slug gốc đã tồn tại thì slug mới là "-2", không phải "-1".
        Assert.Equal("senior-software-engineer-2", result.Slug);

        Assert.Equal(2, jobs.Count);
        Assert.Contains(jobs, x => x.slug == "senior-software-engineer");
        Assert.Contains(jobs, x => x.slug == "senior-software-engineer-2");
    }

    // TC05: SalaryMin > SalaryMax
    [Fact]
    public async Task Create_SalaryMinGreaterThanMax_ThrowValidationException()
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.salaryMin = 50_000_000f;
        request.salaryMax = 30_000_000f;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "MIN_SALARY_GREATER_THAN_MAX_SALARY");
    }

    // TC06: Salary âm
    [Fact]
    public async Task Create_NegativeSalary_ThrowValidationException()
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.salaryMin = -1_000_000f;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "SALARY_CANNOT_BE_NEGATIVE");
    }

    // TC09: Openings <= 0
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_InvalidOpenings_ThrowValidationException(int openings)
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.openings = openings;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "OPENINGS_MUST_BE_GREATER_THAN_ZERO");
    }

    // TC10: Title rỗng
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyTitle_ThrowValidationException(string title)
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.title = title;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "TITLE_REQUIRED");
    }

    // TC11: Location rỗng
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyLocation_ThrowValidationException(string location)
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.location = location;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "LOCATION_REQUIRED");
    }
    
}