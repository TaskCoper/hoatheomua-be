using System.Text.Json;
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.CategoryNew;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("/api/v1/category-news")]

public class CategoryNewController:ControllerBase
{
    private readonly IService _categoryService;
    private readonly Service.Revalidate.IService _revalidateService;
    public CategoryNewController(IService categoryService,  Service.Revalidate.IService revalidateService)
    {
        _categoryService = categoryService;
        _revalidateService = revalidateService;
    }
    
    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateCate([FromBody]Request.CreateCategoriesRequest request) 
    {
        var result = await _categoryService.CreateCate(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "newsList"
        });
        await _revalidateService.RevalidateApi(json);

        return CreatedAtAction(nameof(GetCategory), new { id = result.categoryId }, ApiResponseFactory.Base("Success", true, "Tạo bài viết tin tức mới thành công.", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(int pageIndex, int pageSize, string? keyword)
    {
        var result = await _categoryService.GetCategories(pageIndex, pageSize, keyword);
        return Ok(result);
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
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "newsList"
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteCate(Guid id)
    {
        await _categoryService.DeleteCate(id);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "newsList"
        });
        await _revalidateService.RevalidateApi(json);
        return Ok(ApiResponseFactory.Base(new { message = "Xóa danh mục thành công." }, true, "", HttpContext.TraceIdentifier));
    }
}