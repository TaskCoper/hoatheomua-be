namespace HoaTheoMua.Service.AiService;

public class Response
{
    public record GeneratedImage(string ImageUrl, string? Model, string? RequestId, int DurationMs);
    public record GeneratedContent(string Title, string Content, string Hashtag, string? Model,
        string? RequestId, int DurationMs);
    public record FlowerGeneratedResponse(string FlowerImageUrl, string FlowerWithMockupImageUrl,
    string? Model, string? RequestId, int DurationMs,
    GeneratedImage? FlowerGeneration = null, GeneratedImage? MockupGeneration = null);
}
