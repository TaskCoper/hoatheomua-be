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

    [HttpPut("/api/v1/admin/voucher-programs/{id:guid}")]
    public async Task<IActionResult> UpdateVoucherProgram(
        Guid id, [FromBody] Request.UpdateVoucherProgramRequest request, CancellationToken cancellationToken)
    {
        var result = await _voucherProgramService.UpdateVoucherProgram(id, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("/api/v1/admin/voucher-programs/{id:guid}")]
    public async Task<IActionResult> DeleteVoucherProgram(Guid id, CancellationToken cancellationToken)
    {
        var result = await _voucherProgramService.DeleteVoucherProgram(id, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/admin/voucher-programs/{id:guid}/vouchers")]
    public async Task<IActionResult> GenerateProgramVouchers(
        Guid id, [FromBody] Request.GenerateProgramVouchersRequest request, CancellationToken cancellationToken)
    {
        var result = await _voucherProgramService.GenerateProgramVouchers(id, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
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

    [HttpGet("/api/v1/admin/voucher-programs/with-vouchers")]
    public async Task<IActionResult> GetVoucherProgramsWithVouchers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] Guid? programId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _voucherProgramService.GetVoucherProgramsWithVouchers(
            pageIndex, pageSize, keyword, programId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("/api/v1/admin/voucher-programs/{programId:guid}/vouchers")]
    public async Task<IActionResult> GetVoucherByProgramId(
        Guid programId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _voucherProgramService.GetVoucherByProgramId(pageIndex, pageSize, keyword, programId, isActive);
        return Ok(result);
    }
}