using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class PostHistory : BaseEntity<Guid>, IAuditableEntity
{
    public required string Input { get; set; }
    public Guid BaseId { get; set; }
    public Guid OutputId { get; set; }
    public required string Metadata { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid SystemPromptId { get; set; }
    public SystemPrompt SystemPrompt { get; set; } = null!;
}
