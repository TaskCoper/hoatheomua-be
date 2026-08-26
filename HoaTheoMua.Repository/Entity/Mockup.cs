using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Mockup : BaseEntity<Guid>, IAuditableEntity
{
    public required string ImageUrl { get; set; }
    public required string Description { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
