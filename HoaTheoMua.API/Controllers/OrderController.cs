using FluentValidation;
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class OrderController : ControllerBase
{
    private readonly IService _orderService;
    private readonly IValidator<Request.CreateOrderRequest> _validatorCreateOrder; 
    
    public OrderController(IService orderService, 
        IValidator<Request.CreateOrderRequest> validator)
    {
        _orderService = orderService;
        _validatorCreateOrder = validator;
    }

    [HttpGet("/api/v1/orders")]
    public async Task<IActionResult> GetOrderById(int pageIndex, int pageSize)
    {
        var result = await _orderService.GetOrderPaginationResponse(pageIndex, pageSize);
        return Ok(result);
    }

    [HttpPost("/api/v1/orders")]
    public async Task<IActionResult> CreateOrder([FromBody]Request.CreateOrderRequest request)
    {
        var validationResult = await _validatorCreateOrder.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationExceptionError(errorMessage);
        }
        var result = await _orderService.CreateOrder(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/orders/{id}")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
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
    [Authorize(Policy = JwtExtensions.UserOrAdminPolicy)]
    public async Task<IActionResult> GetShippingLocation([FromBody] Request.LocationFilters filter)
    {
        var result = await _orderService.GetShippingLocation(filter);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
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