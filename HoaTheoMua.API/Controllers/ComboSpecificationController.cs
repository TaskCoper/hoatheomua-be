using System.Text.Json;
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
    private readonly Service.Revalidate.IService _revalidateService;

    public ComboSpecificationController(IService materialComboService, Service.Revalidate.IService revalidateService)
    {
        _materialComboService = materialComboService;
        _revalidateService = revalidateService;
    }

    [HttpPost("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> AddMaterialInCombo(Guid comboId, [FromBody] Request.MaterialComboRequest request)
    {
        var result = await _materialComboService.AddMaterialInCombo(comboId, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product", //{entity:"product", id:[comboId]}
            id = new[] { comboId }
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPut("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateMaterialInCombo(Guid comboId, [FromBody] Request.MaterialComboRequest request)
    {
        var result = await _materialComboService.UpdateMaterialInCombo(comboId, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product", //{entity:"product", id:[comboId]}
            id = new[] { comboId }
        });
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteMaterialInCombo(Guid comboId, [FromBody] Request.DeleteMaterial request)
    {
        var result = await _materialComboService.DeleteMaterialInCombo(comboId, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product", //{entity:"product", id:[comboId]}
            id = new[] { comboId }
        });
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPut("child-quantity")]
    //4. Hệ thống tính định lượng dự kiến của từng thành phần trong combo con đó bằng định lượng chuẩn của combo cha nhân với hệ số mới,
    // làm tròn theo đơn vị tính của thành phần.
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateChildQuantity(Request.UpdateMaterialChildRequest request)
    {
        var result = await _materialComboService.UpdateChildProductWithFactor(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product", //{entity:"product", id:[comboId]}
            id = new[] { request.ProductId }
        });
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }

    [HttpPost("{productId:guid}/publish")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> PublishActiveCombo(Guid productId)
    {
        var result = await _materialComboService.PublicActiveCombo(productId);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product", //{entity:"product", id:[comboId]}
            id = new[] { productId }
        });
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("{productId:guid}/unpublish")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UnPublishActiveCombo(Guid productId)
    {
        var result = await _materialComboService.UnPublicActiveCombo(productId);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product", //{entity:"product", id:[comboId]}
            id = new[] { productId }
        });
        return Ok(ApiResponseFactory.Base(result, true,"", HttpContext.TraceIdentifier));
    }
}