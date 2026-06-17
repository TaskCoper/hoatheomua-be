using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Utils;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.JobDescriptions;

public class Service : IService
{
    private readonly AppDbContext _context;
    private readonly IValidator<Request.CreateRequest> _validator;

    public Service(AppDbContext context, IValidator<Request.CreateRequest> validator)
    {
        _context = context;
        _validator = validator;
    }


    public async Task<Response.CreateResponse> CreateJobAsync(Request.CreateRequest request)
    {
        await ValidateJobRequestAsync(request);

        string finalSlug = await GenerateUniqueSlugAsync(request.title);

        var newJob = new Repository.Entity.JobDescriptions()
        {
            title = request.title,
            location = request.location,
            avatarUrl = request.avatarUrl,
            coverImageUrl = request.coverImageUrl,
            minExperienceYears = request.minExperienceYears,
            maxExperienceYears = request.maxExperienceYears,
            level = request.level,
            employmentType = request.employmentType,
            salaryMin = request.salaryMin,
            salaryMax = request.salaryMax,
            salaryNegotiable = request.salaryNegotiable,
            description = request.description,
            status = request.status,
            openings = request.openings,
            slug = finalSlug,

            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await _context.JobDescriptions.AddAsync(newJob);
        await _context.SaveChangesAsync();

        var responseData = new Response.CreateResponse
        {
            id = newJob.Id,
            slug = newJob.slug,
            title = newJob.title,
            status = JsonNamingPolicy.CamelCase.ConvertName(newJob.status.ToString()),
            createdAt = newJob.CreatedAt,
            updatedAt = newJob.UpdatedAt
        };
        return responseData;
    }

    public async Task<BasePaginationResponse> GetPagedJobsAsync(int page, int pageSize, string? title, string? location,
        EmploymentType? employmentType, float? salaryMin, float? salaryMax, StatusJobDescriptions? status = null)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        ValidateSalaryRange(salaryMin, salaryMax);

        var query = _context.JobDescriptions
            .AsNoTracking()
            .Where(job => !job.IsDeleted);

        if (!string.IsNullOrWhiteSpace(title))
        {
            var searchTitle = title.Trim().ToLower();
            query = query.Where(job => job.title.ToLower().Contains(searchTitle));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var searchLocation = location.Trim().ToLower();
            query = query.Where(job => job.location.ToLower().Contains(searchLocation));
        }

        if (employmentType.HasValue)
        {
            query = query.Where(job => job.employmentType == employmentType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(job => job.status == status.Value);
        }

        if (salaryMin.HasValue)
        {
            query = query.Where(job => job.salaryNegotiable || job.salaryMin >= salaryMin.Value);
        }

        if (salaryMax.HasValue)
        {
            query = query.Where(job => job.salaryNegotiable || job.salaryMax <= salaryMax.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(job => job.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(job => MapToResponse(job))
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, page, pageSize, totalCount);
    }

    public async Task<BaseResponse> GetJobByIdOrSlugAsync(string idOrSlug)
    {
        var job = await GetJobByIdOrSlugQuery(idOrSlug)
            .FirstOrDefaultAsync();

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        var responseData = MapToDetailResponse(job);
        return ApiResponseFactory.Base(responseData);
    }

    public async Task<BaseResponse> GetPublicJobByIdOrSlugAsync(string idOrSlug)
    {
        var normalizedKey = NormalizeLookupKey(idOrSlug);
        var query = _context.JobDescriptions
            .AsNoTracking()
            .Where(job => !job.IsDeleted && job.status == StatusJobDescriptions.Open);

        var job = Guid.TryParse(normalizedKey, out var id)
            ? await query.FirstOrDefaultAsync(job => job.Id == id)
            : await query.FirstOrDefaultAsync(job => job.slug == normalizedKey);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        var responseData = MapToDetailResponse(job);
        return ApiResponseFactory.Base(responseData);
    }

    public async Task<BaseResponse> UpdateJobAsync(Guid id, Request.UpdateRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("INVALID_JOB_ID");
        }

        await ValidateJobRequestAsync(request);

        var job = await _context.JobDescriptions
            .Where(j => !j.IsDeleted)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        if (!string.Equals(request.title, job.title, StringComparison.Ordinal))
        {
            job.slug = await GenerateUniqueSlugAsync(request.title, id);
        }

        job.title = request.title;
        job.location = request.location;
        job.avatarUrl = request.avatarUrl;
        job.coverImageUrl = request.coverImageUrl;
        job.minExperienceYears = request.minExperienceYears;
        job.maxExperienceYears = request.maxExperienceYears;
        job.level = request.level;
        job.employmentType = request.employmentType;
        job.salaryMin = request.salaryMin;
        job.salaryMax = request.salaryMax;
        job.salaryNegotiable = request.salaryNegotiable;
        job.description = request.description;
        job.status = request.status;
        job.openings = request.openings;
        job.UpdatedAt = DateTimeOffset.UtcNow;

        _context.JobDescriptions.Update(job);
        await _context.SaveChangesAsync();

        var responseData = MapToDetailResponse(job);
        return ApiResponseFactory.Base(responseData);
    }

    public async Task DeleteJobAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("INVALID_JOB_ID");
        }

        var job = await _context.JobDescriptions
            .Where(j => !j.IsDeleted)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        bool hasCandidates = await _context.RecruitmentRequests
            .AnyAsync(r => r.JobDescriptionId == id);

        if (hasCandidates)
        {
            throw new ConflictException("CANNOT_DELETE_JOB_HAS_CANDIDATES");
        }

        job.IsDeleted = true;
        job.UpdatedAt = DateTimeOffset.UtcNow;
        _context.JobDescriptions.Update(job);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Repository.Entity.JobDescriptions> GetJobByIdOrSlugQuery(string idOrSlug)
    {
        var normalizedKey = NormalizeLookupKey(idOrSlug);
        var query = _context.JobDescriptions
            .AsNoTracking()
            .Where(job => !job.IsDeleted);

        return Guid.TryParse(normalizedKey, out var id)
            ? query.Where(job => job.Id == id)
            : query.Where(job => job.slug == normalizedKey);
    }

    private static string NormalizeLookupKey(string key)
    {
        var normalizedKey = Uri.UnescapeDataString(key.Trim());

        if (string.IsNullOrWhiteSpace(normalizedKey) || normalizedKey.Length > 300)
        {
            throw new BadRequestException("JOB_DESCRIPTION_LOOKUP_KEY_INVALID");
        }

        if (Guid.TryParse(normalizedKey, out var id))
        {
            if (id == Guid.Empty)
            {
                throw new BadRequestException("JOB_DESCRIPTION_LOOKUP_KEY_INVALID");
            }

            return normalizedKey;
        }

        normalizedKey = normalizedKey.ToLowerInvariant();

        if (!Regex.IsMatch(normalizedKey, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new BadRequestException("JOB_DESCRIPTION_LOOKUP_KEY_INVALID");
        }

        return normalizedKey;
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, Guid? excludeId = null)
    {
        var baseSlug = Slug.GenerateSlug(title);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            throw new BadRequestException("INVALID_JOB_TITLE");
        }

        var finalSlug = baseSlug;
        var suffix = 1;

        while (await _context.JobDescriptions.AnyAsync(job =>
                   job.slug == finalSlug && (!excludeId.HasValue || job.Id != excludeId.Value)))
        {
            suffix++;
            finalSlug = $"{baseSlug}-{suffix}";
        }

        return finalSlug;
    }

    private async Task ValidateJobRequestAsync(Request.CreateRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationExceptionError(validationResult.Errors.First().ErrorMessage);
        }
    }

