using HoaTheoMua.Service.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly IService _storageService;

    public FileController(IService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("upload-url")]
    public async Task<IActionResult> GetUploadUrl([FromBody] Request.GetUploadUrlRequest request)
    {
        var result = await _storageService.GenerateUploadUrlAsync(request);
        return Ok(result);
    }

    [HttpPost("{fileId:guid}/confirm")]
    public async Task<IActionResult> ConfirmUpload(Guid fileId)
    {
        var result = await _storageService.ConfirmUploadAsync(fileId);
        return Ok(result);
    }

    [HttpGet("{fileId:guid}/download-url")]
    public async Task<IActionResult> GetDownloadUrl(Guid fileId, [FromQuery] int expirationMinutes = 60)
    {
        var result = await _storageService.GenerateDownloadUrlAsync(fileId, expirationMinutes);
        return Ok(result);
    }
}
