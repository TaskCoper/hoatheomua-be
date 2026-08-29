using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.RecruitmentRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/recruitment-requests")]
public class RecruitmentRequestsController : ControllerBase
{
    private readonly IService _recruitmentRequestService;

    public RecruitmentRequestsController(IService recruitmentRequestService)
    {
        _recruitmentRequestService = recruitmentRequestService;
    }

    [HttpGet]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetRecruitmentRequests(int page = 1,  int pageSize = 10,  bool? isRead = null)
    {
        var result = await _recruitmentRequestService.GetPagedRecruitmentRequestsAsync(page, pageSize, isRead);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyRecruitmentRequests(int pageIndex = 1, int pageSize = 10)
    {
        var result = await _recruitmentRequestService.GetMyRecruitmentRequestsAsync(pageIndex, pageSize);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetRecruitmentRequestById(Guid id)
    {
        var result = await _recruitmentRequestService.GetRecruitmentRequestByIdAsync(id);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _recruitmentRequestService.MarkAsReadAsync(id);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRecruitmentRequest([FromBody] Request.CreateRecruitmentRequest request)
    {
        var result = await _recruitmentRequestService.CreateRecruitmentRequestAsync(request);
        return CreatedAtAction(nameof(GetRecruitmentRequestById), new { id = result.id }, ApiResponseFactory.Base("Success", true, null, HttpContext.TraceIdentifier));
    }
}
