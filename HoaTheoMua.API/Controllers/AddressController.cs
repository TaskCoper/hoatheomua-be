using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddressService = HoaTheoMua.Service.Address;
using Request = HoaTheoMua.Service.Address.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/addresses")]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class AddressController : ControllerBase
{
    private readonly AddressService.IService _addressService;

    public AddressController(AddressService.IService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result = await _addressService.GetMyAddresses();
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAddress([FromRoute] Guid id)
    {
        var result = await _addressService.GetAddressById(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] Request.CreateAddressRequest request)
    {
        var result = await _addressService.CreateAddress(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAddress(
        [FromRoute] Guid id,
        [FromBody] Request.UpdateAddressRequest request)
    {
        var result = await _addressService.UpdateAddress(id, request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id:guid}/default")]
    public async Task<IActionResult> SetDefaultAddress([FromRoute] Guid id)
    {
        var result = await _addressService.SetDefaultAddress(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAddress([FromRoute] Guid id)
    {
        var result = await _addressService.DeleteAddress(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
