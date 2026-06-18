using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.New;

public interface IService
{
    Task<BasePaginationResponse> GetNewsAsync(int pageIndex, int pageSize, string? keyword, NewsStatus? status);
    Task<NewResponse?> GetNewByIdAsync(string key);
    Task<NewsResponseV2> CreateNewAsync(CreateNewRequest request);
    Task<NewsResponseV2?> UpdateNewAsync(Guid id, UpdateNewRequest request);
    Task<bool> DeleteNewAsync(Guid id);
}
