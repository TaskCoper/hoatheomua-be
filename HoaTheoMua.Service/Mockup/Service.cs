using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Mockup;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<BasePaginationResponse> GetMockup(int pageIndex, int pageSize, bool? isActive, string? search = null)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        
        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        var query = _dbContext.Mockups
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (isAdmin && isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }
        else if (!isAdmin)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.MockupResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(data, pageIndex, pageSize, totalCount);
    }

    public async Task<Response.MockupResponse> CreateMockup(Request.CreateMockupRequest request)
    {
        var normalizedName = NormalizeName(request.Name!);
        
        var activeNames = await _dbContext.Mockups
            .Where(x => !x.IsDeleted)
            .Select(x => x.Name)
            .ToListAsync();

        var isExisting = activeNames.Any(name =>
            string.Equals(NormalizeName(name), normalizedName, StringComparison.OrdinalIgnoreCase));

        if (isExisting)
        {
            throw new ConflictException("Tên Mockup đã tồn tại");
        }

        var newMockup = new Repository.Entity.Mockup()
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = request.Description,
            ImageUrl = request.ImageUrl!,
            IsActive = false, //default
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        
        _dbContext.Mockups.Add(newMockup);
       await _dbContext.SaveChangesAsync();

       var responseMockup = new Response.MockupResponse()
       {
           Id = newMockup.Id,
           Name = newMockup.Name,
           Description = newMockup.Description,
           ImageUrl =  newMockup.ImageUrl,
           IsActive = newMockup.IsActive,
           IsDeleted = newMockup.IsDeleted,
           CreatedAt = newMockup.CreatedAt,
           UpdatedAt = newMockup.UpdatedAt,
       };
       return responseMockup;
    }

    public async Task<Response.MockupResponse> ToggleMockupStatus(Guid id)
    {
        var mockup = await _dbContext.Mockups.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (mockup == null)
        {
            throw new NotFoundException("Mockup không hợp lệ hoặc đã bị xóa.");
        }

        mockup.IsActive = !mockup.IsActive;

        mockup.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        
        var responseMockup = new Response.MockupResponse()
        {
            Id = mockup.Id,
            Name = mockup.Name,
            Description = mockup.Description,
            ImageUrl = mockup.ImageUrl,
            IsActive = mockup.IsActive,
            IsDeleted = mockup.IsDeleted,
            CreatedAt = mockup.CreatedAt,
            UpdatedAt = mockup.UpdatedAt
        };

        return responseMockup;
    }

    public async Task<Response.DeleteMockupResponse> DeleteMockup(Guid id)
    {
        var mockup = await _dbContext.Mockups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mockup == null)
        {
            throw new NotFoundException("Mockup không tồn tại.");
        }

        if (mockup.IsDeleted)
        {
            throw new BadRequestException("Mockup đã bị xóa trước đó.");
        }

        mockup.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        
        var responseMockup = new Response.DeleteMockupResponse()
        {
            Id = mockup.Id,
            Name = mockup.Name,
            IsDeleted = mockup.IsDeleted,
            CreatedAt = mockup.CreatedAt,
            UpdatedAt = mockup.UpdatedAt,
        };

        return responseMockup;
    }

    private static string NormalizeName(string name) =>
        string.Join(" ", name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)); 
}
