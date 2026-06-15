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
    
    [HttpPost("/api/v1/auth/estimate")]
    public async Task<IActionResult> EstimateShippingFee([FromBody] Request.EstimateFeeRequest request)
    {
        var result = await _ahamoveService.EstimateFee(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    
}