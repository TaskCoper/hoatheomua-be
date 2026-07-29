using HoaTheoMua.Service.Geocode;
using Microsoft.AspNetCore.Mvc;
using GeocodeService = HoaTheoMua.Service.Geocode;

namespace HoaTheoMua.Controllers;

[ApiController]
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
        var results = await _geocodeService.Autocomplete(query);
        return Ok(new { results });
    }

    [HttpGet("place")]
    public async Task<IActionResult> GetPlace([FromQuery] string refId)
    {
        var result = await _geocodeService.GetPlaceDetail(refId);
        return Ok(new { result });
    }
}
