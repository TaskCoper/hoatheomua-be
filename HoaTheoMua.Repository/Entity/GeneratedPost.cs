using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class GeneratedPost : BaseEntity<Guid>, IAuditableEntity
{
    public required string Content { get; set; }
    public required string ImageUrl { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
