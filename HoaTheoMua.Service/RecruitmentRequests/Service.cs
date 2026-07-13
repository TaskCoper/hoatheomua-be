using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.RecruitmentRequests;

public class Service : IService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public Service(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BasePaginationResponse> GetPagedRecruitmentRequestsAsync(int pageIndex, int pageSize,
        bool? isRead)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = _context.RecruitmentRequests
            .Include(r => r.JobDescription)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (isRead.HasValue)
        {
            query = query.Where(r => r.isRead == isRead.Value);
        }

        var totalCount = await query.CountAsync();

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.RecruitmentRequestItem
            {
                id = x.Id,
                title = x.title,
                coverLetter = x.coverLetter,
                attachments = x.attachments,
                isRead = x.isRead,
                createdAt = x.CreatedAt,
                user = new Response.UserDto
                {
                    userId = x.UserId,
                    fullName = x.AuthorName,
                    email = x.AuthorEmail,
                    phoneNumber = null,
                    avatar = x.AuthorAvatar
                },
                jobDescription = new Response.JobDto
                {
                    id = x.JobDescription.Id,
                    title = x.JobDescription.title
                }
            })
            .ToListAsync();

        var items = requests.ToList();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<BasePaginationResponse> GetMyRecruitmentRequestsAsync(int pageIndex, int pageSize)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var userId = _currentUser.GetRequiredUserId();

        var query = _context.RecruitmentRequests
            .Include(r => r.JobDescription)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.MyRecruitmentRequestItem
            {
                id = x.Id,
                title = x.title,
                coverLetter = x.coverLetter,
                attachments = x.attachments,
                isRead = x.isRead,
                createdAt = x.CreatedAt,
                jobDescription = new Response.JobDto
                {
                    id = x.JobDescription.Id,
                    title = x.JobDescription.title
                }
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<BaseResponse> GetRecruitmentRequestByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new Exceptions.BadRequestException("INVALID_RECRUITMENT_REQUEST_ID");
        }

        var request = await _context.RecruitmentRequests
            .Include(r => r.JobDescription)
            .Where(a => a.Id == id && !a.IsDeleted)
            .Select(x => new Response.RecruitmentRequestItem
            {
                id = x.Id,
                title = x.title,
                coverLetter = x.coverLetter,
                attachments = x.attachments,
                isRead = x.isRead,
                createdAt = x.CreatedAt,
                user = new Response.UserDto
                {
                    userId = x.UserId,
                    fullName = x.AuthorName,
                    email = x.AuthorEmail,
                    phoneNumber = null,
                    avatar = x.AuthorAvatar
                },
                jobDescription = new Response.JobDto
                {
                    id = x.JobDescription.Id,
                    title = x.JobDescription.title
                }
            })
            .FirstOrDefaultAsync();

        if (request == null)
        {
            throw new Exceptions.NotFoundException("RECRUITMENT_REQUEST_NOT_FOUND");
        }

        return ApiResponseFactory.Base(request);
    }

    public async Task<BaseResponse> MarkAsReadAsync(Guid id)
    {
        var request = await _context.RecruitmentRequests.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (request == null)
        {
            throw new Exceptions.NotFoundException("RECRUITMENT_REQUEST_NOT_FOUND");
        }

        request.isRead = true;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        _context.RecruitmentRequests.Update(request);
        await _context.SaveChangesAsync();

        var response = new Response.MarkAsReadResponse
        {
            id = request.Id,
            isRead = request.isRead,
            updatedAt = request.UpdatedAt
        };

        return ApiResponseFactory.Base(response);
    }

    public async Task<Response.CreateRecruitmentResponse> CreateRecruitmentRequestAsync(
        Request.CreateRecruitmentRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        var jobExistsAndOpen = await _context.JobDescriptions.AnyAsync(j =>
            j.Id == request.jobDescriptionId &&
            !j.IsDeleted &&
            j.status == Repository.Enum.StatusJobDescriptions.Open);

        if (!jobExistsAndOpen)
        {
            throw new Exceptions.NotFoundException("JOB_DESCRIPTION_NOT_FOUND_OR_NOT_OPEN");
        }

        bool isSpam = await _context.RecruitmentRequests.AnyAsync(r =>
            r.UserId == userId &&
            r.JobDescriptionId == request.jobDescriptionId &&
            !r.isRead);

        if (isSpam)
        {
            throw new Exceptions.TooManyRequestException("APPLICATION_ALREADY_SUBMITTED_AND_PENDING");
        }

        var newRequest = new Repository.Entity.RecruitmentRequest
        {
            JobDescriptionId = request.jobDescriptionId,
            UserId = userId,
            // Author profile snapshot from the JWT claims.
            AuthorName = _currentUser.FullName,
            AuthorEmail = _currentUser.Email,
            AuthorAvatar = _currentUser.Avatar,
            title = request.title,
            coverLetter = request.coverLetter,
            attachments = request.attachments ?? new List<string>(),
            isRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await _context.RecruitmentRequests.AddAsync(newRequest);
        await _context.SaveChangesAsync();

        var responseData = new Response.CreateRecruitmentResponse
        {
            id = newRequest.Id,
            jobDescriptionId = newRequest.JobDescriptionId,
            title = newRequest.title,
            attachments = newRequest.attachments,
            isRead = newRequest.isRead,
            createdAt = newRequest.CreatedAt
        };

        return responseData;
    }
}