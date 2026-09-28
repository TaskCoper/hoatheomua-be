using System.Text.Json;
using HoaTheoMua.Extentions;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Revalidate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IService = HoaTheoMua.Service.Revalidate.IService;
using NewService = HoaTheoMua.Service.New;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/news")]
public class NewController : ControllerBase
{
    private readonly NewService.IService _newService;
    private readonly IService _revalidateService;
    private readonly string _newsViewCookieName;

    public NewController(
        NewService.IService newService,
        IService revalidateService,
        IConfiguration configuration)
    {
        _newService = newService;
        _revalidateService = revalidateService;
        _newsViewCookieName = configuration["NewsView:CookieName"]
            ?? NewService.NewsViewConstants.CookieName;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] NewsStatus? status = null)
    {
        var result = await _newService.GetNewsAsync(pageIndex, pageSize, keyword, status);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetNewById(string key)
    {
        var clientContext = Request.Cookies[_newsViewCookieName];

        if (string.IsNullOrWhiteSpace(clientContext))
        {
            clientContext = Guid.NewGuid().ToString("N");

            Response.Cookies.Append(
                _newsViewCookieName,
                clientContext,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });
        }

        var result = await _newService.GetNewByIdAsync(key, clientContext);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    
    public async Task<IActionResult> CreateNew([FromBody] NewService.CreateNewRequest request)
    {
        var result = await _newService.CreateNewAsync(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "news",
            slug = result.slug ?? result.Id.ToString(),
        });
        await _revalidateService.RevalidateApi(json);
        return CreatedAtAction(nameof(GetNewById), new { key = result.slug ?? result.Id.ToString() }, ApiResponseFactory.Base(result, true, "Tạo bài viết tin tức mới thành công.", HttpContext.TraceIdentifier));
    }

    [HttpPut("{identifier}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    
    public async Task<IActionResult> UpDateNew(string identifier, [FromBody] NewService.UpdateNewRequest request)
    {
        var result =  await _newService.UpdateNewAsync(identifier, request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "news",
            slug = result?.slug ?? result?.Id.ToString(),
        });
        await _revalidateService.RevalidateApi(json);

        return Ok(ApiResponseFactory.Base("Cập nhật thông tin bài viết tin tức thành công.", true, "", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{identifier}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    
    public async Task<IActionResult> DeleteNew(string identifier)
    {
        var result = await _newService.DeleteNewAsync(identifier);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "news",
            slug = result.Slug ?? result.Id.ToString(),
        });
        await _revalidateService.RevalidateApi(json);

        
        return Ok(ApiResponseFactory.Base("Xóa bài viết thành công.", true, "", HttpContext.TraceIdentifier));
    }
}
