using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.PostGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]
public class PostGenerationController(IService service) : ControllerBase
{
    [HttpPost("/api/v1/posts/{id:guid}/generate")]
    [EnableRateLimiting(RateLimitExtensions.HeavyPolicy)]
    public async Task<IActionResult> GeneratePlatformPosts(Guid id, Request.GenerateRequest request, CancellationToken cancellationToken)
    {
        var result = await service.GeneratePlatformPosts(id, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
