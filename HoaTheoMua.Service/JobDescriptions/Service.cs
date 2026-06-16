using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.JobDescriptions;

public class Service : IService
{
    private readonly AppDbContext _context;

    public Service(AppDbContext context)
    {
        _context = context;
    }

    public static string GenerateSlug(string phrase)
    {
        if (string.IsNullOrEmpty(phrase)) return string.Empty;

        string str = phrase.ToLowerInvariant();

        string[] arr1 = new string[]
        {
            "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ", "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
            "í", "ì", "ỉ", "ĩ", "ị", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ",
            "ợ",
            "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ý", "ỳ", "ỷ", "ỹ", "ỵ"
        };
        string[] arr2 = new string[]
        {
            "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
            "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
            "o",
            "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y", "y", "y", "y", "y"
        };

        for (int i = 0; i < arr1.Length; i++)
        {
            str = str.Replace(arr1[i], arr2[i]);
        }

        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = str.Replace(" ", "-");

        return str;
    }

    public async Task<Response.CreateResponse> CreateJobAsync(Request.CreateRequest request)
    {
        ValidateJobRequest(request);

        string finalSlug = string.IsNullOrWhiteSpace(request.slug)
            ? GenerateSlug(request.title)
            : GenerateSlug(request.slug);

        bool isSlugExists = await _context.JobDescriptions
            .AnyAsync(x => x.slug == finalSlug);

        if (isSlugExists)
        {
            finalSlug = $"{finalSlug}-{DateTimeOffset.UtcNow.Ticks % 10000}";
        }

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

    public async Task<BaseResponse> GetJobByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("INVALID_JOB_ID");
        }

        var query = _context.JobDescriptions
            .AsNoTracking()
            .Where(job => !job.IsDeleted);

        var job = await query.FirstOrDefaultAsync(job => job.Id == id);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }
        var responseData = MapToDetailResponse(job);
        
        return ApiResponseFactory.Base(responseData);
    }

    public async Task<BaseResponse> GetPublicJobByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("INVALID_JOB_ID");
        }

        var job = await _context.JobDescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(job => job.Id == id && !job.IsDeleted && job.status == StatusJobDescriptions.Open);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        var responseData = MapToDetailResponse(job);
        return ApiResponseFactory.Base(responseData);
    }

    public async Task<BaseResponse> GetJobBySlugAsync(string slug)
    {
        var query = _context.JobDescriptions
            .AsNoTracking()
            .Where(job => !job.IsDeleted && job.status == Repository.Enum.StatusJobDescriptions.Open);

        var job = await query.FirstOrDefaultAsync(job => job.slug == slug);

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

        ValidateJobRequest(request);

        var job = await _context.JobDescriptions
            .Where(j => !j.IsDeleted)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null)
        {
            throw new NotFoundException("JOB_DESCRIPTION_NOT_FOUND");
        }

        if (!string.IsNullOrWhiteSpace(request.slug) && request.slug != job.slug)
        {
            string finalSlug = GenerateSlug(request.slug);
            bool isSlugExists = await _context.JobDescriptions
                .AnyAsync(x => x.slug == finalSlug && x.Id != id);

            if (isSlugExists)
            {
                finalSlug = $"{finalSlug}-{DateTimeOffset.UtcNow.Ticks % 10000}";
            }
            job.slug = finalSlug;
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

    private static void ValidateJobRequest(Request.CreateRequest request)
    {
        if (request.minExperienceYears.HasValue && request.minExperienceYears.Value < 0 ||
            request.maxExperienceYears.HasValue && request.maxExperienceYears.Value < 0)
        {
            throw new BadRequestException("EXPERIENCE_CANNOT_BE_NEGATIVE");
        }

        if (request.minExperienceYears.HasValue && request.maxExperienceYears.HasValue
                                                && request.minExperienceYears.Value > request.maxExperienceYears.Value)
        {
            throw new BadRequestException("MIN_EXPERIENCE_GREATER_THAN_MAX_EXPERIENCE");
        }

        ValidateSalaryRange(request.salaryMin, request.salaryMax);

        if (request.openings <= 0)
        {
            throw new BadRequestException("OPENINGS_MUST_BE_GREATER_THAN_ZERO");
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
            openings = job.openings
        };
    }
}