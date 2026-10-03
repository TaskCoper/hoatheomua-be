using System.Text.Json;
using System.Text.RegularExpressions;
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

    public Service(AppDbContext context)
    {
        _context = context;
    }


    public async Task<Response.CreateResponse> CreateJobAsync(Request.CreateRequest request)
    {
        string finalSlug = await GenerateUniqueSlugAsync(request.title);

        var newJob = new Repository.Entity.JobDescriptions()
        {
            title = request.title,
            location = request.location,
            avatarUrl = request.avatarUrl,
            coverImageUrl = request.coverImageUrl,
            minExperienceYears = request.minExperienceYears,
            maxExperienceYears = request.maxExperienceYears,
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
            Slug = newJob.slug,
            Title = newJob.title,
            Status = JsonNamingPolicy.CamelCase.ConvertName(newJob.status.ToString()),
            CreatedAt = newJob.CreatedAt,
            UpdatedAt = newJob.UpdatedAt
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

        if (salaryMin.HasValue || salaryMax.HasValue)
        {
            var filterMin = salaryMin ?? 0;
            var filterMax = salaryMax ?? float.MaxValue;

            query = query.Where(job =>
                (job.salaryMin ?? 0) <= filterMax && (job.salaryMax ?? float.MaxValue) >= filterMin);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(job => job.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(job => new Response.JobListItem
            {
                Id = job.Id,
                Slug = job.slug,
                Title = job.title,
                Description = job.description,
                CoverImageUrl = job.coverImageUrl,
                AvatarUrl = job.avatarUrl,
                Location = job.location,
                EmploymentType = job.employmentType == EmploymentType.FullTime ? "fullTime" : "partTime",
                SalaryMin = job.salaryMin,
                SalaryMax = job.salaryMax,
                SalaryNegotiable = job.salaryNegotiable,
                Openings = job.openings,
                Status = job.status == StatusJobDescriptions.Open ? "open" : "closed",
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, page, pageSize, totalCount);
    }

    public async Task<BaseResponse> GetJobByIdOrSlugAsync(string idOrSlug)
    {
        var responseData = await GetJobByIdOrSlugQuery(idOrSlug)
            .Select(ProjectToDetail)
            .FirstOrDefaultAsync();

        if (responseData is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        return ApiResponseFactory.Base(responseData);
    }

    public async Task<BaseResponse> GetPublicJobByIdOrSlugAsync(string idOrSlug)
    {
        var normalizedKey = NormalizeLookupKey(idOrSlug);
        var query = _context.JobDescriptions
            .AsNoTracking()
            .Where(job => !job.IsDeleted && job.status == StatusJobDescriptions.Open);

        var responseData = Guid.TryParse(normalizedKey, out var id)
            ? await query.Where(job => job.Id == id).Select(ProjectToDetail).FirstOrDefaultAsync()
            : await query.Where(job => job.slug == normalizedKey).Select(ProjectToDetail).FirstOrDefaultAsync();

        if (responseData is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        return ApiResponseFactory.Base(responseData);
    }

    public async Task<BaseResponse> GetHighestSalaryAsync()
    {
        var highestSalary = await _context.JobDescriptions
            .AsNoTracking()
            .Where(job => job.salaryMax.HasValue)
            .MaxAsync(job => (float?)job.salaryMax);

        return ApiResponseFactory.Base(new Response.HighestSalaryResponse
        {
            Salary = highestSalary
        });
    }

    public async Task<BaseResponse> UpdateJobAsync(string identifier, Request.UpdateRequest request)
    {
        var normalizedIdentifier = NormalizeLookupKey(identifier);
        var query = _context.JobDescriptions.Where(job => !job.IsDeleted);

        var job = Guid.TryParse(normalizedIdentifier, out var id)
            ? await query.FirstOrDefaultAsync(job => job.Id == id)
            : await query.FirstOrDefaultAsync(job => job.slug == normalizedIdentifier);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        job.title = request.title;
        job.location = request.location;
        job.avatarUrl = request.avatarUrl;
        job.coverImageUrl = request.coverImageUrl;
        job.minExperienceYears = request.minExperienceYears;
        job.maxExperienceYears = request.maxExperienceYears;
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

        var responseData = new Response.JobDetailResponse
        {
            Id = job.Id,
            Slug = job.slug,
            Title = job.title,
            Location = job.location,
            AvatarUrl = job.avatarUrl,
            CoverImageUrl = job.coverImageUrl,
            MinExperienceYears = job.minExperienceYears,
            MaxExperienceYears = job.maxExperienceYears,
            EmploymentType = job.employmentType == EmploymentType.FullTime ? "fullTime" : "partTime",
            SalaryMin = job.salaryMin,
            SalaryMax = job.salaryMax,
            SalaryNegotiable = job.salaryNegotiable,
            Openings = job.openings,
            Description = job.description,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt ?? job.CreatedAt,
            Status = job.status == StatusJobDescriptions.Open ? "open" : "closed"
        };
        return ApiResponseFactory.Base(responseData);
    }
    

    public async Task<Response.DeleteResponse> DeleteJobAsync(string identifier)
    {
        var normalizedIdentifier = NormalizeLookupKey(identifier);
        var query = _context.JobDescriptions.Where(job => !job.IsDeleted);

        var job = Guid.TryParse(normalizedIdentifier, out var id)
            ? await query.FirstOrDefaultAsync(job => job.Id == id)
            : await query.FirstOrDefaultAsync(job => job.slug == normalizedIdentifier);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        bool hasCandidates = await _context.RecruitmentRequests
            .AnyAsync(r => r.JobDescriptionId == job.Id);

        if (hasCandidates)
        {
            throw new ConflictException("CANNOT_DELETE_JOB_HAS_CANDIDATES");
        }

        job.IsDeleted = true;
        job.UpdatedAt = DateTimeOffset.UtcNow;
        _context.JobDescriptions.Update(job);
        await _context.SaveChangesAsync();

        return new Response.DeleteResponse()
        {
            Id = job.Id,
            Slug = job.slug,
        };
    }
    

    public async Task<List<string>> GetAllLocation()
    {
        var locations = await _context.JobDescriptions
            .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.location))
            .Select(x => x.location)
            .Distinct()
            .ToListAsync();
        return locations;
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


    private static readonly System.Linq.Expressions.Expression<Func<Repository.Entity.JobDescriptions, Response.JobDetailResponse>> ProjectToDetail = job => new Response.JobDetailResponse
    {
        Id = job.Id,
        Slug = job.slug,
        Title = job.title,
        Location = job.location,
        AvatarUrl = job.avatarUrl,
        CoverImageUrl = job.coverImageUrl,
        MinExperienceYears = job.minExperienceYears,
        MaxExperienceYears = job.maxExperienceYears,
        EmploymentType = job.employmentType == EmploymentType.FullTime ? "fullTime" : "partTime",
        SalaryMin = job.salaryMin,
        SalaryMax = job.salaryMax,
        SalaryNegotiable = job.salaryNegotiable,
        Openings = job.openings,
        Description = job.description,
        Status = job.status == StatusJobDescriptions.Open ? "open" : "closed",
        CreatedAt = job.CreatedAt,
        UpdatedAt = job.UpdatedAt ?? job.CreatedAt
    };

}
