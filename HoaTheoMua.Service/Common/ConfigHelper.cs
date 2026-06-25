using System.Text.Json;
using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;
using HoaTheoMua.Service.AhamoveService;

namespace HoaTheoMua.Service.Common;

public static class ConfigHelper
{
    public static async Task<JsonElement?> GetConfigValueAsync(AppDbContext dbContext, string key)
    {
        var config = await dbContext.Configs.FirstOrDefaultAsync(x => x.Key == key && !x.IsDeleted);
        if (config == null || string.IsNullOrWhiteSpace(config.Value)) return null;

        try
        {
            var jsonDoc = JsonDocument.Parse(config.Value);
            if (jsonDoc.RootElement.TryGetProperty("value", out var val))
            {
                return val;
            }
        }
        catch { }

        return null;
    }

    public static async Task<string> GetConfigStringAsync(AppDbContext dbContext, string key, string defaultValue = "")
    {
        var val = await GetConfigValueAsync(dbContext, key);
        if (val.HasValue && val.Value.ValueKind == JsonValueKind.String)
        {
            return val.Value.GetString() ?? defaultValue;
        }
        return defaultValue;
    }
    
    public static async Task<int> GetConfigIntAsync(AppDbContext dbContext, string key, int defaultValue = 0)
    {
        var val = await GetConfigValueAsync(dbContext, key);
        if (val.HasValue && val.Value.ValueKind == JsonValueKind.Number)
        {
            return val.Value.GetInt32();
        }
        return defaultValue;
    }

    public static async Task<List<Request.ItemsRequest>> GetAhamoveServicesAsync(AppDbContext dbContext)
    {
        var keys = new[] { 
            "ahamove.shipping.sgn_bike_bulky", 
            "ahamove.shipping.sgn_bike_fragile", 
            "ahamove.shipping.sgn_bike_sms", 
            "ahamove.shipping.sgn_bike_d2d" 
        };
        var configs = await dbContext.Configs.Where(x => keys.Contains(x.Key) && !x.IsDeleted).ToListAsync();
        
        var result = new List<Request.ItemsRequest>();
        foreach (var config in configs)
        {
            try
            {
                var doc = JsonDocument.Parse(config.Value);
                if (doc.RootElement.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.String)
                {
                    var innerDoc = JsonDocument.Parse(val.GetString()!);
                    var itemReq = new Request.ItemsRequest { Id = "" };
                    
                    if (innerDoc.RootElement.TryGetProperty("_id", out var idProp))
                        itemReq.Id = idProp.GetString() ?? "";
                        
                    if (innerDoc.RootElement.TryGetProperty("tier_code", out var tierProp))
                        itemReq.TierCode = tierProp.GetString();
                    
                    if (innerDoc.RootElement.TryGetProperty("num", out var numProp))
                        itemReq.Num = numProp.GetInt32();
                        
                    if (!string.IsNullOrEmpty(itemReq.Id))
                    {
                        result.Add(itemReq);
                    }
                }
            }
            catch {}
        }
        
        return result.Any() ? result : new List<Request.ItemsRequest>
        {
            new Request.ItemsRequest { Id = "SGN-BIKE-SMS" },
            new Request.ItemsRequest { Id = "SGN-BIKE-D2D" },
            new Request.ItemsRequest { Id = "SGN-BIKE-FRAGILE" },
            new Request.ItemsRequest { Id = "SGN-BIKE-BULKY", TierCode = "TIER_2" }
        };
    }
}
