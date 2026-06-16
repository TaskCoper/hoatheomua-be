using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

    [ApiController]
[Route("api/v1/config")]
public class TableConfigurationController : ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        return Ok();
        
    }
    
    
    [HttpPost]
    public async Task<IActionResult> CreateConfig()
        
    {

        return Ok();
    }

    [HttpPut("")]
    
    public async Task<IActionResult> UpdateConfig()

    {
        return Ok();
    }

    [HttpDelete("")]
    
    public async Task<IActionResult> DeleteConfig()
    {
        return Ok();
    }
}
