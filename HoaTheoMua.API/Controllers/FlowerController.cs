using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Flower;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1")]
public class FlowerController : ControllerBase
{
    private readonly IService _flowerService;

    public FlowerController(IService flowerService)
    {
        _flowerService = flowerService;
    }

    [HttpGet("generated-flowers")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetCustomerGeneratedFlowers(
        [FromQuery] Request.CustomerGeneratedFlowerListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _flowerService.GetCustomerGeneratedFlowers(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("generated-flowers/{generatedFlowerId:guid}/image")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> DownloadGeneratedFlowerImage(Guid generatedFlowerId)
    {
        var result = await _flowerService.DownloadImage(generatedFlowerId);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    [HttpGet("admin/generated-flowers")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetAdminGeneratedFlowers(
        [FromQuery] Request.AdminGeneratedFlowerListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _flowerService.GetAdminGeneratedFlowers(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("ai-flowers")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateAiFlower(Request.CreateFlowerRequest request)
    {
        var result = await _flowerService.CreateAiFlower(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("ai-flowers/{sourceFlowerId}/regenerate")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> RegenerateAiFlower(Guid sourceFlowerId, Request.RegenerateFlowerRequest request)
    {
        var result = await _flowerService.RegenerateAiFlower(sourceFlowerId, request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

}
