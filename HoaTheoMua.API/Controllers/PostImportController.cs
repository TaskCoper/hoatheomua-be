using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.PostImport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Authorize(Policy = JwtExtensions.AdminPolicy)]
public class PostImportController(IService service) : ControllerBase
{
    [HttpGet("/api/v1/post-imports/defaults")]
    public async Task<IActionResult> Defaults(CancellationToken ct) => Ok(ApiResponseFactory.Base(await service.GetSettings(ct), traceId: HttpContext.TraceIdentifier));

    [HttpPut("/api/v1/post-imports/defaults")]
    public async Task<IActionResult> Defaults(Request.Settings request, CancellationToken ct) => Ok(ApiResponseFactory.Base(await service.SaveSettings(request, ct), traceId: HttpContext.TraceIdentifier));

    [HttpPost("/api/v1/post-imports")]
    [RequestSizeLimit(WorkbookReader.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file, [FromHeader(Name = "Idempotency-Key")] string requestKey, CancellationToken ct)
    {
        if (file.Length > WorkbookReader.MaxFileBytes) throw new ImportException(413, "IMPORT_FILE_TOO_LARGE", "File Excel tối đa 10 MB.");
        await using var stream = file.OpenReadStream();
        var result = await service.Import(stream, file.FileName, requestKey, ct);
        return Accepted($"/api/v1/post-imports/{result.Id}", ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }
    [HttpGet("/api/v1/post-imports")]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(ApiResponseFactory.Base(await service.List(ct), traceId: HttpContext.TraceIdentifier));

    [HttpGet("/api/v1/post-imports/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
        => Ok(ApiResponseFactory.Base(await service.Get(id, pageIndex, pageSize, ct), traceId: HttpContext.TraceIdentifier));

    [HttpGet("/api/v1/post-imports/{id:guid}/export")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
        => File(await service.Export(id, ct), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"post-import-{id:N}.xlsx");
}
