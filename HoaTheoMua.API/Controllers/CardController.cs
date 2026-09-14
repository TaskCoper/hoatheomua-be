using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Card;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class CardController : ControllerBase
{
    private readonly IService _cardService;
    public CardController(IService cardService)
    {
        _cardService = cardService;
    }

    // [HttpPost("handmade-cards")]
    // public async Task<IActionResult> CreateHandMadeCard(Request.CreateCardRequest request)
    // {
    //     var result = await  _cardService.CreateHandMadeCard(request);
    //     return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    // }
    //
    // [HttpPost("ai-cards")]
    // public async Task<IActionResult> CreateCardAi(Request.CreateCardAiRequest request)
    // {
    //     var result = await _cardService.CreateCardAi(request);
    //     return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    // }
}
