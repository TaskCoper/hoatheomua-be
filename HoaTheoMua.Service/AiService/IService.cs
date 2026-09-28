namespace HoaTheoMua.Service.AiService;

public interface IService
{
    Task<Response.GeneratedImage> GenerateCardImage(Request.CardImageInput input, CancellationToken cancellationToken = default);
    Task<Response.GeneratedContent> GenerateContent(Request.ContentInput input, CancellationToken cancellationToken = default);
    Task<Response.GeneratedImage> GenerateImage(Request.ImageInput input, CancellationToken cancellationToken = default);
    Task<Response.FlowerGeneratedResponse> GenerateFlowerImage(Request.FlowerImageInput input, CancellationToken cancellationToken = default);
    Task<Response.GeneratedImage> GenerateFlowerOnly(Request.FlowerImageInput input, CancellationToken cancellationToken = default);
    Task<Response.GeneratedImage> ApplyFlowerMockup(Request.FlowerImageInput input, Response.GeneratedImage flower, CancellationToken cancellationToken = default);
}
