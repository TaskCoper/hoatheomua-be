using System.Text;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Webhook;
using Microsoft.AspNetCore.Mvc;
using BaoKimService = HoaTheoMua.Service.BaoKim;

namespace HoaTheoMua.Controllers;

[ApiController]
public class SepayController : ControllerBase
{
    private readonly IService _sepayService;
    private readonly BaoKimService.IService _baoKimService;
    private readonly IHostEnvironment _environment;

    // Mirrors the MVC pipeline's own options, since this controller deserializes the BaoKim
    // callback by hand rather than through model binding.
    private static readonly JsonSerializerOptions CallbackJsonOptions = new(JsonSerializerDefaults.Web);

    public SepayController(
        IService sepayService, BaoKimService.IService baoKimService, IHostEnvironment environment)
    {
        _sepayService = sepayService;
        _baoKimService = baoKimService;
        _environment = environment;
    }

    [HttpPost("/api/v1/sepay")]
    public async Task<IActionResult> SepayWebhookHandler([FromBody] Request.SepayWebhookRequest request)
    {
        await _sepayService.SepayWebhookHandler(request);
        return Ok(ApiResponseFactory.Base("success", true, "", HttpContext.TraceIdentifier));
    }

    // Deliberately not [FromBody]: the signature covers the exact bytes BaoKim sent, and
    // re-serializing a bound DTO would not reproduce them (property order, spacing and escaping all
    // differ), so the body is read raw, verified, and only then deserialized.
    [HttpPost("/api/v1/baokim")]
    public async Task<IActionResult> BaoKimWebhookHandler()
    {
        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync();

        // Bring-up logging, Development only: the body carries payment details and the headers can
        // carry whatever BaoKim sends, neither of which belongs in prod logs. Dumping every header
        // is the point — it shows whether BaoKim really names the header "Signature" (otherwise we
        // read null and report it as missing) and what casing it uses.
        if (_environment.IsDevelopment())
        {
            Console.WriteLine("[BAOKIM WEBHOOK] ---------- incoming callback ----------");
            foreach (var header in HttpContext.Request.Headers)
            {
                Console.WriteLine($"[BAOKIM WEBHOOK] header {header.Key}: {header.Value}");
            }
            Console.WriteLine($"[BAOKIM WEBHOOK] body: {rawBody}");
            Console.WriteLine("[BAOKIM WEBHOOK] -------------------------------------");
        }

        _baoKimService.VerifyCallbackSignature(
            rawBody, HttpContext.Request.Headers["Signature"].FirstOrDefault());

        BaoKimPaymentCallback? request;
        try
        {
            request = JsonSerializer.Deserialize<BaoKimPaymentCallback>(rawBody, CallbackJsonOptions);
        }
        catch (JsonException ex)
        {
            throw new BadRequestException($"Malformed BaoKim callback body: {ex.Message}");
        }

        if (request == null)
        {
            throw new BadRequestException("Empty BaoKim callback body");
        }

        await _sepayService.BaoKimWebHookHandler(request);

        // BaoKim reads `code` out of the body to decide whether to redeliver, so this returns the
        // gateway's own ack shape rather than the usual ApiResponseFactory envelope, which has no
        // `code` field and would make BaoKim retry every successful delivery.
        return Ok(new BaoKimCallbackAck());
    }
}