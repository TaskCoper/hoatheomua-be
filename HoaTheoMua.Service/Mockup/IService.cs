using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Mockup;

public interface IService
{
    public Task<BasePaginationResponse> GetMockup(int pageIndex, int pageSize, bool? isActive, string? search = null);
    public Task<Response.MockupResponse> CreateMockup(Request.CreateMockupRequest request);
    public Task<Response.MockupResponse> ToggleMockupStatus(Guid id);
    public Task<Response.DeleteMockupResponse> DeleteMockup(Guid id);
}