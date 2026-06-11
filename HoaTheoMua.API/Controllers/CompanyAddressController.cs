using HoaTheoMua.Service.Models;
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
    public async Task<IActionResult> GetCompanyAddresses(
        [FromQuery] Request.GetCompanyAddressesRequest request)
    {
        var result = await _companyAddressService.GetCompanyAddresses(request);

        return Ok(ApiResponseFactory.Base(
            result,
            true,
            "Lấy danh sách địa chỉ công ty thành công.",
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
            "Lấy chi tiết địa chỉ công ty thành công.",
            HttpContext.TraceIdentifier
        ));
    }
}