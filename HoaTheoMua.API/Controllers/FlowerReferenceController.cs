using HoaTheoMua.Extentions;
using HoaTheoMua.Service.FlowerReference;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/reference-flowers")]
public class FlowerReferenceController : ControllerBase
{
    private readonly IService _flowerReferenceService;

    public FlowerReferenceController(IService flowerReferenceService)
    {
        _flowerReferenceService = flowerReferenceService;
    }


    [HttpGet("")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetFlowerReference([FromQuery] Request.GetFlowerReferenceRequest request)
    {
        var result = await _flowerReferenceService.GetFlowerReference(request.pageIndex, request.pageSize, request.isActive, request.type, request.search);
        return Ok(result);
    }
    
    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateFlowerReference(Request.CreateFlowerReferenceRequest request)
    {
        var result = await _flowerReferenceService.CreateFlowerReference(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id::guid}/status")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> ToggleFlowerReferenceStatus(Guid id)
    {
        var result = await _flowerReferenceService.ToggleFlowerReferenceStatus(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id::guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteFlowerReference(Guid id)
    {
        var result = await _flowerReferenceService.DeleteFlowerReference(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
