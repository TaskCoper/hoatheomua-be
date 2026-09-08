using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Mockup;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/mockups")]
public class MockupController : ControllerBase
{
    private readonly IService _mockupService;
    
    public MockupController(IService mockupService)
    {
        _mockupService = mockupService;
    }

    [HttpGet("")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetMockup(int pageIndex, int pageSize, bool? isActive, string? search = null)
    {
        var result = await _mockupService.GetMockup(pageIndex, pageSize, isActive, search);
        return Ok(result);
    }
    
    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateMockup(Request.CreateMockupRequest request)
    {
        var result = await _mockupService.CreateMockup(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id::guid}/status")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> ToggleMockupStatus(Guid id)
    {
        var result = await _mockupService.ToggleMockupStatus(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id::guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteMockup(Guid id)
    {
        var result = await _mockupService.DeleteMockup(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}