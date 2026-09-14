namespace HoaTheoMua.Service.AiService;

public class Response
{
    public record GeneratedContent(string Title, string Content, string Hashtag, string? Model,
        string? RequestId, int DurationMs);
}
