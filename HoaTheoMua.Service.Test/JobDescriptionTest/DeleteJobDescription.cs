using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JobDescriptions;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using RecruitmentRequestEntity = HoaTheoMua.Repository.Entity.RecruitmentRequest;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions.Service;

namespace HoaTheoMua.Service.Test.JobDescriptionTest;

public class DeleteJobDescriptionTest
{
    private AppDbContext CreateMockDbContext(
        List<JobDescriptionEntity> jobs,
        List<RecruitmentRequestEntity> recruitmentRequests)
    {
        var mockJobDbSet = jobs.BuildMockDbSet();
        var mockRecruitmentRequestDbSet = recruitmentRequests.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.JobDescriptions)
            .Returns(mockJobDbSet.Object);

        mockDbContext
            .Setup(x => x.RecruitmentRequests)
            .Returns(mockRecruitmentRequestDbSet.Object);

        mockJobDbSet
            .Setup(x => x.Update(It.IsAny<JobDescriptionEntity>()))
            .Callback((JobDescriptionEntity job) =>
            {
                var index = jobs.FindIndex(x => x.Id == job.Id);

                if (index >= 0)
                {
                    jobs[index] = job;
                }
            });

        mockDbContext
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return mockDbContext.Object;
    }

    private JobDescriptionService CreateService(AppDbContext dbContext)
    {
        return new JobDescriptionService(dbContext);
    }

    private JobDescriptionEntity CreateJob(
        Guid id,
        bool isDeleted = false)
    {
        return new JobDescriptionEntity
        {
            Id = id,
            title = "Senior Software Engineer",
            slug = "senior-software-engineer",
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
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = null,
            IsDeleted = isDeleted
        };
    }

    private RecruitmentRequestEntity CreateRecruitmentRequest(
        Guid jobDescriptionId,
        bool isDeleted = false)
    {
        var request = Activator.CreateInstance<RecruitmentRequestEntity>()!;

        SetPropertyIfExists(request, "Id", Guid.NewGuid());
        SetPropertyIfExists(request, "JobDescriptionId", jobDescriptionId);
        SetPropertyIfExists(request, "IsDeleted", isDeleted);
        SetPropertyIfExists(request, "CreatedAt", DateTimeOffset.UtcNow.AddDays(-1));
        SetPropertyIfExists(request, "UpdatedAt", null);

        return request;
    }

    private void SetPropertyIfExists(
        object obj,
        string propertyName,
        object? value)
    {
        var property = obj.GetType().GetProperty(propertyName);

        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }

    // TC01: Xóa job thành công
    [Fact]
    public async Task Delete_Success()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(jobId, isDeleted: false)
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var dbContext = CreateMockDbContext(jobs, recruitmentRequests);
        var service = CreateService(dbContext);

        // Act
        await service.DeleteJobAsync(jobId);

        // Assert
        var job = jobs.First(x => x.Id == jobId);

        Assert.True(job.IsDeleted);
        Assert.NotNull(job.UpdatedAt);
    }

    // TC02: Job có ứng viên / recruitment request
    [Fact]
    public async Task Delete_JobHasCandidates_ThrowConflict()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(jobId, isDeleted: false)
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            CreateRecruitmentRequest(jobId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(jobs, recruitmentRequests);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.DeleteJobAsync(jobId)
        );

        // Assert
        Assert.Equal("CANNOT_DELETE_JOB_HAS_CANDIDATES", exception.Message);

        var job = jobs.First(x => x.Id == jobId);

        Assert.False(job.IsDeleted);
        Assert.Null(job.UpdatedAt);
    }

    // TC03: ID không hợp lệ
    [Fact]
    public async Task Delete_EmptyId_ThrowBadRequest()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>();
        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var dbContext = CreateMockDbContext(jobs, recruitmentRequests);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.DeleteJobAsync(Guid.Empty)
        );

        // Assert
        Assert.Equal("INVALID_JOB_ID", exception.Message);
    }

    // TC04: Không tìm thấy job
    [Fact]
    public async Task Delete_NotFound_ThrowNotFound()
    {
        // Arrange
        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(Guid.NewGuid(), isDeleted: false)
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var dbContext = CreateMockDbContext(jobs, recruitmentRequests);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteJobAsync(Guid.NewGuid())
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND", exception.Message);
    }

    // TC06: Job đã bị soft delete trước đó
    [Fact]
    public async Task Delete_AlreadyDeleted_ThrowNotFound()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(jobId, isDeleted: true)
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>();

        var dbContext = CreateMockDbContext(jobs, recruitmentRequests);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteJobAsync(jobId)
        );

        // Assert
        Assert.Equal("JOB_DESCRIPTION_NOT_FOUND", exception.Message);
    }

    // TC07: Job có recruitment request đã soft delete vẫn không được xóa
    // Vì service hiện tại chỉ check JobDescriptionId, không filter IsDeleted của RecruitmentRequests
    [Fact]
    public async Task Delete_JobHasDeletedRecruitmentRequest_ThrowConflict()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        var jobs = new List<JobDescriptionEntity>
        {
            CreateJob(jobId, isDeleted: false)
        };

        var recruitmentRequests = new List<RecruitmentRequestEntity>
        {
            CreateRecruitmentRequest(jobId, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(jobs, recruitmentRequests);
        var service = CreateService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.DeleteJobAsync(jobId)
        );

        // Assert
        Assert.Equal("CANNOT_DELETE_JOB_HAS_CANDIDATES", exception.Message);

        var job = jobs.First(x => x.Id == jobId);

        Assert.False(job.IsDeleted);
        Assert.Null(job.UpdatedAt);
    }
}