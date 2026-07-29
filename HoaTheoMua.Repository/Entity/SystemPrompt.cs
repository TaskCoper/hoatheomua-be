using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class SystemPrompt : BaseEntity<Guid>, IAuditableEntity
{
    public required ConfigType Type { get; set; }
    public required string Content { get; set; }

    public ICollection<History> Histories { get; set; } = new List<History>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
