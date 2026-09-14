using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class GeneratedPost
{
    public Guid Id { get; set; }
    public Guid PlatformPostId { get; set; }
    public ContentSource Source { get; set; }
    public Guid? ParentGeneratedPostId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = null!;
    public string? Hashtag { get; set; }
    public string? FeedbackInput { get; set; }
    public Guid? SystemPromptId { get; set; }
    public string? PromptSnapshot { get; set; }
    public string? AiModel { get; set; }
    public string? AiRequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public int? DurationMs { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
