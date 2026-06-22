using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.JobDescriptions;

public interface IService
{
    Task<Response.CreateResponse> CreateJobAsync (Request.CreateRequest request);
    
    Task<BasePaginationResponse> GetPagedJobsAsync(int page, int pageSize, string? title, string? location, EmploymentType? employmentType, float? salaryMin, float? salaryMax, StatusJobDescriptions? status = null);
    Task<BaseResponse> GetJobByIdOrSlugAsync(string idOrSlug);
    Task<BaseResponse> GetPublicJobByIdOrSlugAsync(string idOrSlug);
    Task<BaseResponse> GetHighestSalaryAsync();
    Task<BaseResponse> UpdateJobAsync(Guid id, Request.UpdateRequest request);
    Task DeleteJobAsync(Guid id);
}