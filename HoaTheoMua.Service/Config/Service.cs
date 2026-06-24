using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;


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

        var result = new Repository.Entity.Config()
        {

            Key = key,
            Value = request.Value.GetRawText(),
            IsPublic = true,
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
        var key = request.Key.Trim().ToLower();

        var query = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync();

        if (query == null)
        {
            throw new BadRequestException("Id/Key Not Found");
        }

        var isDuplicate = await _dbContext.Configs
            .AnyAsync(x => 
                x.Key.Trim().ToLower() == key && 
                !x.IsDeleted &&
                x.Id != id);
        if (isDuplicate)
        {
            throw new BadRequestException("DUPLICATE KEY");
        }
        
        query.Key = request.Key;
        query.Value = request.Value.GetRawText();
        if (request.IsPublic.HasValue)
        {
            query.IsPublic = request.IsPublic.GetValueOrDefault();
        }

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
            Value = StringToJson(x.Value),
            UserId = x.UserId,
        }).ToListAsync();

        if (result.Count == 0)
        {
            throw new NotFoundException("DONT_HAVE_ANY_CONFIG");
        }
        return result;
    }

    private static JsonElement StringToJson(string value)
    {
        return JsonSerializer.Deserialize<JsonElement>(value);
    }
}

