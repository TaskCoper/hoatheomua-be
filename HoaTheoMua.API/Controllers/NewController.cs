using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewService = HoaTheoMua.Service.New;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewController : ControllerBase
{
    private readonly NewService.IService _newService;

    public NewController(NewService.IService newService)
    {
        _newService = newService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] NewsStatus? status = null)
    {
        var result = await _newService.GetNewsAsync(pageIndex, pageSize, keyword, status);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNewById(Guid id)
    {
        var result = await _newService.GetNewByIdAsync(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateNew([FromBody] NewService.CreateNewRequest request)
    {
        var result = await _newService.CreateNewAsync(request);
        return CreatedAtAction(nameof(GetNewById), new { id = result.Id }, ApiResponseFactory.Base(result, true, "Tạo bài viết tin tức mới thành công.", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpDateNew(Guid id, [FromBody] NewService.UpdateNewRequest request)
    {
        await _newService.UpdateNewAsync(id, request);
        return Ok(ApiResponseFactory.Base("Cập nhật thông tin bài viết tin tức thành công.", true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNew(Guid id)
    {
        await _newService.DeleteNewAsync(id);
        return Ok(ApiResponseFactory.Base("Xóa bài viết thành công.", true, "", HttpContext.TraceIdentifier));
    }
}
