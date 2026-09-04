using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HoaTheoMua.Controllers;

[ApiController]
public class OrderController : ControllerBase
{
    private readonly IService _orderService;

    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("/api/v1/orders")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]

    public async Task<IActionResult> GetOrderById(int pageIndex, int pageSize, string? status = null)
    {
        var result = await _orderService.GetOrderPaginationResponse(pageIndex, pageSize, status);
        return Ok(result);
    }

    [HttpPost("/api/v1/orders")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [EnableRateLimiting(RateLimitExtensions.OrderWritePolicy)]
    public async Task<IActionResult> CreateOrder([FromBody]Request.CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    // [HttpPost("/api/v1/orders/default")]
    // [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    // public async Task<IActionResult> CreateDefaultOrder()
    // {
    //     var result = await _orderService.CreateDefaultOrder();
    //     return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    // }
    
    [HttpPatch("/api/v1/orders/{id}/cancel")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
    {
        var result = await _orderService.CancelOrder(id);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/orders/{id}")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _orderService.GetOrderById(id);
        return Ok(result);
    }


    [HttpGet("/api/v1/orders/{id}/status")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetStatusOrder(Guid id)
    {
        var result = await _orderService.GetStatusOrder(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/orders/nhanh-vn")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetNhanhVnOrders([FromBody] NhanhVnOrderListRequest.OrderListRequest request)
    {
        var result = await _orderService.GetOrderNhanhVn(request);
        return Ok(result);
    }

    [HttpPost("/api/v1/orders/shipping-location")]
    [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    public async Task<IActionResult> GetShippingLocation([FromBody] Request.LocationRequest filter)
    {
        var result = await _orderService.GetShippingLocation(filter);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    [HttpPatch("/api/v1/orders/update-ahamove")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateAhaMoveService([FromBody] Request.UpdateAhamoveRequest request)
    {
        var result = await _orderService.UpdateAhaMoveService(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("/api/v1/orders/admin")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]

    public async Task<IActionResult> GetOrderByAdmin([FromQuery] string? search, int pageIndex, int pageSize)
    {
        var result = await _orderService.GetOrderByAdmin(search, pageIndex, pageSize);
        return Ok(result);
    }
    
    // [HttpPost("/api/v1/orders/taxcode")]
    // [Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
    // public async Task<IActionResult> GetInfoTaxCode([FromQuery] string taxCode)
    // {
    //     var result = await _orderService.GetInfoTaxCode( taxCode);
    //     return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    // }

    // [HttpGet("/api/v1/orders")]
    // public async Task<IActionResult> GetAllOrderTransport(int pageIndex, int pageSize)
    // {
    //     var result = await _orderService.GetOrderPaginationResponse(pageIndex, pageSize);
    //     return Ok(result);
    // }
    //
    // [HttpPost("/api/v1/orders/update-tracking-number")]
    // public async Task<IActionResult> UpdateOrder([FromBody]Request.UpdateOrderRequest request)
    // {
    //     var result = await _orderService.UpdateOrder(request);
    //     return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    // }
}
