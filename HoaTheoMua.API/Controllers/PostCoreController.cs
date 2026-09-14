using HoaTheoMua.Extentions;
using HoaTheoMua.Service.PostCore;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]
public class PostCoreController(IService service) : ControllerBase
{
    [HttpPost("/api/v1/posts")]
    public async Task<IActionResult> CreatePostCore(Request.CreatePostCoreRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreatePostCore(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/posts")]
    public async Task<IActionResult> GetPostCoreList([FromQuery] Request.GetPostCoreListRequest request, CancellationToken cancellationToken)
    {
        var result = await service.GetPostCoreList(request, cancellationToken);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("/api/v1/posts/{id:guid}")]
    public async Task<IActionResult> GetPostCoreDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetPostCoreDetail(id, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("/api/v1/posts/{id:guid}")]
    public async Task<IActionResult> UpdatePostCore(Guid id, Request.UpdatePostCoreRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdatePostCore(id, request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("/api/v1/posts/{id:guid}")]
    public async Task<IActionResult> DeletePostCore(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeletePostCore(id, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

}
