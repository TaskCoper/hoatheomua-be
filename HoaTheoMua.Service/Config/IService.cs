using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Config;

public interface IService
{
    Task<BasePaginationResponse> GetConfig(int pageIndex, int paegSize);

    Task<Response.ConfigResponse?> GetConfigById(Guid id);

    Task<Response.ConfigResponse> CreateConfig(Request.CreateConfigRequest request);

    Task<string> UpdateConfig(Guid id, Request.UpdateConfigRequest request);

    Task<string> DeleteConfig(Guid id);
}