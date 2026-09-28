using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.FlowerReference;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<BasePaginationResponse> GetFlowerReference(int pageIndex, int pageSize, bool? isActive, string? type, string? search = null)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        
        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        var query = _dbContext.FlowerReferences
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

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == ParseType(type));
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
            .Select(x => new Response.FlowerReferenceResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                Type = x.Type == FlowerReferenceType.Flower ? "flower" : "bundle",
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(data, pageIndex, pageSize, totalCount);
    }

    public async Task<Response.FlowerReferenceResponse> CreateFlowerReference(Request.CreateFlowerReferenceRequest request)
    {
        var normalizedName = NormalizeName(request.Name!);
        
        var activeNames = await _dbContext.FlowerReferences
            .Where(x => !x.IsDeleted)
            .Select(x => x.Name)
            .ToListAsync();

        var isExisting = activeNames.Any(name =>
            string.Equals(NormalizeName(name), normalizedName, StringComparison.OrdinalIgnoreCase));

        if (isExisting)
        {
            throw new ConflictException("Tên hình tham khảo đã tồn tại. Vui lòng chọn tên khác.");
        }

        var flowerReference = new Repository.Entity.FlowerReference()
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = request.Description,
            ImageUrl = request.ImageUrl!,
            IsActive = false, //default
            Type = ParseType(request.Type!),
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        
        _dbContext.FlowerReferences.Add(flowerReference);
       await _dbContext.SaveChangesAsync();

       var response = new Response.FlowerReferenceResponse()
       {
           Id = flowerReference.Id,
           Name = flowerReference.Name,
           Description = flowerReference.Description,
           ImageUrl =  flowerReference.ImageUrl,
           IsActive = flowerReference.IsActive,
           Type = flowerReference.Type.ToString().ToLowerInvariant(),
           IsDeleted = flowerReference.IsDeleted,
           CreatedAt = flowerReference.CreatedAt,
           UpdatedAt = flowerReference.UpdatedAt,
       };
       return response;
    }

    public async Task<Response.FlowerReferenceResponse> ToggleFlowerReferenceStatus(Guid id)
    {
        var flowerReference = await _dbContext.FlowerReferences.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (flowerReference == null)
        {
            throw new NotFoundException("Không tìm thấy hình tham khảo hoặc hình tham khảo đã bị xóa.");
        }

        flowerReference.IsActive = !flowerReference.IsActive;

        flowerReference.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        
        var response = new Response.FlowerReferenceResponse()
        {
            Id = flowerReference.Id,
            Name = flowerReference.Name,
            Description = flowerReference.Description,
            ImageUrl = flowerReference.ImageUrl,
            IsActive = flowerReference.IsActive,
            Type = flowerReference.Type.ToString().ToLowerInvariant(),
            IsDeleted = flowerReference.IsDeleted,
            CreatedAt = flowerReference.CreatedAt,
            UpdatedAt = flowerReference.UpdatedAt
        };

        return response;
    }

    public async Task<Response.DeleteFlowerReferenceResponse> DeleteFlowerReference(Guid id)
    {
        var flowerReference = await _dbContext.FlowerReferences
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (flowerReference == null)
        {
            throw new NotFoundException("Không tìm thấy hình tham khảo cần xóa hoặc hình tham khảo đã bị xóa.");
        }

        flowerReference.IsDeleted = true;
        flowerReference.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        
        var response = new Response.DeleteFlowerReferenceResponse()
        {
            Id = flowerReference.Id,
            Name = flowerReference.Name,
            IsDeleted = flowerReference.IsDeleted,
            CreatedAt = flowerReference.CreatedAt,
            UpdatedAt = flowerReference.UpdatedAt,
        };

        return response;
    }

    private static string NormalizeName(string name) =>
        string.Join(" ", name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)); 

    private static FlowerReferenceType ParseType(string value)
    {
        if (int.TryParse(value, out _) || !Enum.TryParse<FlowerReferenceType>(value, true, out var type))
        {
            throw new BadRequestException("Loại hình tham khảo không hợp lệ. Vui lòng chọn \"flower\" hoặc \"bundle\".");
        }

        return type;
    }
}
