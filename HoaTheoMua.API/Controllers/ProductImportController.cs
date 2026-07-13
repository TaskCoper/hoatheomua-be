using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.ProductImport;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class ProductImportController : ControllerBase
{
    private readonly IService _importService;

    public ProductImportController(IService importService)
    {
        _importService = importService;
    }

    [HttpPost("/api/v1/products/import-excel")]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await _importService.ImportFromExcel(stream);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
