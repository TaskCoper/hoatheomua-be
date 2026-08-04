using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.New;

public interface IService
{
    Task<BasePaginationResponse> GetNewsAsync(int pageIndex, int pageSize, string? keyword, NewsStatus? status);
    Task<NewResponse?> GetNewByIdAsync(string key);
    Task<NewResponseV2> CreateNewAsync(CreateNewRequest request);
    Task<NewResponseV2?> UpdateNewAsync(Guid id, UpdateNewRequest request);
    Task<ClassDeleteResponse> DeleteNewAsync(Guid id);
}
