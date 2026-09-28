using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Card;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jobs = HoaTheoMua.Service.CardJobs;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class CardJobsController(Jobs.Service jobs) : ControllerBase
{
    [HttpPost("ai-card-jobs")]
    [HttpPost("ai-cards")]
    public async Task<IActionResult> Create(Request.CreateCardAiRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
        => AcceptedJob(await jobs.Create(request, key, ct));

    [HttpGet("ai-card-jobs/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(ApiResponseFactory.Base(await jobs.Get(id, ct), true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("ai-card-jobs")]
    public async Task<IActionResult> List(CancellationToken ct, bool active = true, int pageIndex = 1, int pageSize = 20, Guid? orderDetailId = null, Guid? virtualId = null)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(await jobs.List(active, pageIndex, pageSize, ct, orderDetailId, virtualId));
    }

    [HttpPost("ai-card-jobs/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        => Ok(ApiResponseFactory.Base(await jobs.Cancel(id, ct), true, "", HttpContext.TraceIdentifier));

    private IActionResult AcceptedJob(Jobs.JobResponse job)
    {
        Response.Headers.CacheControl = "no-store";
        return Accepted($"/api/v1/ai-card-jobs/{job.JobId}", ApiResponseFactory.Base(job, true, "", HttpContext.TraceIdentifier));
    }
}
