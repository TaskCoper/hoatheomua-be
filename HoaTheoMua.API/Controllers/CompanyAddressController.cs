using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress;
using Request = HoaTheoMua.Service.CompanyAddress.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/company-addresses")]
public class CompanyAddressController : ControllerBase
{
    private readonly CompanyAddressService.IService _companyAddressService;

    public CompanyAddressController(CompanyAddressService.IService companyAddressService)
    {
        _companyAddressService = companyAddressService;
    }
    [HttpGet]
    public async Task<IActionResult> GetCompanyAddress()
    {
        var result = await _companyAddressService.GetCompanyAddress();

        return Ok(ApiResponseFactory.Base(
            result,
            true,
            result == null ? "Không tìm thấy địa chỉ công ty." : "Lấy địa chỉ công ty thành công.",
            HttpContext.TraceIdentifier
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCompanyAddressById([FromRoute] Guid id)
    {
        var result = await _companyAddressService.GetCompanyAddressById(id);

        return Ok(ApiResponseFactory.Base(
            result,
            true,
            result == null ? "Không tìm thấy địa chỉ công ty." : "Lấy địa chỉ công ty thành công.",
            HttpContext.TraceIdentifier
        ));
    }
    
    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateCompanyAddress(
        [FromBody] Request.CreateCompanyAddressRequest request)
    {
        var result = await _companyAddressService.CreateCompanyAddress(request);

        return StatusCode(201, ApiResponseFactory.Base(
            result,
            true,
            "Tạo địa chỉ công ty thành công.",
            HttpContext.TraceIdentifier
        ));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateCompanyAddress(
        [FromRoute] Guid id,
        [FromBody] Request.UpdateCompanyAddressRequest request)
    {
        var result = await _companyAddressService.UpdateCompanyAddress(id, request);

        return Ok(ApiResponseFactory.Base(
            result,
            true,
            "Cập nhật địa chỉ công ty thành công.",
            HttpContext.TraceIdentifier
        ));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteCompanyAddress([FromRoute] Guid id)
    {
        var result = await _companyAddressService.DeleteCompanyAddress(id);

        return Ok(ApiResponseFactory.Base(
            result,
            true,
            "Xóa địa chỉ công ty thành công.",
            HttpContext.TraceIdentifier
        ));
    }
}