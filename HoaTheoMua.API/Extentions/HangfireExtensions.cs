using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using HoaTheoMua.Service.Jobs;

namespace HoaTheoMua.Extentions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        var workerCount = int.Parse(configuration["Hangfire:WorkerCount"] ?? "5");
        var queues = configuration.GetSection("Hangfire:Queues").Get<string[]>()
            ?? ["critical", "default", "low"];

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = workerCount;
            options.Queues = queues;
            options.ServerName = $"{Environment.MachineName}:{Guid.NewGuid()}";
        });

        services.AddScoped<IJobService, HangfireJobService>();
        services.AddScoped<CleanupJob>();

        return services;
    }

    public static WebApplication UseHangfireDashboardIfEnabled(
        this WebApplication app,
        IConfiguration configuration)
    {
        var enabled = bool.Parse(configuration["Hangfire:EnableDashboard"] ?? "false");

        if (!enabled)
        {
            return app;
        }

        var path = configuration["Hangfire:DashboardPath"] ?? "/hangfire";

        app.UseHangfireDashboard(path, new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter()],
            DisplayStorageConnectionString = false,
            DashboardTitle = "HoaTheoMua Background Jobs"
        });

        return app;
    }
}

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment()
            || (httpContext.User.Identity?.IsAuthenticated ?? false);
    }
}
