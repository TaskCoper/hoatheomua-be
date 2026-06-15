using HoaTheoMua.Service.Cache;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.Jobs;

public class CleanupJob
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(ICacheService cacheService, ILogger<CleanupJob> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task WarmupCacheAsync()
    {
        _logger.LogInformation("Warming up dashboard cache");

        await _cacheService.SetAsync(
            "stats:dashboard",
            $"Warmup at {DateTimeOffset.UtcNow}",
            TimeSpan.FromHours(1));
    }

    public Task CleanupExpiredDataAsync()
    {
        _logger.LogInformation("Cleaning expired data");

        return Task.CompletedTask;
    }
}
