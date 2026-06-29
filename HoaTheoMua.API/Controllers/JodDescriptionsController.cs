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
    
    private readonly IService _jobDescriptionService;

    public JodDescriptionsController(IService jobDescriptionService)
    {
        _jobDescriptionService = jobDescriptionService;
    }


    [HttpPost]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateJobDescription(Request.CreateRequest request)
    {
        var result = await _jobDescriptionService.CreateJobAsync(request);
        return CreatedAtAction(nameof(GetJobDescriptionByIdOrSlug), new { key = result.id }, ApiResponseFactory.Base("Success", true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetJobDescription( int page = 1,  int pageSize = 10,  string? title = null,  string? location = null,  EmploymentType? employmentType = null,  float? salaryMin = null, float? salaryMax = null, StatusJobDescriptions? status = null)
    {
        var result = await _jobDescriptionService.GetPagedJobsAsync(page, pageSize, title, location, employmentType, salaryMin, salaryMax, status);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("highest-salary")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetHighestSalary()
    {
        var result = await _jobDescriptionService.GetHighestSalaryAsync();
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("{key}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> GetJobDescriptionByIdOrSlug(string key)
    {
        var result = await _jobDescriptionService.GetJobByIdOrSlugAsync(key);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateJobDescription(Guid id, [FromBody] Request.UpdateRequest request)
    {
        var result = await _jobDescriptionService.UpdateJobAsync(id, request);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteJobDescription(Guid id)
    {
        await _jobDescriptionService.DeleteJobAsync(id);
        return NoContent();
    }
    [HttpGet("locations")]
    public async Task<IActionResult> GetAllLocation()
    {
        var result = await _jobDescriptionService.GetAllLocation();
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }
    [HttpGet("/api/v1/public/job-descriptions")]
    
    public async Task<IActionResult> GetPublicJobDescriptions( int page = 1,  int pageSize = 10,  string? title = null,  string? location = null,  EmploymentType? employmentType = null,  float? salaryMin = null,  float? salaryMax = null)
    {
        var result = await _jobDescriptionService.GetPagedJobsAsync(page, pageSize, title, location, employmentType, salaryMin, salaryMax, StatusJobDescriptions.Open);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpGet("/api/v1/public/job-descriptions/{key}")]
    public async Task<IActionResult> GetPublicJobDescriptionByIdOrSlug(string key)
    {
        var result = await _jobDescriptionService.GetPublicJobByIdOrSlugAsync(key);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }
    

}