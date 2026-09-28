using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class BasePost : BaseEntity<Guid>, IAuditableEntity
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Hashtag { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
