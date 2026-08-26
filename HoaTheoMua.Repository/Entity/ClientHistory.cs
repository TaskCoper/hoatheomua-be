using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class ClientHistory : BaseEntity<Guid>, IAuditableEntity
{
    public required string Input { get; set; }
    public Guid BaseId { get; set; }
    public Guid OutputId { get; set; }
    public Guid? OrderId { get; set; }
    public required GenerationType Type { get; set; }
    public required string Metadata { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid SystemPromptId { get; set; }
    public SystemPrompt SystemPrompt { get; set; } = null!;
}
