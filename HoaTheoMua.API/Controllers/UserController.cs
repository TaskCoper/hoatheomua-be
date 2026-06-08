
using HoaTheoMua.Service.Auth;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class UserController: ControllerBase
{
    private readonly IService _authService;
    
    public UserController(IService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("/api/v1/auth/register")]
    public async Task<IActionResult> RegisterTask([FromBody]Request.RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result,"", HttpContext.TraceIdentifier));
    }
}