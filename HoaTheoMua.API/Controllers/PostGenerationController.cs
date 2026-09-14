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
    [HttpGet("/api/v1/posts/{id:guid}/platforms/{platform}/official")]
    public async Task<IActionResult> GetOfficialVersion(Guid id, string platform, CancellationToken cancellationToken)
    {
        var result = await service.GetOfficialVersion(id, platform, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/posts/{id:guid}/images/regenerate")]
    [EnableRateLimiting(RateLimitExtensions.HeavyPolicy)]
    public async Task<IActionResult> RegenerateImage(Guid id, Request.RegenerateImageRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RegenerateImage(id, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/posts/{id:guid}/images")]
    public async Task<IActionResult> GetImageVersions(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetImageVersions(id, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("/api/v1/posts/{id:guid}/platforms/{platform}/images")]
    public async Task<IActionResult> UpdatePlatformImages(Guid id, string platform, Request.UpdatePlatformImagesRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdatePlatformImages(id, platform, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/posts/{id:guid}/generate")]
    [EnableRateLimiting(RateLimitExtensions.HeavyPolicy)]
    public async Task<IActionResult> GeneratePlatformPosts(Guid id, Request.GenerateRequest request, CancellationToken cancellationToken)
    {
        var result = await service.GeneratePlatformPosts(id, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/posts/{id:guid}/platforms/{platform}/content/regenerate")]
    [EnableRateLimiting(RateLimitExtensions.HeavyPolicy)]
    public async Task<IActionResult> RegeneratePlatformContent(Guid id, string platform, Request.RegenerateContentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RegeneratePlatformContent(id, platform, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("/api/v1/posts/{id:guid}/platforms/{platform}/content")]
    public async Task<IActionResult> EditPlatformContent(Guid id, string platform, Request.EditContentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.EditPlatformContent(id, platform, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/posts/{id:guid}/platforms/{platform}/content/versions")]
    public async Task<IActionResult> GetContentVersions(Guid id, string platform, CancellationToken cancellationToken)
    {
        var result = await service.GetContentVersions(id, platform, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("/api/v1/posts/{id:guid}/platforms/{platform}/content/official")]
    public async Task<IActionResult> SelectOfficialContent(Guid id, string platform, Request.SelectContentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SelectOfficialContent(id, platform, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
