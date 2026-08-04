using System.Text.Json;
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
    private readonly Service.Revalidate.IService _revalidateService;

    public JodDescriptionsController(IService jobDescriptionService,  Service.Revalidate.IService revalidateService)
    {
        _jobDescriptionService = jobDescriptionService;
        _revalidateService = revalidateService;
    }


    [HttpPost]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateJobDescription(Request.CreateRequest request)
    {
        var result = await _jobDescriptionService.CreateJobAsync(request);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "recruitment",
            slug = result.Slug,
        });
        await _revalidateService.RevalidateApi(json);
        
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

    [HttpPut("{identifier}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateJobDescription(string identifier, [FromBody] Request.UpdateRequest request)
    {
        var result = await _jobDescriptionService.UpdateJobAsync(identifier, request);

        var data = (Response.JobDetailResponse)result.Value!;
        
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "recruitment",
            slug = data.Slug ?? data.Id.ToString(),
        });
        await _revalidateService.RevalidateApi(json);
        result.TraceId = HttpContext.TraceIdentifier;
        return Ok(result);
    }

    [HttpDelete("{identifier}")]
    [Authorize (Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> DeleteJobDescription(string identifier)
    {
        var result = await _jobDescriptionService.DeleteJobAsync(identifier);
        JsonElement json = JsonSerializer.SerializeToElement(new
        {
            entity = "recruitment",
            slug = result.Slug ??  result.Id.ToString(),
        });
        await _revalidateService.RevalidateApi(json);
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
