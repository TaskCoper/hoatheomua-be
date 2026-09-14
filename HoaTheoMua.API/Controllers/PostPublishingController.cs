using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.AdminPolicy)]
public class PostPublishingController(IService service) : ControllerBase
{
    [HttpGet("/api/v1/posts/{id:guid}/platforms/{platform}/publication-preview")]
    public async Task<IActionResult> Preview(Guid id, string platform, [FromQuery] Guid accountId, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.Preview(id, platform, accountId, ct), traceId: HttpContext.TraceIdentifier));
    [HttpPost("/api/v1/posts/{id:guid}/platforms/{platform}/publish")]
    public async Task<IActionResult> Publish(Guid id, string platform, Request.Publish request, CancellationToken ct)
    {
        var job = await service.Publish(id, platform, request, ct);
        return Accepted($"/api/v1/post-gen/publications/{job.PublicationId}", ApiResponseFactory.Base(job, traceId: HttpContext.TraceIdentifier));
    }
    [HttpGet("/api/v1/posts/{id:guid}/platforms/{platform}/publications")]
    public async Task<IActionResult> List(Guid id, string platform, CancellationToken ct, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.List(id, platform, pageIndex, pageSize, ct));
    [HttpPost("/api/v1/posts/{id:guid}/platforms/{platform}/schedule")]
    public async Task<IActionResult> Schedule(Guid id, string platform, Request.Schedule request, CancellationToken ct)
    {
        var job = await service.Schedule(id, platform, request, ct);
        return Accepted($"/api/v1/post-gen/publications/{job.PublicationId}", ApiResponseFactory.Base(job, traceId: HttpContext.TraceIdentifier));
    }
    [HttpPut("/api/v1/posts/{id:guid}/platforms/{platform}/schedule")]
    public async Task<IActionResult> Reschedule(Guid id, string platform, Request.Reschedule request, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.Reschedule(id, platform, request, ct), traceId: HttpContext.TraceIdentifier));
    [HttpDelete("/api/v1/posts/{id:guid}/platforms/{platform}/schedule")]
    public async Task<IActionResult> CancelSchedule(Guid id, string platform, [FromQuery] Request.CancelSchedule request, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.CancelSchedule(id, platform, request, ct), traceId: HttpContext.TraceIdentifier));
    [HttpGet("/api/v1/post-gen/publications/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.Get(id, ct), traceId: HttpContext.TraceIdentifier));
    [HttpPost("/api/v1/post-gen/publications/{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, Request.Revision request, CancellationToken ct)
        => Accepted(ApiResponseFactory.Base(await service.Retry(id, request, ct), traceId: HttpContext.TraceIdentifier));
    [HttpPost("/api/v1/post-gen/publications/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, Request.Revision request, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.Cancel(id, request, ct), traceId: HttpContext.TraceIdentifier));
    [HttpPost("/api/v1/post-gen/publications/{id:guid}/recheck")]
    public async Task<IActionResult> Recheck(Guid id, Request.Revision request, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.Recheck(id, request, ct), traceId: HttpContext.TraceIdentifier));
    [HttpPost("/api/v1/post-gen/publications/{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, Request.Resolve request, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await service.Resolve(id, request, ct), traceId: HttpContext.TraceIdentifier));
}
