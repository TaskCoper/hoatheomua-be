using System.Text.Json;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.NhanhVnService;
using Microsoft.AspNetCore.Mvc;
using IService = HoaTheoMua.Service.NhanhVnService.IService;

namespace HoaTheoMua.Controllers;
[ApiController]

public class NhanhVnController : ControllerBase
{
    private readonly IService _nhanhVnService;
    private readonly Service.AhamoveService.IService _ahamoveService;

    // Dependency Injection thông qua Constructor tương tự Ahamove
    public NhanhVnController(IService nhanhVnService,  Service.AhamoveService.IService ahamoveService)
    {
        _nhanhVnService = nhanhVnService;
        _ahamoveService = ahamoveService;
    }
    
    [HttpPost("api/v1/nhanh-vn/orders-tags")]
    public async Task<IActionResult> GetOrderLabel(OrderLabelRequest request)
    {
        var result = await _nhanhVnService.GetOrderLabel(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("api/v1/nhanh-vn/orders-sources")]
    public async Task<IActionResult> GetOrderSource(OrderSourceRequest request)
    {
        var result = await _nhanhVnService.GetOrderSource(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/nhanh-vn/depot")]
    public async Task<IActionResult> GetNhanhVnDepot([FromBody] NhanhVnRequest.DepotRequest  request)
    {
        var result = await _ahamoveService.GetDepotDetails(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
        // return Ok(result);
    }
    
    [HttpPost("/api/v1/nhanh-vn/webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement? body)
    {
        Console.WriteLine(body);

        var result = await _nhanhVnService.HandleWebhook(body);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }

    [HttpGet("api/v1/nhanh-vn/orders")]
    public async Task<IActionResult> GetNhanhVnOrders([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _nhanhVnService.GetOrderNhanhVnPagination(pageIndex, pageSize);
        return Ok(result);
    }

    [HttpGet("api/v1/nhanh-vn/orders/{id}")]
    public async Task<IActionResult> GetNhanhVnOrderById(Guid id)
    {
        var result = await _nhanhVnService.GetOrderNhanhVnById(id);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }
}