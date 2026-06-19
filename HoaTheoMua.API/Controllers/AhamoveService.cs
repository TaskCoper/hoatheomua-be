using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class AhamoveService: ControllerBase
{
    private readonly IService _ahamoveService;

    public AhamoveService(IService ahamoveService)
    {
        _ahamoveService = ahamoveService;
    }
    
    [HttpPost("/api/v1/ahamove/estimate")]
    public async Task<IActionResult> EstimateShippingFee([FromBody] Request.EstimateShippingFeeRequest request)
    {
        var result = await _ahamoveService.EstimateShippingFee(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("/api/v1/ahamove/service-details")]
    public async Task<IActionResult> GetNhanhVnOrders([FromQuery] int pageIndex, int pageSize)
    {
        var result = await _ahamoveService.GetServiceDetailsAhaMove(pageIndex, pageSize);
        //return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
        return Ok(result);
    }
    
    // [HttpPost("/api/v1/ahamove/orders")]
    // public async Task<IActionResult> CreateOrder([FromBody] Request.CreateOrderRequestAhamove request)
    // {
    //     var result = await _ahamoveService.CreateOrder(request);
    //     return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    // }
    //
    
    
}