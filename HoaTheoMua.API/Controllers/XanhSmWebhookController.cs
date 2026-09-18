using HoaTheoMua.Service.XanhSm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class XanhSmWebhookController(XanhSmWebhookService webhookService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("/api/v1/xanhsm/webhook")]
    [Consumes("application/json")]
    [RequestSizeLimit(262144)]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        using var body = new MemoryStream();
        await Request.Body.CopyToAsync(body, cancellationToken);
        await webhookService.ReceiveAsync(body.ToArray(),
            Request.Headers["X-Environment"].ToString(),
            Request.Headers["X-VSF-Signature"].ToString(),
            Request.Headers["X-VSF-Signature-Secondary"].ToString(), cancellationToken);
        return Ok(new { received = true });
    }
}
