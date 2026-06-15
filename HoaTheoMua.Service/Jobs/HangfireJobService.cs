using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.Jobs;

public class HangfireJobService : IJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, ILogger<HangfireJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall)
    {
        var jobId = _backgroundJobClient.Enqueue(methodCall);
        _logger.LogInformation("Hangfire job enqueued. JobId: {JobId}", jobId);

        return jobId;
    }

    public string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule(methodCall, delay);
        _logger.LogInformation("Hangfire job scheduled. JobId: {JobId}, Delay: {Delay}", jobId, delay);

        return jobId;
    }

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression, string queue = "default")
    {
        _recurringJobManager.AddOrUpdate(
            recurringJobId,
            methodCall,
            cronExpression,
            new RecurringJobOptions
            {
                QueueName = queue
            });

        _logger.LogInformation(
            "Hangfire recurring job registered. JobId: {JobId}, Cron: {Cron}, Queue: {Queue}",
            recurringJobId,
            cronExpression,
            queue);
    }

    public void RemoveRecurring(string recurringJobId)
    {
        _recurringJobManager.RemoveIfExists(recurringJobId);
    }

    public bool Delete(string jobId)
    {
        return _backgroundJobClient.Delete(jobId);
    }
}
