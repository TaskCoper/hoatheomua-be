using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfigList()
    {
        var result = await _configService.GetConfig();
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("content")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfigContentList([FromQuery] bool? isPublic, string? groupName, string? key)
    {
        var result = await _configService.GetConfigContent(isPublic, groupName, key);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
    [HttpGet("settings")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfigSettingList([FromQuery] string? groupName, string? key)
    {
        var result = await _configService.GetConfigSetting(groupName, key);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
    [HttpGet("secrets")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfigSecretList([FromQuery] string? groupName, string? key)
    {
        var result = await _configService.GetConfigSecret(groupName, key);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
    [HttpGet("secrets/{id:guid}/reveal")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfigReveal(Guid id)
    {
        var result = await _configService.GetConfigReveal(id);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfig([FromRoute] Guid id)
    {
        var result = await _configService.GetConfigById(id);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateConfig([FromBody] Request.CreateConfigRequest request)
    {
        var result = await _configService.CreateConfig(request);
        return Ok(ApiResponseFactory.Base(result,  true,"", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateConfig(
        [FromRoute] Guid id,
        [FromBody] Request.UpdateConfigRequest request)
    {
        var result = await _configService.UpdateConfig (id, request);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteConfig([FromRoute] Guid id)
    {
        var result = await _configService.DeleteConfig(id);
        return Ok(ApiResponseFactory.Base(result,  true,"", HttpContext.TraceIdentifier));
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicConfig([FromQuery] string? groupName, string? key)
    {
        var result = await _configService.GetConfigIsPublic(groupName, key);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("group")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetConfigGroups([FromQuery] string groupName)
    {
        var result = await _configService.GetConfigGroups(groupName);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
}