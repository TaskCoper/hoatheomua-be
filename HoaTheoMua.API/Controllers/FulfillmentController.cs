using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FulfillmentService = HoaTheoMua.Service.Fulfillment;

namespace HoaTheoMua.Controllers;

/// <summary>
/// Ops window onto the fulfilment saga. The design has no timers, so this is also how a stalled
/// fulfilment gets found: filter a non-terminal state and ask for rows untouched for N minutes.
/// </summary>
[ApiController]
public class FulfillmentController : ControllerBase
{
    private readonly FulfillmentService.IService _fulfillmentService;

    public FulfillmentController(FulfillmentService.IService fulfillmentService)
    {
        _fulfillmentService = fulfillmentService;
    }

    /// <summary>
    /// e.g. ?state=Failed              -> orders whose payment landed but whose fulfilment did not
    ///      ?stuckForMinutes=15        -> anything untouched for 15 minutes, in any state
    /// </summary>
    [HttpGet("/api/v1/admin/fulfillments")]
    [Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]
    public async Task<IActionResult> GetFulfillments(
        [FromQuery] string? state,
        [FromQuery] int? stuckForMinutes,
        [FromQuery] int limit = 100)
    {
        var result = await _fulfillmentService.GetFulfillmentStatesAsync(
            state, stuckForMinutes, limit, HttpContext.RequestAborted);

        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
