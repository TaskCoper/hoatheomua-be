using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Order;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class OrderController: ControllerBase
{
    private readonly IService _orderService;
    
    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpGet("/api/v1/orders")]
    public async Task<IActionResult> GetOrderById(int pageIndex, int pageSize)
    {
        var result = await _orderService.GetOrderPaginationResponse(pageIndex,  pageSize);
        return Ok(result);
    }
    
    [HttpPost("/api/v1/orders")]
    public async Task<IActionResult> CreateOrder([FromBody]Request.CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    
    [HttpGet("/api/v1/orders/{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _orderService.GetOrderById(id);
        return Ok(result);
    }
    
    
    [HttpGet("/api/v1/orders/{id}/status")]
    public async Task<IActionResult> GetStatusOrder(Guid id)
    {
        var result = await _orderService.GetStatusOrder(id);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    
    [HttpPut("/api/v1/orders/{id}")]
    public async Task<IActionResult> UpdateOrderById(Request.UpdateOrderByIdRequest request)
    {
        var result = await _orderService.UpdateOrderById(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    
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