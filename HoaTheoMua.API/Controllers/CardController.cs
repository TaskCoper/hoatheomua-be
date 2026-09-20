using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Card;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1")]
public class CardController : ControllerBase
{
    private readonly IService _cardService;
    public CardController(IService cardService)
    {
        _cardService = cardService;
    }

    [HttpGet("admin/generated-cards")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetAdminGeneratedCards(
        [FromQuery] Request.AdminGeneratedCardListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cardService.GetAdminGeneratedCards(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("handmade-cards")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateHandMadeCard(Request.CreateCardRequest request)
    {
        var result = await  _cardService.CreateHandMadeCard(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("ai-cards")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CreateCardAi(Request.CreateCardAiRequest request)
    {
        var result = await _cardService.CreateCardAi(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("final-generated-card")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> UpdateFinalGeneratedCardId(Request.UpdateFinalGeneratedCardId request)
    {
        var result = await _cardService.UpdateFinalGeneratedCardId(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("order-details/{orderDetailId:guid}/cards")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetCardByOrderDetailId(
        Guid orderDetailId,
        [FromQuery] Guid virtualId)
    {
        var result = await _cardService.GetCardByOrderDetailId(orderDetailId, virtualId);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("generated-cards/by-products")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetGeneratedCardsByProductIds(
        Request.GetGeneratedCardsByProductIdsRequest request)
    {
        var result = await _cardService.GetGeneratedCardsByProductIds(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