    private static void ValidateSalaryRange(float? salaryMin, float? salaryMax)
    {
        if (salaryMin.HasValue && salaryMin.Value < 0 ||
            salaryMax.HasValue && salaryMax.Value < 0)
        {
            throw new BadRequestException("SALARY_CANNOT_BE_NEGATIVE");
        }

        if (salaryMin.HasValue && salaryMax.HasValue && salaryMin.Value > salaryMax.Value)
        {
            throw new BadRequestException("MIN_SALARY_GREATER_THAN_MAX_SALARY");
        }
    }

    private Response.JobDetailResponse MapToDetailResponse(Repository.Entity.JobDescriptions job)
    {
        return new Response.JobDetailResponse
        {
            id = job.Id,
            slug = job.slug,
            title = job.title,
            location = job.location,
            avatarUrl = job.avatarUrl,
            coverImageUrl = job.coverImageUrl,
            minExperienceYears = job.minExperienceYears,
            maxExperienceYears = job.maxExperienceYears,

            // Chuyển đổi Enum PascalCase sang chuỗi camelCase chuẩn JSON
            level = JsonNamingPolicy.CamelCase.ConvertName(job.level.ToString()),
            employmentType = JsonNamingPolicy.CamelCase.ConvertName(job.employmentType.ToString()),
            status = JsonNamingPolicy.CamelCase.ConvertName(job.status.ToString()),

            salaryMin = job.salaryMin,
            salaryMax = job.salaryMax,
            salaryNegotiable = job.salaryNegotiable,
            openings = job.openings,
            description = job.description,
            createdAt = job.CreatedAt,
            updatedAt = job.UpdatedAt ?? job.CreatedAt
        };
    }

    private static Response.JobListItem MapToResponse(Repository.Entity.JobDescriptions job)
    {
        return new Response.JobListItem
        {
            id = job.Id,
            slug = job.slug,
            title = job.title,
            avatarUrl = job.avatarUrl,
            location = job.location,
            employmentType = JsonNamingPolicy.CamelCase.ConvertName(job.employmentType.ToString()),
            salaryMin = job.salaryMin,
            salaryMax = job.salaryMax,
            salaryNegotiable = job.salaryNegotiable,
            openings = job.openings,
            status = JsonNamingPolicy.CamelCase.ConvertName(job.status.ToString())
        };
    }
}