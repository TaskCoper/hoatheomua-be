using System.Threading.Tasks;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.ProductImport;
using Microsoft.AspNetCore.Http;
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
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(
                title: "Lỗi dữ liệu đầu vào",
                status: 400,
                detail: "Vui lòng chọn file Excel hợp lệ",
                messageCode: "FILE_REQUIRED",
                errors: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        using var stream = file.OpenReadStream();
        var result = await _importService.ImportFromExcel(stream);

        return Ok(ApiResponseFactory.Base(
            data: result,
            isSuccess: true,
            errorCustom: null,
            traceId: HttpContext.TraceIdentifier
        ));
    }
}
