using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc;
using ConfigService = HoaTheoMua.Service.Config;
using Request = HoaTheoMua.Service.Config.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/config")]
public class ConfigController : ControllerBase
{
    private readonly ConfigService.IService _configService;

    public ConfigController(ConfigService.IService configService)
    {
        _configService = configService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetConfigList()
    {
        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetConfig([FromRoute] Guid id)
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateConfig([FromBody] Request.CreateConfigRequest request)
    {
        return Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateConfig(
        [FromRoute] Guid id,
        [FromBody] Request.UpdateConfigRequest request)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConfig([FromRoute] Guid id)
    {
        return Ok();
    }
}