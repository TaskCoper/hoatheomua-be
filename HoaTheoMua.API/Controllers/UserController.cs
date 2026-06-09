using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.User;
using HoaTheoMua.Service.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class UserController: ControllerBase
{
    private readonly IService _UserService;
    public UserController(IService userService)
    {
        _UserService = userService;
    }

    [HttpGet("api/users/me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _UserService.GetMe();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/register")]
    public async Task<IActionResult> RegisterTask([FromBody]Request.RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Success", HttpContext.TraceIdentifier));
    }
}