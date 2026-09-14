using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.FlowerReference;

public interface IService
{
    public Task<BasePaginationResponse> GetFlowerReference(int pageIndex, int pageSize, bool? isActive, string? type, string? search = null);
    public Task<Response.FlowerReferenceResponse> CreateFlowerReference(Request.CreateFlowerReferenceRequest request);
    public Task<Response.FlowerReferenceResponse> ToggleFlowerReferenceStatus(Guid id);
    public Task<Response.DeleteFlowerReferenceResponse> DeleteFlowerReference(Guid id);
}
