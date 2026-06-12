using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class SepayController : ControllerBase
{
    private readonly IService _sepayService;

    public SepayController(IService sepayService)
    {
        _sepayService = sepayService;
    }
    
    [HttpPost("/api/v1/sepay")]
    public async Task<IActionResult> SepayWebhookHandler([FromBody] Request.SepayWebhookRequest request)
    {
        await _sepayService.SepayWebhookHandler(request);
        return Ok(ApiResponseFactory.Base("success", true, "",HttpContext.TraceIdentifier));
    }
}