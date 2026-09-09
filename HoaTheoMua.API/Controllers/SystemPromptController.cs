using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.SystemPrompt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/system-prompts")]
public class SystemPromptController : ControllerBase
{
    private readonly IService _systemPromptService;

    public SystemPromptController(IService systemPromptService)
    {
        _systemPromptService = systemPromptService;
    }
    
    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateSystemPromptDefault(Request.SystemPromptRequest request)
    {
        var result = await _systemPromptService.CreateSystemPromptDefault(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    [HttpGet("")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetSystemPrompt()
    {
        var result = await _systemPromptService.GetSystemPrompt();
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetSystemPromptInfo(Guid id)
    {
        var result = await _systemPromptService.GetSystemPromptInfo(id);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateSystemPrompt(Guid id, Request.UpdateSystemPromptRequest request)
    {
        var result = await _systemPromptService.UpdateSystemPrompt(id, request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
}