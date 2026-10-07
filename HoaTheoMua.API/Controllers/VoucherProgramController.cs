using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.VoucherProgram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.AdminPolicy)]
public class VoucherProgramController : ControllerBase
{
    private readonly IService _voucherProgramService;

    public VoucherProgramController(IService voucherProgramService)
    {
        _voucherProgramService = voucherProgramService;
    }

    [HttpPost("/api/v1/admin/voucher-programs")]
    public async Task<IActionResult> CreateVoucherProgram(
        [FromBody] Request.CreateVoucherProgramRequest request, CancellationToken cancellationToken)
    {
        var result = await _voucherProgramService.CreateVoucherProgram(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/admin/voucher-programs")]
    public async Task<IActionResult> GetVoucherPrograms(
        [FromQuery] Request.GetVoucherProgramsRequest request, CancellationToken cancellationToken)
    {
        var result = await _voucherProgramService.GetVoucherPrograms(request, cancellationToken);
        return Ok(result);
    }
}
