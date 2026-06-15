using System.Linq.Expressions;

namespace HoaTheoMua.Service.Jobs;

public interface IJobService
{
    string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall);

    string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay);

    void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        Expression<Func<TJob, Task>> methodCall,
        string cronExpression,
        string queue = "default");

    void RemoveRecurring(string recurringJobId);

    bool Delete(string jobId);
}
