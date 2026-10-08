using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Voucher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1")]
public class VoucherController : ControllerBase
{
    private readonly IService _voucherService;

    public VoucherController(IService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet("admin/vouchers")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetVouchers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] string? visibility = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? programId = null)
    {
        var result = await _voucherService.GetVouchers(pageIndex, pageSize, keyword, visibility, isActive, programId);
        return Ok(result);
    }

    [HttpPost("admin/vouchers")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateVoucher([FromBody] Request.CreateVoucherRequest request)
    {
        var result = await _voucherService.CreateVoucher(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("admin/vouchers/{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteVoucher([FromRoute] Guid id)
    {
        var result = await _voucherService.DeleteVoucher(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPatch("admin/vouchers/active")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> SetVouchersActive([FromBody] Request.SetVouchersActiveRequest request)
    {
        var result = await _voucherService.SetVouchersActive(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("admin/vouchers/export")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> ExportVouchers(
        [FromQuery] string? visibility = null,
        [FromQuery] List<Guid>? programIds = null)
    {
        var file = await _voucherService.ExportVouchers(visibility, programIds);
        return File(file.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
    }

    [HttpGet("admin/vouchers/{id:guid}/usages")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetVoucherUsages(
        [FromRoute] Guid id,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _voucherService.GetVoucherUsages(id, pageIndex, pageSize);
        return Ok(result);
    }

    [HttpGet("vouchers/public")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    public async Task<IActionResult> GetPublicVouchers([FromQuery] decimal? productFee = null)
    {
        var result = await _voucherService.GetPublicVouchers(productFee);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("vouchers/estimate")]
    [Authorize(Policy = JwtExtensions.CustomerPolicy)]
    [EnableRateLimiting(RateLimitExtensions.VoucherEstimatePolicy)]
    public async Task<IActionResult> EstimateVoucher([FromBody] Request.EstimateVoucherRequest request)
    {
        var result = await _voucherService.EstimateVoucher(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    
    
}