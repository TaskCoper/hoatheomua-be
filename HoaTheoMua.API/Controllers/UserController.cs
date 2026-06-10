using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Auth;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.User;
using Microsoft.AspNetCore.Mvc;
using IService = HoaTheoMua.Service.Auth.IService;
using Request = HoaTheoMua.Service.Auth.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
public class UserController: ControllerBase
{
    private readonly IService _authService;
    public UserController(IService userService)
    {
        _authService = userService;
    }

    [HttpGet("api/v1/auth/me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _authService.GetMe();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/register")]
    public async Task<IActionResult> RegisterTask([FromBody]Request.RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery]Request.VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmail(request);
        if (!string.IsNullOrEmpty(result!.AccessToken) && !string.IsNullOrEmpty(result.RefreshToken))
        {
            HttpContext.Response.WriteAuthCookies(result.AccessToken, result.RefreshToken);
        }
        return Ok(ApiResponseFactory.SuccessResponse(result,"", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/auth/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var result = await _authService.RefreshToken();
        HttpContext.Response.WriteAuthCookies(result.AccessToken, result.RefreshToken);
        return Ok(ApiResponseFactory.SuccessResponse(null, "success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/sign-in")]
    public async Task<IActionResult> SignIn([FromBody] Request.LoginRequest request)
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
            data: result.Message,
            "success",
            traceId: HttpContext.TraceIdentifier
        ));
    }
}