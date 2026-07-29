using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class PostCoreBase : BaseEntity<Guid>, IAuditableEntity
{
    public required string Content { get; set; }
    public required string PictureUrl { get; set; }
    
    public ICollection<History> Histories { get; set; } = new List<History>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
