using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace HoaTheoMua.Service.Config;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public Service(AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    

    public async Task<BasePaginationResponse> GetConfig(int pageIndex, int pageSize)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        

        var result = _dbContext.Configs
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value,
                UserId = x.UserId,

            });
            

        var items = await result
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var count =  items.Count();
       
        return(ApiResponseFactory.BasePagination(items, pageIndex, pageSize, count));
        
    }

    public async Task<Response.ConfigResponse> GetConfigById(Guid id)
    {

        var result = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new Response.ConfigResponse()
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value,
                UserId =  x.UserId,
            })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<Response.ConfigResponse> CreateConfig(
        Request.CreateConfigRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            throw new NotFoundException("User Not Found");
        }

        var result =  new Repository.Entity.Config()
        {
                Key = request.Key,
                Value = request.Value.GetRawText(),
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null,
                IsDeleted = false
            };
            
            await _dbContext.Configs.AddAsync(result);
            await _dbContext.SaveChangesAsync();

            var query = new Response.ConfigResponse()
            {
                Id = result.Id,
                Key = result.Key,
                Value = result.Value,
                UserId = result.UserId,
            };
            return query;

    }

    public async Task<string> UpdateConfig(
        Guid id,
        Request.UpdateConfigRequest request)
    {
        var query = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        query.Key = request.Key;
        query.Value = request.Value.GetRawText();
        
        await _dbContext.Configs.AddAsync(query);
        await _dbContext.SaveChangesAsync();

        return ("Cập nhật thành công");
    }

    public async Task<string> DeleteConfig(Guid id)
    {
       var query = await _dbContext.Configs.Where(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
       query.IsDeleted = true;
       
        _dbContext.Configs.Update(query);
        await _dbContext.SaveChangesAsync();

        return "Xóa thành công.";
    }

    private Guid GetUserId()
    {
        var UserId = _httpContextAccessor.HttpContext?.User.FindFirst("UserId").Value;
        if (UserId != null)
        {
            return Guid.Parse(UserId);
        }
        return Guid.Empty;
    }
}