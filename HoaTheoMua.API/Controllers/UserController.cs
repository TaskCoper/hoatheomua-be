using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Mvc;
using AuthService = HoaTheoMua.Service.Auth;
using UserService = HoaTheoMua.Service.User;

namespace HoaTheoMua.Controllers;

[ApiController]
public class UserController : ControllerBase
{
    private readonly AuthService.IService _userService;
    private readonly AuthService.IService _authService;

    public UserController(
        AuthService.IService userService,
        AuthService.IService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    
    [HttpPost("/api/v1/auth/sign-in")]
    public async Task<IActionResult> SignIn([FromBody] AuthService.Request request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.ErrorResponse(
                message: "Dữ liệu đăng nhập không hợp lệ",
                errors: ModelState,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        var result = await _authService.LoginAsync(request);

        HttpContext.Response.WriteAuthCookies(result.AccessToken, result.RefreshToken);

        return Ok(ApiResponseFactory.SuccessResponse(
            data: null,
            traceId: HttpContext.TraceIdentifier
        ));
    }
}