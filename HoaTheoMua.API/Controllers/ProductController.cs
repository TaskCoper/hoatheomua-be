using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
public class ProductController: ControllerBase
{
    private readonly IService _productService;
    
    public ProductController(IService productService)
    {
        _productService = productService;
    }
    
    [HttpPost("/api/v1/products/nhanh-vn/search")]
    public async Task<IActionResult> SearchNhanhVnProducts([FromBody] Request.ProductListRequest request)
    {
        var result = await _productService.GetListProducts(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
}