using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc.Filters;
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

    public async Task<List<Response.ConfigResponse>> GetConfigContent(bool? isPublic, string? groupName, string? key)
    {
        var query = _dbContext.Configs
            .Where(x => !x.IsDeleted && 
                        x.Kind!.ToLower().Trim() == 
                        nameof(KindConfig.Content).ToLower().Trim());

        if (isPublic != null)
        {
            query = query.Where(x => x.IsPublic == isPublic);
        }

        if (!string.IsNullOrEmpty(groupName))
        {
            var filterGroupName = groupName.ToLower().Trim();
            query = query.Where(x => x.Group.ToLower().Trim().Contains(filterGroupName));
        }
        
        if (!string.IsNullOrEmpty(key))
        {
            var filterKey = key.ToLower().Trim();
            query = query.Where(x => x.Key.ToLower().Trim().Contains(filterKey));
        }
        
        var result = await query
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Group = x.Group,
                IsPublic = x.IsPublic,
                Value = StringToJson(x.Value),
                Kind = x.Kind,
                UserId = x.UserId,
            })
            .ToListAsync();

        return result;
  
    }

    public async Task<List<Response.ConfigResponse>> GetConfigSetting(string? groupName, string? key)
    {
        var query = _dbContext.Configs
            .Where(x => !x.IsDeleted && 
                        x.Kind!.ToLower().Trim() == 
                        nameof(KindConfig.Setting).ToLower().Trim());

        if (!string.IsNullOrEmpty(groupName))
        {
            var filterGroupName = groupName.ToLower().Trim();
            query = query.Where(x => x.Group.ToLower().Trim().Contains(filterGroupName));
        }
        
        if (!string.IsNullOrEmpty(key))
        {
            var filterKey = key.ToLower().Trim();
            query = query.Where(x => x.Key.ToLower().Trim().Contains(filterKey));
        }

        var result = await query
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Group = x.Group,
                IsPublic = x.IsPublic,
                Kind = x.Kind,
                Value = StringToJson(x.Value),
                UserId = x.UserId,
            })
            .ToListAsync();

        return result;
    }
    
    public async Task<List<Response.ConfigResponse>> GetConfigSecret(string? groupName, string? key)
    {
        var query =  _dbContext.Configs
            .Where(x => !x.IsDeleted &&
                        x.Kind!.ToLower().Trim() ==
                        nameof(KindConfig.Secret).ToLower().Trim());
        
        if (!string.IsNullOrEmpty(groupName))
        {
            var filterGroupName = groupName.ToLower().Trim();
            query = query.Where(x => x.Group.ToLower().Trim().Contains(filterGroupName));
        }
        
        if (!string.IsNullOrEmpty(key))
        {
            var filterKey = key.ToLower().Trim();
            query = query.Where(x => x.Key.ToLower().Trim().Contains(filterKey));
        }
        
        var result = await  query
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Group = x.Group,
                IsPublic = x.IsPublic,
                Kind = x.Kind,
                Value = MaskJsonOrString(x.Value),
                UserId = x.UserId,
            }).ToListAsync();
        
        return result;
    }

    public async Task<Response.RevealResponse> GetConfigReveal(Guid id)
    {
        var query = await _dbContext.Configs
            .Where(x => x.Id == id && !x.IsDeleted && 
                        x.Kind!.ToLower().Trim() == nameof(KindConfig.Secret).ToLower().Trim())
            .Select(x => new Response.RevealResponse()
            {
                Id  = x.Id,
                Key = x.Key,
                Value =  StringToJson(x.Value),
                RevealedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            })
            .FirstOrDefaultAsync();

        if (query == null)
        {
            throw new NotFoundException("Config not found or not a secret");
        }

        return query;
    }


    public async Task<List<Response.ConfigResponse>> GetConfig()
    {
        var query = _dbContext.Configs
            .Where(x => !x.IsDeleted);
        
        var result = await query
            .Select(x => new Response.ConfigResponse
            {
                Id = x.Id,
                Key = x.Key,
                Kind = x.Kind,
                Group = x.Group,
                IsPublic = x.IsPublic,
                Value = x.Kind != null && x.Kind.ToLower().Trim() == nameof(KindConfig.Secret).ToLower().Trim()
                    ? MaskJsonOrString(x.Value)
                    : StringToJson(x.Value),
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
                Kind = x.Kind,
                IsPublic = x.IsPublic,
                Value = x.Kind != null && x.Kind.ToLower().Trim() == nameof(KindConfig.Secret).ToLower().Trim()
                    ? MaskJsonOrString(x.Value)
                    : StringToJson(x.Value),
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
            Kind = request.Kind,
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
            Kind = result.Kind,
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
        var keyPrefix = request.Key.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        
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
                .FirstOrDefaultAsync(x => x.Id != id && x.Key.ToLower().Trim().Contains(request.Key!.ToLower().Trim()) && !x.IsDeleted);
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

        if (request.Kind.ToLower().Trim() != query.Kind!.ToLower().Trim())
        {
            throw new ConflictException("ConfigKindImmutable");
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

    public async Task<List<Response.ConfigResponse>> GetConfigIsPublic(string? groupName, string? key)
    {
        var query = _dbContext.Configs
            .Where(x => x.IsPublic == true && x.Kind!.ToLower().Trim() == nameof(KindConfig.Content).ToLower().Trim());
        
        if (!string.IsNullOrEmpty(groupName))
        {
            var filterGroupName = groupName.ToLower().Trim();
            query = query.Where(x => x.Group.ToLower().Trim().Contains(filterGroupName));
        }
        
        if (!string.IsNullOrEmpty(key))
        {
            var filterKey = key.ToLower().Trim();
            query = query.Where(x => x.Key.ToLower().Trim().Contains(filterKey));
        }
        
        var result = await query.Select(x => new Response.ConfigResponse
        {
            Id = x.Id,
            Key = x.Key,
            IsPublic = x.IsPublic,
            Kind = x.Kind,
            Group = x.Group,
            Value = x.Kind != null && x.Kind.ToLower().Trim() == nameof(KindConfig.Secret).ToLower().Trim()
                ? MaskJsonOrString(x.Value)
                : StringToJson(x.Value),
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
                Kind = x.Kind,
                Value = x.Kind != null && x.Kind.ToLower().Trim() == nameof(KindConfig.Secret).ToLower().Trim()
                    ? MaskJsonOrString(x.Value)
                    : StringToJson(x.Value),
            }).ToListAsync();

        return configs;
    }
    
    
    public async Task<Response.ConfigItem> GetNhanhVnOrderTag()
    {
        var query = await _dbContext.Configs
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Key.Contains("nhanhvn.order_tags"));
        
        if (query == null)
        {
            throw new BadRequestException("NhanhVnOrderTag Not Found");
        }
        
        
        var config = JsonSerializer.Deserialize<Response.ConfigItem>(query.Value);
        
        if (config == null || config.Options == null || config.Value == null)
        {
            throw new BadRequestException("Cannot convert value");
        }
        
        config.Options = config.Options
            .Where(x =>config.Value.Contains((int)x.Value!))
            .ToList();
        
        return config;
    }

    public async Task<int> GetNhanhVnOrderSources()
    {
        var query = await _dbContext.Configs
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Key.Contains("nhanhvn.order_sources"));
        
        if (query == null)
        {
            throw new BadRequestException("GetNhanhVnOrderSources Not Found");
        }
        
        
        var config = JsonSerializer.Deserialize<Response.OrderSourceConfig>(query.Value);
        
        if (config == null || config.Options == null || config.Value == null)
        {
            throw new BadRequestException("Cannot convert value");
        }
        
        return config.Value;
    }
    
    private static JsonElement StringToJson(string value)
    {
        return JsonSerializer.Deserialize<JsonElement>(value);
    }
    
    private static JsonElement MaskJsonOrString(string? rawValue)
    {
        if (string.IsNullOrEmpty(rawValue))
            return JsonDocument.Parse("{}").RootElement; 
        
        var jObject = JObject.Parse(rawValue);
        if (jObject["value"] != null)
        {
            string secretToken = jObject["value"]!.ToString();
            
            jObject["value"] = MaskSecret(secretToken);

            string maskedJsonString = jObject.ToString(Formatting.None);

            return JsonDocument.Parse(maskedJsonString).RootElement;
        }

        return JsonDocument.Parse(MaskSecret(rawValue)).RootElement;
    }
    
    private static string MaskSecret(string? value, int visibleSuffix = 10)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length <= visibleSuffix)
            return "••••••••••";
        return "••••••••••••••••••••" + value[^visibleSuffix..];
    }
}

