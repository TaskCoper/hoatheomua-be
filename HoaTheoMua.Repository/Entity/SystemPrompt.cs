using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class SystemPrompt : BaseEntity<Guid>, IAuditableEntity
{
    public SystemPromptType Type { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
