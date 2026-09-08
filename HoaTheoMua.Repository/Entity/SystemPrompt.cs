using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class SystemPrompt
{
    public Guid Id { get; set; }
    public SystemPromptType Type { get; set; }
    public string Content { get; set; } = null!;
}
