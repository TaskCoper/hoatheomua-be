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
    
    [HttpPost("/api/v1/products/nhanh-vn/list")]
    public async Task<IActionResult> SearchNhanhVnProducts([FromBody] Request.ProductListRequest request)
    {
        var result = await _productService.GetListProducts(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    [HttpPost("/api/v1/products/nhanh-vn/details")]
    public async Task<IActionResult> SearchNhanhVnProducts([FromBody] Request.ProductDetailRequest request)
    {
        var result = await _productService.GetProductDetails(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/products")]
    public async Task<IActionResult> GetLocalProducts([FromQuery] Request.LocalProductQueryRequest request)
    {
        var result = await _productService.GetLocalProducts(request);
        return Ok(result);
    }

    [HttpGet("/api/v1/products/max-price")]
    public async Task<IActionResult> GetMaxPrice([FromQuery] Guid? categoryId)
    {
        var result = await _productService.GetMaxPrice(categoryId);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/products/{id:guid}")]
    public async Task<IActionResult> GetLocalProductDetail(Guid id)
    {
        var result = await _productService.GetLocalProductDetail(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("/api/v1/products/{id:guid}/move")]
    public async Task<IActionResult> MoveProduct(Guid id, Request.MoveProductRequest request)
    {
        var result = await _productService.MoveProduct(id, request);
        return Ok(result);
    }
}