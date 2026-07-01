using System.Text.Json;
using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;
using HoaTheoMua.Service.AhaMoveService;

namespace HoaTheoMua.Service.Common;

public class GroupConfig
{
    private readonly Dictionary<string, JsonElement> _configs = new();

    public GroupConfig(List<Repository.Entity.Config> configs)
    {
        foreach (var config in configs)
        {
            if (string.IsNullOrWhiteSpace(config.Value)) continue;
            try
            {
                var doc = JsonDocument.Parse(config.Value);
                if (doc.RootElement.TryGetProperty("value", out var val))
                {
                    _configs[config.Key] = val;
                }
            }
            catch
            {
                throw new ArgumentException("Config value could not be parsed");
            }
        }
    }

    public T? GetValue<T>(string key,  T? defaultValue = default)
    {
        if (_configs.TryGetValue(key, out var val))
        {
            try
            {
                // if (typeof(T) == typeof(string))
                // {
                //     if (val.ValueKind == JsonValueKind.String)
                //         return (T)(object)(val.GetString() ?? string.Empty);
                //     
                //     return (T)(object)val.ToString();
                // }

                var deserialized = JsonSerializer.Deserialize<T>(val.GetRawText());
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
            catch
            {
                throw  new ArgumentException("Config value could not be parsed");
            }
        }
        return defaultValue;
    }


    
    public List<AhaMoveRequest.ItemsRequest> GetAhamoveServices(List<SpecialRequest> availableRequests)
    {
        var result = new List<AhaMoveRequest.ItemsRequest>();
        
        foreach (var kvp in _configs)
        {
            if (kvp.Key.StartsWith("ahamove.shipping.") && kvp.Value.ValueKind == JsonValueKind.String)
            {
                try
                {
                    var innerDoc = JsonDocument.Parse(kvp.Value.GetString()!);
                    string id = "";
                    
                    if (innerDoc.RootElement.TryGetProperty("_id", out var idProp))
                        id = idProp.GetString() ?? "";

                    if (string.IsNullOrEmpty(id))
                        continue;

                    // Kiểm tra xem Id có tồn tại trong danh sách request của AhaMove không
                    var matchedRequest = availableRequests?.FirstOrDefault(r => r.Id == id);
                    if (matchedRequest != null)
                    {
                        var itemReq = new AhaMoveRequest.ItemsRequest { Id = id };

                        if (innerDoc.RootElement.TryGetProperty("tier_code", out var tierProp))
                            itemReq.TierCode = tierProp.GetString();

                        if (innerDoc.RootElement.TryGetProperty("num", out var numProp))
                            itemReq.Num = numProp.GetInt32();

                        result.Add(itemReq);
                    }   
                }
                catch
                {
                    // throw new ArgumentException("Config value could not be parsed");
                }
            }
        }
        
        return result;
    }
}

public static class ConfigHelper
{
    public static async Task<GroupConfig> GetConfigsByGroupsAsync(AppDbContext dbContext, params string[] groups)
    {
        var configs = await dbContext.Configs.Where(x => groups.Contains(x.Group) && !x.IsDeleted).ToListAsync();
        return new GroupConfig(configs);
    }
}
