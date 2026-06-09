using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    // 1. Kiểm thử Policy "api" (Sliding Window: 100 requests / 1 phút)
    [HttpGet("normal")]
    [EnableRateLimiting("api")]
    public IActionResult TestNormalApi()
    {
        return Ok(new 
        { 
            Message = "Thành công gọi API Normal!", 
            Timestamp = DateTime.UtcNow 
        });
    }

    // 2. Kiểm thử Policy "login" (Fixed Window: 5 requests / 1 phút)
    [HttpGet("login")]
    [EnableRateLimiting("login")]
    public IActionResult TestLoginApi()
    {
        return Ok(new 
        { 
            Message = "Thành công gọi API Login!", 
            Timestamp = DateTime.UtcNow 
        });
    }

    // 3. Kiểm thử Policy "heavy" (Token Bucket: Tối đa 10, hồi 5 token / phút)
    [HttpGet("heavy")]
    [EnableRateLimiting("heavy")]
    public IActionResult TestHeavyApi()
    {
        return Ok(new 
        { 
            Message = "Thành công gọi API Tác vụ nặng!", 
            Timestamp = DateTime.UtcNow 
        });
    }

    // 4. Bỏ qua Rate Limit (Trường hợp đặc biệt muốn API không bị giới hạn)
    [HttpGet("bypass")]
    [DisableRateLimiting] 
    public IActionResult TestBypassApi()
    {
        return Ok(new 
        { 
            Message = "API này không bao giờ bị giới hạn băng thông.", 
            Timestamp = DateTime.UtcNow 
        });
    }
}