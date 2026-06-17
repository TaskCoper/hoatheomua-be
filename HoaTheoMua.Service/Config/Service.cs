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
        
        
        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (userIdString == null)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        var userId = Guid.Parse(userIdString!);
        
        var existUser = await _dbContext.Users.AnyAsync(x => x.Id == userId);

        if (existUser == false)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        
        //anh phải lấy được cái config của user thông qua accessToken
        var result = _dbContext.Configs
            .Where(x => !x.IsDeleted && x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value, //suat ra value bi 2 dau gach gach "//1234//"
                UserId = x.UserId,

            });
            

        var count = await result.CountAsync();
        var items = await result
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return(ApiResponseFactory.BasePagination(items, pageIndex, pageSize, count));
        
    }

    public async Task<Response.ConfigResponse> GetConfigById(Guid id)
    {

        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (userIdString == null)
        {
            throw  new UnauthorizedException("User Not Found");
        }

        var userId = Guid.Parse(userIdString);
        
        var existUser = await _dbContext.Users.AnyAsync(x => x.Id == userId);
        if (existUser == false)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        
        var result = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted && x.UserId == userId)
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
        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (userIdString == null)
        {
            throw  new UnauthorizedException("User Not Found");
        }

        var userId = Guid.Parse(userIdString);
        
        var existUser = await _dbContext.Users.AnyAsync(x => x.Id == userId);
        if (existUser == false)
        {
            throw  new UnauthorizedException("User Not Found");
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
        
        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (userIdString == null)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        var userId = Guid.Parse(userIdString!);
        
        var existUser = await _dbContext.Users.AnyAsync(x => x.Id == userId);

        if (existUser == false)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        
        var query = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted && x.UserId == userId).FirstOrDefaultAsync();
        
        query.Key = request.Key;
        query.Value = request.Value.GetRawText();
        
        // await _dbContext.Configs.AddAsync(query);
        _dbContext.Configs.Update(query);
        await _dbContext.SaveChangesAsync();

        return ("Cập nhật thành công");
    }

    public async Task<string> DeleteConfig(Guid id)
    {
        
        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (userIdString == null)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        var userId = Guid.Parse(userIdString!);
        
        var existUser = await _dbContext.Users.AnyAsync(x => x.Id == userId);

        if (existUser == false)
        {
            throw  new UnauthorizedException("User Not Found");
        }
        
       var query = await _dbContext.Configs.Where(x => x.Id == id && !x.IsDeleted && x.UserId == userId).FirstOrDefaultAsync();
       query.IsDeleted = true;
       
        _dbContext.Configs.Update(query);
        await _dbContext.SaveChangesAsync();

        return "Xóa thành công.";
    }
}