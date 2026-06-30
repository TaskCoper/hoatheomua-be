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


    
    public List<AhaMoveRequest.ItemsRequest> GetAhamoveServices()
    {
        var keys = new[] { 
            "ahamove.shipping.sgn_bike_bulky", 
            "ahamove.shipping.sgn_bike_fragile", 
            "ahamove.shipping.sgn_bike_sms", 
            "ahamove.shipping.sgn_bike_d2d",
            
            "ahamove.shipping.sgn_bike_reim_bulky",
            // "ahamove.shipping.sgn_bike_pod"
        };
        
        var result = new List<AhaMoveRequest.ItemsRequest>();
        foreach (var key in keys)
        {
            if (_configs.TryGetValue(key, out var val) && val.ValueKind == JsonValueKind.String)
            {
                try
                {
                    var innerDoc = JsonDocument.Parse(val.GetString()!);
                    var itemReq = new AhaMoveRequest.ItemsRequest { Id = "" };

                    if (innerDoc.RootElement.TryGetProperty("_id", out var idProp))
                        itemReq.Id = idProp.GetString() ?? "";

                    if (innerDoc.RootElement.TryGetProperty("tier_code", out var tierProp))
                        itemReq.TierCode = tierProp.GetString();

                    if (innerDoc.RootElement.TryGetProperty("num", out var numProp))
                        itemReq.Num = numProp.GetInt32();
                    
                    //
                    // if (innerDoc.RootElement.TryGetProperty("name", out var nameProp))
                    //     itemReq.Name = nameProp.GetString();
                    //
                    // if (innerDoc.RootElement.TryGetProperty("price", out var priceProp))
                    //     itemReq.Price = priceProp.GetDecimal();

                    if (!string.IsNullOrEmpty(itemReq.Id))
                    {
                        result.Add(itemReq);
                    }
                }
                catch
                {
                    throw new ArgumentException("Config value could not be parsed");
                }
            }
        }
        
        return result.Any() ? result : new List<AhaMoveRequest.ItemsRequest>
        {
            new AhaMoveRequest.ItemsRequest { Id = "SGN-BIKE-SMS" },
            new AhaMoveRequest.ItemsRequest { Id = "SGN-BIKE-D2D" },
            new AhaMoveRequest.ItemsRequest { Id = "SGN-BIKE-FRAGILE" },
            new AhaMoveRequest.ItemsRequest { Id = "SGN-BIKE-BULKY", TierCode = "TIER_2" }
        };
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
