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
    
    [HttpPost("/api/v1/nhanh-vn/webhook/inventory")]
    public async Task<IActionResult> WebhookNhanhVnInventory([FromBody] NhanhInventoryChangeWebhookRequest request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnInventory(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/nhanh-vn/webhook/add-product")]
    public async Task<IActionResult> WebhookNhanhVnAddProduct([FromBody] NhanhProductAddWebhookRequest request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnProductAdd(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/nhanh-vn/webhook/order-add")]
    public async Task<IActionResult> WebhookNhanhVnOrderAdd([FromBody] NhanhOrderAddWebhookRequest request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnOrderAdd(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/nhanh-vn/webhook/order-update")]
    public async Task<IActionResult> WebhookNhanhVnOrderUpdate([FromBody] NhanhOrderAddWebhookRequest request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnOrderUpdate(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/nhanh-vn/webhook/order-delete")]
    public async Task<IActionResult> WebhookNhanhVnOrderDelete([FromBody] NhanhOrderDeleteWebhookRequest request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnOrderDelete(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/nhanh-vn/webhook/update-product")]
    public async Task<IActionResult> WebhookNhanhVnProductUpdate([FromBody] NhanhProductAddWebhookRequest request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnProductUpdate(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/nhanh-vn/webhook/delete-product")]
    public async Task<IActionResult> WebhookNhanhVnDeleteProduct([FromBody] string request)
    {
        var result = await _nhanhVnService.WebhookNhanhVnProductDelete(request);
        return Ok(ApiResponseFactory.Base(result, true, "Success", HttpContext.TraceIdentifier));
    }
}