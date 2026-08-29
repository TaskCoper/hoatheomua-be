using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.RecruitmentRequests;

public interface IService
{
    Task<BasePaginationResponse> GetPagedRecruitmentRequestsAsync(int pageIndex, int pageSize, bool? isRead);
    Task<BasePaginationResponse> GetMyRecruitmentRequestsAsync(int pageIndex, int pageSize);
    Task<BaseResponse> GetRecruitmentRequestByIdAsync(Guid id);
    Task<BaseResponse> MarkAsReadAsync(Guid id);
    Task<Response.CreateRecruitmentResponse> CreateRecruitmentRequestAsync(Request.CreateRecruitmentRequest request);
}
