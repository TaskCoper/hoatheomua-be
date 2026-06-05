using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class AuditLog: BaseEntity<Guid>, IAuditableEntity
{
    public ActionAuditLog Action { get; set; } 
    public required string Description { get; set; }
    public required string OldValue { get; set; }
    public required string NewValue { get; set; }
    
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    
    public User? User { get; set; }
    public Guid UserId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}