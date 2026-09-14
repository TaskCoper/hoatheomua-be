namespace HoaTheoMua.Service.AiService;

public interface IService
{
    Task<Response.GeneratedContent> GenerateContent(Request.ContentInput input, CancellationToken cancellationToken = default);
}
