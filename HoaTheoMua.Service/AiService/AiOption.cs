namespace HoaTheoMua.Service.AiService;

public class AiOption
{
    public string ChatCompletionsUrl { get; set; } = "https://proxy.hoatheomua.com/v1/chat/completions";
    public string Model { get; set; } = "chatgpt/gpt-5.6";
    // Authentication is currently optional; set this when the proxy enables API keys.
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 120;
}
