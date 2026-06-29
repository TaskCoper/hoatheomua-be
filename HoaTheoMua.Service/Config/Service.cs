using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace HoaTheoMua.Service.Config;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public Service(AppDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }


    public async Task<List<Response.ConfigResponse>> GetConfig()
    {
        var query = _dbContext.Configs
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt);

        var result = await query
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Group = x.Group,
                IsPublic = x.IsPublic,
                Value = StringToJson(x.Value),
                UserId = x.UserId,
            })
            .ToListAsync();

        return result;
    }

    public async Task<Response.ConfigResponse?> GetConfigById(Guid id)
    {
        // Require an authenticated caller; identity comes from the token, not a Users lookup.
        _ = _currentUser.GetRequiredUserId();

        var result = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new Response.ConfigResponse()
            {
                Id = x.Id,
                Key = x.Key,
                Group = x.Group,
                IsPublic = x.IsPublic,
                Value = StringToJson(x.Value),
                UserId = x.UserId,
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw new BadRequestException("Id Not Found");
        }

        return result;
    }

    public async Task<Response.ConfigResponse> CreateConfig(
        Request.CreateConfigRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        var key = request.Key.Trim().ToLower();

        var existKey = await _dbContext.Configs
            .AnyAsync(x => x.Key.Trim().ToLower() == key && !x.IsDeleted);

        if (existKey)
        {
            throw new BadRequestException("Key is exists");
        }

        var keyPrefix = request.Key!.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        
        if (request.Value.TryGetProperty("group", out JsonElement groupElement)
            && groupElement.ValueKind == JsonValueKind.String)
        {
            var group = groupElement.GetString();
            if (request.Group != group || request.Group != keyPrefix || group !=  keyPrefix)
            {
                throw new ConflictException("group doesn't match");
            }
        }
        
        
        var result = new Repository.Entity.Config()
        {

            Key = key,
            Group =  request.Group,
            Value = request.Value.GetRawText(),
            IsPublic = false,
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
            Group = result.Group,
            IsPublic = result.IsPublic,
            Value = StringToJson(result.Value),
            UserId = result.UserId,
        };
        return query;

    }

    public async Task<string> UpdateConfig(
        Guid id,
        Request.UpdateConfigRequest request)
    {
        
        var keyPrefix = request.Key!.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        
        if (request.Value.TryGetProperty("group", out JsonElement groupElement)
            && groupElement.ValueKind == JsonValueKind.String)
        {
            var group = groupElement.GetString();
            if (request.Group != group || request.Group != keyPrefix || group !=  keyPrefix)
            {
                throw new ConflictException("group doesn't match");
            }
        }
        
        if (!string.IsNullOrEmpty(request.Key))
        {
            var existKey = await  _dbContext.Configs
                .FirstOrDefaultAsync(x => x.Id != id && x.Key.ToLower().Trim().Contains(request.Key!.ToLower().Trim()));
            if (existKey != null)
            {
                throw new BadRequestException("Key is exists");
            }
        }
        
        var query = await _dbContext.Configs
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        

        
        if (query == null)
        {
            throw new BadRequestException("Id/Key Not Found");
        }
        
        if (request.IsPublic != null)
        {
            query.IsPublic = request.IsPublic.Value;
        }
        
        query.Key = request.Key;
        query.Group = request.Group;
        query.Value = request.Value.GetRawText();
        // if (request.IsPublic.HasValue)
        // {
        //     query.IsPublic = request.IsPublic.GetValueOrDefault();
        // }

        // await _dbContext.Configs.AddAsync(query);
        _dbContext.Configs.Update(query);
        await _dbContext.SaveChangesAsync();

        return ("Cập nhật thành công");
    }

    public async Task<string> DeleteConfig(Guid id)
    {
        var query = await _dbContext.Configs.Where(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();

        if (query == null)
        {
            throw new BadRequestException("Id Not Found");
        }

        query.IsDeleted = true;

        _dbContext.Configs.Update(query);
        await _dbContext.SaveChangesAsync();

        return "Xóa thành công.";
    }

    public async Task<List<Response.ConfigResponse>> GetConfigIsPublic()
    {
        var query = _dbContext.Configs.Where(x => x.IsPublic == true);

        var result = await query.Select(x => new Response.ConfigResponse
        {
            Id = x.Id,
            Key = x.Key,
            IsPublic = x.IsPublic,
            Group = x.Group,
            Value = StringToJson(x.Value),
            UserId = x.UserId,
        }).ToListAsync();

        if (result.Count == 0)
        {
            throw new NotFoundException("DONT_HAVE_ANY_CONFIG");
        }
        return result;
    }

    public async Task<List<Response.ConfigGroupResponse>> GetConfigGroups(string groupName)
    {   
        var key = groupName.ToLower().Trim();
        var configs = await _dbContext.Configs
            .Where(x => x.Group.ToLower().Trim().Contains(key) && !x.IsDeleted && x.IsPublic)
            .Select(x => new Response.ConfigGroupResponse()
            {
                Key = x.Key,
                Value = StringToJson(x.Value),
            }).ToListAsync();

        return configs;
    }

    private static JsonElement StringToJson(string value)
    {
        return JsonSerializer.Deserialize<JsonElement>(value);
    }
}

