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

    [HttpGet("ai-flowers/{generatedFlowerId:guid}/composition")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetGeneratedFlowerComposition(Guid generatedFlowerId)
    {
        var result = await _flowerService.GetGeneratedFlowerComposition(generatedFlowerId);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("generated-flowers/{generatedFlowerId:guid}/image")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetGeneratedFlowerImage(Guid generatedFlowerId)
    {
        var result = await _flowerService.GetGeneratedFlowerImage(generatedFlowerId);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("{mockupId:guid}/generated-flowers")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetGeneratedFlowersByMockupId(Guid mockupId, int pageSize, int pageIndex)
    {
        var result = await _flowerService.GetGeneratedFlowersByMockupId(mockupId, pageSize, pageIndex);
        return Ok(result);
    }
}
