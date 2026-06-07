using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HoaTheoMua.Service.CacheService;

public class Service: IService
{
    private readonly IDistributedCache _distributedCache;
    private readonly TimeSpan _defaultExpiration =  TimeSpan.FromMinutes(15);

    public Service(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }
    
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };
        
        var jsonString = JsonSerializer.Serialize(value);
        _distributedCache.SetString(key, jsonString, options);
    }

    public bool TryGet<T>(string key, out T value)
    {
        var jsonString = _distributedCache.GetString(key);
        if (string.IsNullOrEmpty(jsonString))
        {
            value = default!;
            return false;
        }
        value = JsonSerializer.Deserialize<T>(jsonString)!;
        return true;
    }

    public void Remove(string key)
    {
        _distributedCache.Remove(key);
    }
}