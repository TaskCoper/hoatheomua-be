using HoaTheoMua.Extentions;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.JobDescriptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/job-descriptions")]
public class JodDescriptionsController: ControllerBase
{
    
    private readonly IService  _JobDescriptionService;

    public JodDescriptionsController(IService jobDescriptionService)
    {
        _JobDescriptionService = jobDescriptionService;
    }


    [HttpPost]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateJobDescription(Request.CreateRequest request)
    {
        var result = await _JobDescriptionService.CreateJobAsync(request);
        return CreatedAtAction(nameof(GetJobDescriptionById), new { id = result.id }, ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetJobDescription( int page = 1,  int pageSize = 10,  string? title = null,  string? location = null,  EmploymentType? employmentType = null,  float? salaryMin = null, float? salaryMax = null)
    {
        var result = await _JobDescriptionService.GetPagedJobsAsync(page, pageSize, title, location, employmentType, salaryMin, salaryMax);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetJobDescriptionById(Guid id)
    {
        var result = await _JobDescriptionService.GetJobByIdAsync(id);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateJobDescription(Guid id, [FromBody] Request.UpdateRequest request)
    {
        var result = await _JobDescriptionService.UpdateJobAsync(id, request);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteJobDescription(Guid id)
    {
        await _JobDescriptionService.DeleteJobAsync(id);
        return NoContent();
    }

    [HttpGet("/api/v1/public/job-descriptions")]

    public async Task<IActionResult> GetPublicJobDescriptions( int page = 1,  int pageSize = 10,  string? title = null,  string? location = null,  EmploymentType? employmentType = null,  float? salaryMin = null,  float? salaryMax = null)
    {
        var result = await _JobDescriptionService.GetPagedJobsAsync(page, pageSize, title, location, employmentType, salaryMin, salaryMax, StatusJobDescriptions.Open);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("/api/v1/public/job-descriptions/{id}")]

    public async Task<IActionResult> GetPublicJobDescriptionById(Guid id)
    {
        var result = await _JobDescriptionService.GetPublicJobByIdAsync(id);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("/api/v1/public/job-descriptions/slug/{slug}")]

    public async Task<IActionResult> GetJobDescriptionBySlug(string slug)
    {
        var result = await _JobDescriptionService.GetJobBySlugAsync(slug);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }
}