using System.Reflection;
using FluentValidation;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JobDescriptions.Validations;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static HoaTheoMua.Service.Test.TestSupport.ReflectionHelper;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions.Service;

namespace HoaTheoMua.Service.Test.JobDescriptionTest;

public class GetPublicJobDescriptionByIdOrSlugTest
{
    private static readonly Guid JobId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid NotFoundId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-06-23T08:51:30.37969Z");

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static JobDescriptionService CreateService(AppDbContext dbContext)
    {
        IValidator<HoaTheoMua.Service.JobDescriptions.Request.CreateRequest> validator =
            new JobDescriptionRequestValidator();

        return new JobDescriptionService(dbContext, validator);
    }

    private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new InvalidOperationException(
            $"Enum {typeof(TEnum).Name} không có giá trị: {value}");
    }

    private static JobDescriptionEntity CreateJob(
        Guid? id = null,
        string slug = "senior-software-engineer",
        string title = "Senior Software Engineer",
        string status = "Open",
        bool isDeleted = false)
    {
        var job = new JobDescriptionEntity
        {
            Id = id ?? JobId,
            slug = slug,
            title = title,
            location = "Hồ Chí Minh",
            avatarUrl = "https://example.com/avatar.png",
            coverImageUrl = "https://example.com/cover.png",
            employmentType = ParseEnum<EmploymentType>("FullTime"),
            salaryNegotiable = false,
            description = "We are looking for a senior software engineer...",
            status = ParseEnum<StatusJobDescriptions>(status),
            openings = 2
        };

        SetPropertyIfExists(job, "MinExperienceYears", 3f);
        SetPropertyIfExists(job, "MaxExperienceYears", 7f);
        SetPropertyIfExists(job, "Level", "Senior");
        SetPropertyIfExists(job, "SalaryMin", 30000000f);
        SetPropertyIfExists(job, "SalaryMax", 50000000f);
        SetPropertyIfExists(job, "IsDeleted", isDeleted);
        SetPropertyIfExists(job, "CreatedAt", Now);
        SetPropertyIfExists(job, "UpdatedAt", Now);

        return job;
    }

    [Fact]
    public async Task GetPublicJob_ById_Success()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPublicJobByIdOrSlugAsync(JobId.ToString());

        // Assert
        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(JobId, GetGuid(value, "id"));
        Assert.Equal("senior-software-engineer", GetString(value, "slug"));
        Assert.Equal("Senior Software Engineer", GetString(value, "title"));
        Assert.Equal("Hồ Chí Minh", GetString(value, "location"));
    }

    [Fact]
    public async Task GetPublicJob_BySlug_Success()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        // Act
        var result = await service.GetPublicJobByIdOrSlugAsync("senior-software-engineer");

        // Assert
        var value = GetValue(result);

        Assert.NotNull(value);
        Assert.Equal(JobId, GetGuid(value, "id"));
        Assert.Equal("senior-software-engineer", GetString(value, "slug"));
        Assert.Equal("Senior Software Engineer", GetString(value, "title"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("senior developer")]
    [InlineData("senior@developer!")]
    public async Task GetPublicJob_InvalidKey_ThrowBadRequest(string key)
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        // Act
        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetPublicJobByIdOrSlugAsync(key));

        // Assert
        Assert.Contains("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", ex.Message);
    }

    [Fact]
    public async Task GetPublicJob_KeyTooLong_ThrowBadRequest()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var key = new string('a', 301);

        // Act
        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetPublicJobByIdOrSlugAsync(key));

        // Assert
        Assert.Contains("JOB_DESCRIPTION_LOOKUP_KEY_INVALID", ex.Message);
    }

    [Fact]
    public async Task GetPublicJob_NotFound_ThrowNotFound()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetPublicJobByIdOrSlugAsync("backend-developer"));

        // Assert
        Assert.Contains("JOB_DESCRIPTION_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task GetPublicJob_GuidNotFound_ThrowNotFound()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetPublicJobByIdOrSlugAsync(NotFoundId.ToString()));

        // Assert
        Assert.Contains("JOB_DESCRIPTION_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task GetPublicJob_DeletedJob_ThrowNotFound()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(
            CreateJob(
                slug: "deleted-job",
                isDeleted: true));

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetPublicJobByIdOrSlugAsync("deleted-job"));

        // Assert
        Assert.Contains("JOB_DESCRIPTION_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task GetPublicJob_NotOpenStatus_ThrowNotFound()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(
            CreateJob(
                slug: "closed-job",
                status: "Closed"));

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetPublicJobByIdOrSlugAsync("closed-job"));

        // Assert
        Assert.Contains("JOB_DESCRIPTION_NOT_FOUND", ex.Message);
    }

    // SetPropertyIfExists moved to TestSupport/ReflectionHelper.cs (using static)

    private static object? GetValue(object result)
    {
        return GetProperty(result, "value") ?? GetProperty(result, "Value");
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

    private static string? GetString(object? obj, string propertyName)
    {
        return GetProperty(obj, propertyName)?.ToString();
    }

    private static Guid GetGuid(object? obj, string propertyName)
    {
        var value = GetProperty(obj, propertyName);

        if (value is Guid guid)
        {
            return guid;
        }

        return Guid.Parse(value!.ToString()!);
    }
}