using System.Text.Json;
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Controllers;

[ApiController]
public class ProductController: ControllerBase
{
    private readonly IService _productService;
    private readonly Service.Revalidate.IService _revalidateService;
    
    public ProductController(IService productService, Service.Revalidate.IService revalidateService)
    {
        _productService = productService;
        _revalidateService = revalidateService;
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

    [HttpPost("/api/v1/product")]
    public async Task<IActionResult> GetLocalProductByList([FromBody] Request.GetProductWithList request)
    {
        var result = await _productService.GetProductWithList(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v2/products")]
    public async Task<IActionResult> GetLocalProductsV2([FromQuery] Request.LocalProductQueryRequest request)
    {
        var result = await _productService.GetLocalProductsV2(request);
        return Ok(result);
    }

    [HttpGet("/api/v1/products/max-price")]
    public async Task<IActionResult> GetMaxPrice([FromQuery] List<Guid>? categoryIds)
    {
        var result = await _productService.GetMaxPrice(categoryIds);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/products/{id:guid}")]
    public async Task<IActionResult> GetLocalProductDetail(Guid id)
    {
        var result = await _productService.GetLocalProductDetail(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("/api/v1/products/{id:guid}/move")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> MoveProduct(Guid id, Request.MoveProductRequest request)
    {
        var result = await _productService.MoveProduct(id, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product",
            id = new[] { id },
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(result);
    }
    
    [HttpPut("/api/v1/products/pin")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> PinProduct(Request.PinProductRequest request)
    {
        var result = await _productService.PinProduct(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "product",
            id = new[] { request.ProductId },
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(result);
    }

    [HttpGet("/api/v1/products-pin")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetProductPin([FromQuery] Request.GetProductPinRequest request)
    {
        var result = await _productService.GetProductPin(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    
    [HttpPatch("/api/v1/products/{id:guid}/update")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateProduct(Guid id ,[FromBody] Request.UpdateProductRequest request)
    {
        var result = await _productService.UpdateProduct(id, request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/products/{id}/combo-specification")]
    public async Task<IActionResult> GetComboSpecification(Guid id)
    {
        var result = await _productService.GetComboSpecificationAsync(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/materials")]
    public async Task<IActionResult> GetMaterials([FromQuery] Request.GetMaterialsRequest request)
    {
        var result = await _productService.GetMaterialsAsync(request);
        return Ok(result);
    }
}
