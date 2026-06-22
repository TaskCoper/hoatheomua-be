using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc;
using ConfigService = HoaTheoMua.Service.Config;
using Request = HoaTheoMua.Service.Config.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/configs")]
public class ConfigController : ControllerBase
{
    private readonly ConfigService.IService _configService;

    public ConfigController(ConfigService.IService configService)
    {
        _configService = configService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetConfigList()
    {
        var result = await _configService.GetConfig();
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetConfig([FromRoute] Guid id)
    {
        var result = await _configService.GetConfigById(id);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateConfig([FromBody] Request.CreateConfigRequest request)
    {
        var result = await _configService.CreateConfig(request);
        return Ok(ApiResponseFactory.Base(result,  true,"", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateConfig(
        [FromRoute] Guid id,
        [FromBody] Request.UpdateConfigRequest request)
    {
        var result = await _configService.UpdateConfig (id, request);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConfig([FromRoute] Guid id)
    {
        var result = await _configService.DeleteConfig(id);
        return Ok(ApiResponseFactory.Base(result,  true,"", HttpContext.TraceIdentifier));
    }
}