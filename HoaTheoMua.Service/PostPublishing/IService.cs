using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.PostPublishing;

public interface IService
{
    Task<Response.Preview> Preview(Guid id, string platform, Guid accountId, CancellationToken ct = default);
    Task<Response.Job> Publish(Guid id, string platform, Request.Publish request, CancellationToken ct = default);
    Task<Response.Job> Schedule(Guid id, string platform, Request.Schedule request, CancellationToken ct = default);
    Task<Response.Job> Reschedule(Guid id, string platform, Request.Reschedule request, CancellationToken ct = default);
    Task<Response.Job> CancelSchedule(Guid id, string platform, Request.CancelSchedule request, CancellationToken ct = default);
    Task<BasePaginationResponse> List(Guid id, string platform, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<Response.Detail> Get(Guid publicationId, CancellationToken ct = default);
    Task<Response.Job> Retry(Guid publicationId, Request.Revision request, CancellationToken ct = default);
    Task<Response.Job> Cancel(Guid publicationId, Request.Revision request, CancellationToken ct = default);
    Task<Response.Job> Recheck(Guid publicationId, Request.Revision request, CancellationToken ct = default);
    Task<Response.Job> Resolve(Guid publicationId, Request.Resolve request, CancellationToken ct = default);
}
