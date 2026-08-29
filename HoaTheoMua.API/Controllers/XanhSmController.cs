using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class XanhSmController : ControllerBase
{
    private readonly HoaTheoMua.Service.Order.IService _orderService;

    public XanhSmController(HoaTheoMua.Service.Order.IService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("/api/v1/xanhsm/estimate")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> EstimateShippingFee(
        [FromBody] Request.EstimateXanhSmShippingFeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.EstimateXanhSmShippingFee(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
