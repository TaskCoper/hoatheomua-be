using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.Storage;

public class CleanupPendingUploadsJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CleanupPendingUploadsJob> _logger;

    public CleanupPendingUploadsJob(IServiceProvider services, ILogger<CleanupPendingUploadsJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var storageOption = scope.ServiceProvider.GetRequiredService<IOptions<StorageOption>>().Value;
            var cutoffTime = DateTimeOffset.UtcNow.AddHours(-storageOption.PendingExpirationHours);

            var expiredFiles = await dbContext.FileMetadata
                .Where(f => f.UploadStatus == FileUploadStatus.Pending && f.CreatedAt < cutoffTime)
                .ToListAsync(stoppingToken);

            foreach (var file in expiredFiles)
            {
                file.UploadStatus = FileUploadStatus.Expired;
                file.UpdatedAt = DateTimeOffset.UtcNow;
            }

            if (expiredFiles.Count > 0)
            {
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Marked {Count} pending uploads as expired.", expiredFiles.Count);
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
