using HoaTheoMua.Service.AuthService;
using HoaTheoMua.Service.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private const int AccessTokenMaxAgeSeconds = 900;
    private const int RefreshTokenMaxAgeSeconds = 2592000;

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(request, ipAddress, userAgent);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message
            });
        }

        Response.Cookies.Append("accessToken", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromSeconds(AccessTokenMaxAgeSeconds)
        });

        Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/auth",
            MaxAge = TimeSpan.FromSeconds(RefreshTokenMaxAgeSeconds)
        });

        return Ok(new
        {
            message = result.Message
        });
    }
}