using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class SystemPrompt : BaseEntity<Guid>, IAuditableEntity
{
    public required SystemPromptType Type { get; set; }
    public required string Content { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<PostHistory> PostHistories { get; set; } = new List<PostHistory>();
    public ICollection<ClientHistory> ClientHistories { get; set; } = new List<ClientHistory>();
}
