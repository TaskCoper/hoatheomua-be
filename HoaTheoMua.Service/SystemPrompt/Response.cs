using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.SystemPrompt;

public class Response
{
    public class SystemPromptResponse
    {
        public Guid Id { get; set; }
        public SystemPromptType Type { get; set; }
        public string Content { get; set; } = null!;
    }
    
    public class AllSystemPromptResponse
    {
        public Guid Id { get; set; }
        public SystemPromptType Type { get; set; }
    }
}