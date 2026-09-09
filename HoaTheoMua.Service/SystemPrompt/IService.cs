namespace HoaTheoMua.Service.SystemPrompt;

public interface IService
{
    public Task<string> CreateSystemPromptDefault(Request.SystemPromptRequest request);
    public Task<List<Response.AllSystemPromptResponse>> GetSystemPrompt();
    public Task<Response.SystemPromptResponse> GetSystemPromptInfo(Guid id);
    public Task<Response.SystemPromptResponse> UpdateSystemPrompt(Guid id, Request.UpdateSystemPromptRequest request);
}