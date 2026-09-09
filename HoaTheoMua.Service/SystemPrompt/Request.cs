using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.SystemPrompt;

public class Request
{
    public class SystemPromptRequest
    {
        public required string Content { get; set; }
        public SystemPromptType Type { get; set; }
    }

    public class UpdateSystemPromptRequest
    {
        public string? Content { get; set; }
    }
}