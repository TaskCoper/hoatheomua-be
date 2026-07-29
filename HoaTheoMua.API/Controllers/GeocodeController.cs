using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Geocode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeocodeService = HoaTheoMua.Service.Geocode;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.CustomerOrAdminPolicy)]
[Route("api/v1/geocode")]
public class GeocodeController : ControllerBase
{
    private readonly GeocodeService.IService _geocodeService;

    public GeocodeController(GeocodeService.IService geocodeService)
    {
        _geocodeService = geocodeService;
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] Request.AutocompleteQuery query)
    {
        var res = await _geocodeService.Autocomplete(query);
        return StatusCode(res.StatusCode, res);
    }

    [HttpGet("place")]
    public async Task<IActionResult> GetPlace([FromQuery] string refId)
    {
        var res = await _geocodeService.GetPlaceDetail(refId);
        return StatusCode(res.StatusCode, res);
    }
}
