using System.Reflection;
using FluentValidation;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JobDescriptions.Validations;
using Microsoft.EntityFrameworkCore;
using Xunit;

using JobDescriptionEntity = HoaTheoMua.Repository.Entity.JobDescriptions;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions.Service;
using JobRequest = HoaTheoMua.Service.JobDescriptions.Request;

namespace HoaTheoMua.Service.Test.JobDescriptionTest;

public class UpdateJobDescriptionTest
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
        string location = "Hồ Chí Minh",
        string status = "Open",
        bool isDeleted = false)
    {
        var job = new JobDescriptionEntity
        {
            Id = id ?? JobId,
            slug = slug,
            title = title,
            location = location,
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

    private static JobRequest.UpdateRequest CreateValidRequest(
        string title = "Senior Software Engineer Updated",
        string location = "Hà Nội",
        string status = "Open",
        int openings = 3,
        float? salaryMin = 35000000f,
        float? salaryMax = 55000000f)
    {
        var request = new JobRequest.UpdateRequest
        {
            title = title,
            location = location,
            avatarUrl = "https://example.com/avatar-new.png",
            coverImageUrl = "https://example.com/cover-new.png",
            minExperienceYears = 3,
            maxExperienceYears = 7,
            employmentType = ParseEnum<EmploymentType>("FullTime"),
            salaryMin = salaryMin,
            salaryMax = salaryMax,
            salaryNegotiable = false,
            description = "Updated description...",
            status = ParseEnum<StatusJobDescriptions>(status),
            openings = openings
        };

        SetPropertyIfExists(request, "Level", ParseEnumIfExists("LevelJobDescriptions", "Senior"));

        return request;
    }

    [Fact]
    public async Task UpdateJob_Success()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateValidRequest();

        var result = await service.UpdateJobAsync(JobId, request);

        var value = GetValue(result);
        var jobInDb = await dbContext.JobDescriptions.FirstAsync(x => x.Id == JobId);

        Assert.NotNull(value);
        Assert.Equal("Senior Software Engineer Updated", jobInDb.title);
        Assert.Equal("Hà Nội", jobInDb.location);
        Assert.Equal("senior-software-engineer-updated", jobInDb.slug);
        Assert.Equal(3, jobInDb.openings);

        Assert.Equal("Senior Software Engineer Updated", GetString(value, "title"));
        Assert.Equal("senior-software-engineer-updated", GetString(value, "slug"));
    }

    [Fact]
    public async Task UpdateJob_EmptyId_ThrowBadRequest()
    {
        await using var dbContext = CreateDbContext();

        var service = CreateService(dbContext);
        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateJobAsync(Guid.Empty, request));

        Assert.Contains("INVALID_JOB_ID", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_ValidationFailed_ThrowValidation()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            title: "",
            location: "",
            openings: 1);

        request.avatarUrl = "";
        request.coverImageUrl = "";
        request.description = "";

        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("TITLE_REQUIRED", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_NotFound_ThrowNotFound()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateJobAsync(NotFoundId, request));

        Assert.Contains("JOB_DESCRIPTION_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_SalaryMinGreaterThanSalaryMax_ThrowValidation()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            salaryMin: 50000000f,
            salaryMax: 30000000f);

        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("MIN_SALARY_GREATER_THAN_MAX_SALARY", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_TitleChanged_RegenerateSlug()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            title: "Junior Software Engineer");

        var result = await service.UpdateJobAsync(JobId, request);

        var value = GetValue(result);
        var jobInDb = await dbContext.JobDescriptions.FirstAsync(x => x.Id == JobId);

        Assert.NotNull(value);
        Assert.Equal("Junior Software Engineer", jobInDb.title);
        Assert.Equal("junior-software-engineer", jobInDb.slug);
        Assert.Equal("junior-software-engineer", GetString(value, "slug"));
    }

    [Fact]
    public async Task UpdateJob_SameTitle_KeepOldSlug()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(
            CreateJob(
                title: "Senior Software Engineer",
                slug: "senior-software-engineer"));

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            title: "Senior Software Engineer");

        var result = await service.UpdateJobAsync(JobId, request);

        var value = GetValue(result);
        var jobInDb = await dbContext.JobDescriptions.FirstAsync(x => x.Id == JobId);

        Assert.NotNull(value);
        Assert.Equal("Senior Software Engineer", jobInDb.title);
        Assert.Equal("senior-software-engineer", jobInDb.slug);
        Assert.Equal("senior-software-engineer", GetString(value, "slug"));
    }

    [Fact]
    public async Task UpdateJob_StatusOpenToClosed_Success()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(
            CreateJob(status: "Open"));

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            status: "Closed");

        var result = await service.UpdateJobAsync(JobId, request);

        var value = GetValue(result);
        var jobInDb = await dbContext.JobDescriptions.FirstAsync(x => x.Id == JobId);

        Assert.NotNull(value);
        Assert.Equal(ParseEnum<StatusJobDescriptions>("Closed"), jobInDb.status);
        Assert.Equal("Closed", jobInDb.status.ToString());
    }

    [Fact]
    public async Task UpdateJob_OpeningsZero_ThrowValidation()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            openings: 0);

        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("OPENINGS_MUST_BE_GREATER_THAN_ZERO", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_AlreadyDeleted_ThrowNotFound()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(
            CreateJob(isDeleted: true));

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("JOB_DESCRIPTION_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_EmptyTitle_ThrowValidation()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            title: "");

        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("TITLE_REQUIRED", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_WhitespaceTitle_ThrowValidation()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            title: "   ");

        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("TITLE_REQUIRED", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_NegativeSalary_ThrowValidation()
    {
        await using var dbContext = CreateDbContext();

        dbContext.JobDescriptions.Add(CreateJob());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var request = CreateValidRequest(
            salaryMin: -1000000f,
            salaryMax: 30000000f);

        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => service.UpdateJobAsync(JobId, request));

        Assert.Contains("SALARY_CANNOT_BE_NEGATIVE", ex.Message);
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
            if (value is DateTimeOffset dto)
            {
                property.SetValue(obj, dto);
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
}