using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Config;

public interface IService
{
    Task<List<Response.ConfigResponse>> GetConfig();

    Task<Response.ConfigResponse?> GetConfigById(Guid id);
    
    Task<List<Response.ConfigResponse>> GetConfigIsPublic();
    Task<List<Response.ConfigGroupResponse>> GetConfigGroups(string groupName);
    ///
    
    Task<List<Response.ConfigResponse>> GetConfigContent(bool? isPublic);
    Task<List<Response.ConfigResponse>> GetConfigSetting();
    Task<List<Response.ConfigResponse>> GetConfigSecret();
    Task<Response.RevealResponse> GetConfigReveal(Guid id);
    Task<string> UpdateConfig(Guid id, Request.UpdateConfigRequest request);
    Task<Response.ConfigResponse> CreateConfig(Request.CreateConfigRequest request);
    Task<string> DeleteConfig(Guid id);
}