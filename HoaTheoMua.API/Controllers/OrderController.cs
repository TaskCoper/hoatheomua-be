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
    
    [HttpPost("/api/v1/orders")]
    public async Task<IActionResult> CreateOrderTransport([FromBody]Request.CreateOrderRequest request)
    {
        var result = await _orderService.ProcessOrderTransport(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    
    [HttpGet("/api/v1/orders/{id}")]
    public async Task<IActionResult> GetOrderTransportById(Guid id)
    {
        var result = await _orderService.CheckingOrder(id);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    
    [HttpGet("/api/v1/orders")]
    public async Task<IActionResult> GetAllOrderTransport(int pageIndex, int pageSize)
    {
        var result = await _orderService.GetOrderPaginationResponse(pageIndex, pageSize);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
}