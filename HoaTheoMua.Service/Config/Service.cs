using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    

    public async Task<List<Response.ConfigResponse>> GetConfig()
    {
        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (userIdString == null)
        {
            throw new UnauthorizedException("User Not Found");
        }

        var userId = Guid.Parse(userIdString);

        var existUser = await _dbContext.Users.AnyAsync(x => x.Id == userId);

        if (existUser == false)
        {
            throw new UnauthorizedException("User Not Found");
        }

        var configs = await _dbContext.Configs
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = configs
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Value = StringToJson(x.Value),
                UserId = x.UserId,
            })
            .ToList();

        return result;
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
        
        // var isIdExist = await _dbContext.Configs.AnyAsync(x => x.Id == id && !x.IsDeleted);
        // if (isIdExist == false)
        // {
        //     throw new BadRequestException(" Id Not Exists");
        // }
        //
        // var result = await _dbContext.Configs
        //     .Where(x => x.Id == id && !x.IsDeleted )
        //     .Select(x => new Response.ConfigResponse()
        //     {
        //         Id = x.Id,
        //         Key = x.Key,
        //         Value = StringToJson(x.Value),
        //         UserId =  x.UserId,
        //     })
        //     .FirstOrDefaultAsync();

        var result = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new Response.ConfigResponse()
            {
                Id = x.Id,
                Key = x.Key,
                Value = StringToJson(x.Value),
                UserId = x.UserId,
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw  new BadRequestException("  Id Not Found");
        }
        
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
        
        var key = request.Key.Trim().ToLower();

        var existKey = await _dbContext.Configs
            .AnyAsync(x => x.Key.Trim().ToLower() == key && !x.IsDeleted);

        if (existKey)
        {
            throw new BadRequestException("Key is exists");
        }
        
        var result =  new Repository.Entity.Config()
        {
                
            Key = key,
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
            Value = StringToJson(result.Value),
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
        
        var key = request.Key.Trim().ToLower();

        var query = await _dbContext.Configs
            .Where(x => x.Id == id  && x.Key.Trim().ToLower() != key && !x.IsDeleted)
            .FirstOrDefaultAsync();

        if (query == null)
        {
            throw  new BadRequestException("  Id/Key Not Found");
        }
        
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
        
        var isIdExist = await _dbContext.Configs.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (isIdExist == null)
        {
            throw new BadRequestException(" Id Not Exists");
        } 
        isIdExist.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync();

        return "Xóa thành công.";
    }
    
    private static JsonElement StringToJson(string value)
    {
        return JsonSerializer.Deserialize<JsonElement>(value);
    }
}

