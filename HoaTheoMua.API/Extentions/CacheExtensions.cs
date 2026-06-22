using HoaTheoMua.Service.Cache;

namespace HoaTheoMua.Extentions;

public static class CacheExtensions
{
    public static IServiceCollection AddRedisCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = configuration["Redis:InstanceName"] ?? "HoaTheoMua:";
        });

        services.AddScoped<ICacheService, DistributedCacheService>();

        return services;
    }
}
