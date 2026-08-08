using System.Text.Json;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Revalidate;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;


[ApiController]
public class RevalidateController: ControllerBase
{
    private readonly IService _service;
    
    public RevalidateController(IService service)
    {
        _service = service;
    }
    
    
    [HttpPost("/api/v1/revalidate")]
    public async Task<IActionResult> Revalidate([FromBody] JsonElement request)
    {
        var result = await _service.RevalidateApi(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}