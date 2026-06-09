
using HoaTheoMua.Extentions;
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

    [HttpPost("/api/v1/auth/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var result = await _authService.RefreshToken();
        HttpContext.Response.WriteAuthCookies(result.AccessToken, result.RefreshToken);
        return Ok(ApiResponseFactory.SuccessResponse(null, "success", HttpContext.TraceIdentifier));
    }
}