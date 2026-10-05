using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Flower;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jobs = HoaTheoMua.Service.FlowerJobs;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class FlowerJobsController(Jobs.Service jobs) : ControllerBase
{
    [HttpPost("ai-flower-jobs")]
    public async Task<IActionResult> Create(Request.CreateFlowerRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
        => AcceptedJob(await jobs.Create(request, key, ct));

    [HttpPost("ai-flowers/{id:guid}/regeneration-jobs")]
    public async Task<IActionResult> Regenerate(Guid id, Request.RegenerateFlowerRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
        => AcceptedJob(await jobs.Regenerate(id, request, key, ct));

    [HttpGet("ai-flower-jobs/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(ApiResponseFactory.Base(await jobs.Get(id, ct), true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("ai-flower-jobs")]
    public async Task<IActionResult> List(CancellationToken ct, bool active = true, int pageIndex = 1, int pageSize = 20)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(await jobs.List(active, pageIndex, pageSize, ct));
    }

    [HttpPost("ai-flower-jobs/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await jobs.Cancel(id, ct), true, "", HttpContext.TraceIdentifier));

    private IActionResult AcceptedJob(Jobs.JobResponse job)
    {
        Response.Headers.CacheControl = "no-store";
        return Accepted($"/api/v1/ai-flower-jobs/{job.JobId}", ApiResponseFactory.Base(job, true, "", HttpContext.TraceIdentifier));
    }
}
