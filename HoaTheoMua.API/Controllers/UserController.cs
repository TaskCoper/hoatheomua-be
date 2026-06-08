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
    
}