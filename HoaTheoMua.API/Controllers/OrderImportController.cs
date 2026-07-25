using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.OrderNhanhVnImport;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;


[ApiController]
public class OrderImportController: ControllerBase
{
    private readonly IService _orderImportService;
    
    public OrderImportController(IService orderImportService)
    {
        _orderImportService = orderImportService;
    }
    
    [HttpPost("/api/v1/orders/import-excel")]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var result = await _orderImportService.ImportFromExcelOrderNhanhVn(stream);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
}