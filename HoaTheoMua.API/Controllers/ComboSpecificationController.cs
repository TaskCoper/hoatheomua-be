using HoaTheoMua.Extentions;
using Microsoft.AspNetCore.Authorization;
using HoaTheoMua.Service.Material;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/combo-specification")]
public class ComboSpecificationController : ControllerBase
{ 
    private readonly IService _materialComboService;

    public ComboSpecificationController(IService materialComboService)
    {
        _materialComboService = materialComboService;
    }

    [HttpPost("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> AddMaterialInCombo(Guid comboId, [FromBody] Request.MaterialComboRequest request)
    {
        var result = await _materialComboService.AddMaterialInCombo(comboId, request);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPut("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateMaterialInCombo(Guid comboId, [FromBody] Request.MaterialComboRequest request)
    {
        var result = await _materialComboService.UpdateMaterialInCombo(comboId, request);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteMaterialInCombo(Guid comboId, [FromBody] Request.DeleteMaterial request)
    {
        var result = await _materialComboService.DeleteMaterialInCombo(comboId, request);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPut("child-quantity")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateChildQuantity(Request.UpdateMaterialChildRequest request)
    {
        var result = await _materialComboService.UpdateChildProductWithFactor(request);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
}