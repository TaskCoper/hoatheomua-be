using System.Text.Json;
using HoaTheoMua.Extentions;
using CategoryProductService = HoaTheoMua.Service.CategoryProduct;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Revalidate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("/api/v1/category-products")]
public class CategoryProductController : ControllerBase
{
    private readonly CategoryProductService.IService _categoryProductService;
    private readonly IService _revalidateService;

    public CategoryProductController(CategoryProductService.IService categoryProductService, IService revalidateService)
    {
        _categoryProductService = categoryProductService;
        _revalidateService = revalidateService;
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateCategoryProduct([FromBody] CategoryProductService.Request.CreateCategoryProductRequest request)
    {
        var result = await _categoryProductService.CreateCateProduct(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "category"
        });
        await _revalidateService.RevalidateApi(json);
        return CreatedAtAction(nameof(GetCategoryProduct), new { id = result.categoryId }, ApiResponseFactory.Base(result, true, "Tạo danh mục sản phẩm mới thành công.", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoryProducts(int pageIndex, int pageSize, string? keyword)
    {
        var result = await _categoryProductService.GetCateProducts(pageIndex, pageSize, keyword);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryProduct(Guid id)
    {
        var result = await _categoryProductService.GetCateProduct(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateCategoryProduct(Guid id, [FromBody] CategoryProductService.Request.UpdateCategoryProductRequest request)
    {
        var result = await _categoryProductService.UpdateCateProduct(id, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "category"
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}/products")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> AssignProductsToCategory(Guid id, [FromBody] CategoryProductService.Request.AssignProductsToCategoryRequest request)
    {
        await _categoryProductService.AssignProductsToCategory(id, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "category"
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(new { message = "Gán danh sách sản phẩm vào danh mục thành công." }, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteCategoryProduct(Guid id)
    {
        await _categoryProductService.DeleteCateProduct(id);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "category"
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(new { message = "Xóa danh mục sản phẩm thành công." }, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("with-children")]
    public async Task<IActionResult> GetCategoryWithChild([FromQuery] CategoryProductService.Request.GetCategoryProductWithChild request)
    {
        var result = await _categoryProductService.GetCategoryWithChild(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("detail")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateCategoryProductDetail(CategoryProductService.Request.CategoryProductDetailRequest request)
    {
        var result = await _categoryProductService.UpdateCategoryProductDetail(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "category"
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
}
