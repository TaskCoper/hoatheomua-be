using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CardOut : BaseEntity<Guid>, IAuditableEntity
{
    public required string Content { get; set; }
    public  required string ImageUrl { get; set; }
    
    public Guid UserId { get; set; }
    public History? History { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
