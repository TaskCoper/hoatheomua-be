using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.PostInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.AdminPolicy)]
public class PostInsightsController(IService service) : ControllerBase
{
    [HttpGet("/api/v1/post-gen/platform-accounts/{id:guid}/insights")]
    public async Task<IActionResult> GetAccount(Guid id, [FromQuery] AccountInsightsRequest request, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(ApiResponseFactory.Base(await service.GetAccount(id, request, ct), traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/post-gen/publications/{id:guid}/insights")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(ApiResponseFactory.Base(await service.Get(id, ct), traceId: HttpContext.TraceIdentifier));
    }
}
