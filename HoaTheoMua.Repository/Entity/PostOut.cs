using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class PostOut : BaseEntity<Guid>, IAuditableEntity
{
    public required string Content { get; set; }
    public required string ImageUrl { get; set; }
    
    public Guid UserId { get; set; }
    public History?  History { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
