using HoaTheoMua.Extentions;
using HoaTheoMua.Service.CategoryNew;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("/api/category-news")]

public class CategoryNewController:ControllerBase
{
    private readonly IService _categoryService;
    public CategoryNewController(IService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateCate([FromBody]Request.CreateCategoriesRequest request) 
    {
        var result = await _categoryService.CreateCate(request);
        return Ok(ApiResponseFactory.Base(result,true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(int pageIndex, int pageSize)
    {
        var result = await _categoryService.GetCategories(pageIndex, pageSize);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var result = await _categoryService.GetCategory(id);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateCate(Guid id, [FromBody] Request.UpdateCategoriesRequest request)
    {
        var result = await _categoryService.UpdateCate(id, request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteCate(Guid id)
    {
        await _categoryService.DeleteCate(id);
        return Ok(ApiResponseFactory.Base(new { message = "Xóa danh mục thành công." }, true, "", HttpContext.TraceIdentifier));
    }
}