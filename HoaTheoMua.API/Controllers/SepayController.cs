using System.Text;
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
        return Ok(ApiResponseFactory.Base("success", true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("api/v1/baokim")]
    public async Task<IActionResult> BaoKimWebhookHandler()
    {
        // Đọc body
        Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(
                   Request.Body,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
        }

        // Lấy headers
        var headers = Request.Headers.ToDictionary(
            h => h.Key,
            h => h.Value.ToString());

        // Lấy query string
        var query = Request.Query.ToDictionary(
            q => q.Key,
            q => q.Value.ToString());

        Console.WriteLine(new
        {
            Method = Request.Method,
            Path = Request.Path,
            Query = query,
            Headers = headers,
            Body = body
        });

        return Ok(new
        {
            Method = Request.Method,
            Path = Request.Path,
            Query = query,
            Headers = headers,
            Body = body
        });
    }
}