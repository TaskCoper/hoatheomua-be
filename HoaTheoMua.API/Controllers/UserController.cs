
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.User;
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
}