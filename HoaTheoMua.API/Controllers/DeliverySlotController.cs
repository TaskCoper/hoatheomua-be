using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConfigService = HoaTheoMua.Service.Config;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/delivery-slots")]
public class DeliverySlotController : ControllerBase
{
    private readonly ConfigService.IService _configService;

    public DeliverySlotController(ConfigService.IService configService)
    {
        _configService = configService;
    }

    [HttpPatch("limits")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateLimits(
        [FromBody] ConfigService.Request.UpdateDeliverySlotLimitsRequest request)
    {
        var result = await _configService.UpdateDeliverySlotLimits(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
