using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.New;

public interface IService
{
    Task<BasePaginationResponse> GetNewsAsync(int pageIndex, int pageSize, string? keyword, NewsStatus? status);
    Task<NewResponse?> GetNewByIdAsync(Guid id);
    Task<NewResponse> CreateNewAsync(CreateNewRequest request);
    Task<NewResponse?> UpdateNewAsync(Guid id, UpdateNewRequest request);
    Task<bool> DeleteNewAsync(Guid id);
}
