namespace HoaTheoMua.Service.AiService;

public interface IService
{
    Task<Response.GeneratedContent> GenerateContent(Request.ContentInput input, CancellationToken cancellationToken = default);
    Task<Response.GeneratedImage> GenerateImage(Request.ImageInput input, CancellationToken cancellationToken = default);
    Task<Response.FlowerGeneratedResponse> GenerateFlowerImage(Request.FlowerImageInput input, CancellationToken cancellationToken = default);
}
