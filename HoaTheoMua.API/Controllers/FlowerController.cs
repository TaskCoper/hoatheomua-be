using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Flower;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api")]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class FlowerController : ControllerBase
{
    private readonly IService _flowerService;

    public FlowerController(IService flowerService)
    {
        _flowerService = flowerService;
    }

    [HttpPost("ai-flowers")]
    public async Task<IActionResult> CreateAiFlower(Request.CreateFlowerRequest request)
    {
        var result = await _flowerService.CreateAiFlower(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("ai-flowers/{sourceFlowerId}/regenerate")]
    public async Task<IActionResult> RegenerateAiFlower(Guid sourceFlowerId, Request.RegenerateFlowerRequest request)
    {
        var result = await _flowerService.RegenerateAiFlower(sourceFlowerId, request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

}
