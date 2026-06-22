using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Config;

public interface IService
{
    Task<List<Response.ConfigResponse>> GetConfig();

    Task<Response.ConfigResponse?> GetConfigById(Guid id);

    Task<Response.ConfigResponse> CreateConfig(Request.CreateConfigRequest request);

    Task<string> UpdateConfig(Guid id, Request.UpdateConfigRequest request);

    Task<string> DeleteConfig(Guid id);
    
    Task<List<Response.ConfigResponse>> GetConfigIsPublic();
}