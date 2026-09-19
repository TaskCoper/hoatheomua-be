using HoaTheoMua.Extentions;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.CardTemplate;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/card-templates")]
public class CardTemplateController : ControllerBase
{
    private readonly IService _cardTemplateService;

    public CardTemplateController(IService cardTemplateService)
    {
        _cardTemplateService = cardTemplateService;
    }

    [HttpPost("")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateCardTemplate(Request.CreateCartTemplateRequest request)
    {
        var result = await _cardTemplateService.CreateCardTemplate(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetCardTemplate(int pageIndex, int pageSize, bool? isActive, string? templateType, string? search = null,
        [FromQuery] List<Guid>? productIds = null)
    {
        var result = await _cardTemplateService.GetCardTemplate(pageIndex, pageSize, isActive, templateType, search, productIds);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetCardTemplateDetail(Guid id)
    {
        var result = await _cardTemplateService.GetCardTemplateDetail(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateCardTemplate(Guid id, Request.UpdateCardTemplateRequest request)
    {
        var result = await _cardTemplateService.UpdateCardTemplate(id, request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteCardTemplate(Guid id)
    {
        var result = await _cardTemplateService.DeleteCardTemplate(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}

/*
 *[HttpGet("")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetCardTemplate(int pageIndex, int pageSize, bool? isActive, string? templateType, string? search = null)
    {
        var result = await _cardTemplateService.GetCardTemplate(pageIndex, pageSize, isActive, templateType, search);
        return Ok(result);
        //nhận vào mảng param product, từ generateCard --> client his -> 1. Card đc tạo ra từ product đó luông 2. card đó đc tạo ra từ GF mà GF đc tạo ra từ product đó
        //dựa vào rootId
        
        // thông tin + [ { name, n}]
        
    }
    
    //api nhận vào orderDetailId, virutalId và genreeated Card Id --> tìm đc cục nào trong mảng, udpate genetared mà anh anh gửi xuống update vào finalGenredCard
    //api nhận vào orderDetail lấy hết các card thuoi7c5 orderDetial đó
 * 
 */