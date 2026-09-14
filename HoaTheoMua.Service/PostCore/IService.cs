using HoaTheoMua.Service.Models;
namespace HoaTheoMua.Service.PostCore;

public interface IService
{
    Task<Response.PostCoreDetail> CreatePostCore(Request.CreatePostCoreRequest request, CancellationToken cancellationToken = default);
    Task<BasePaginationResponse> GetPostCoreList(Request.GetPostCoreListRequest request, CancellationToken cancellationToken = default);
    Task<Response.PostCoreDetail> GetPostCoreDetail(Guid id, CancellationToken cancellationToken = default);
    Task<Response.PostCoreDetail> UpdatePostCore(Guid id, Request.UpdatePostCoreRequest request, CancellationToken cancellationToken = default);
    Task<Response.DeletedPost> DeletePostCore(Guid id, CancellationToken cancellationToken = default);
}
