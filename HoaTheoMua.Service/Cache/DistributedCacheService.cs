using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.Cache;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly TimeSpan _defaultExpiration;

    public DistributedCacheService(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _defaultExpiration = TimeSpan.FromMinutes(
            double.Parse(configuration["Redis:DefaultExpirationMinutes"] ?? "60"));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache GET failed with key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? _defaultExpiration
            };

            await _cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache SET failed with key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache REMOVE failed with key {Key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, absoluteExpirationRelativeToNow, cancellationToken);

        return value;
    }
}
