using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.New;

public interface IService
{
    Task<BasePaginationResponse> GetNewsAsync(int pageIndex, int pageSize, string? keyword, NewsStatus? status);
    Task<NewResponse?> GetNewByIdAsync(string key);
    Task<NewResponseV2> CreateNewAsync(CreateNewRequest request);
    Task<NewResponseV2?> UpdateNewAsync(string identifier, UpdateNewRequest request);
    Task<ClassDeleteResponse> DeleteNewAsync(string identifier);
}
