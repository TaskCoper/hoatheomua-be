using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class AuditLog: BaseEntity<Guid>, IAuditableEntity
{
    public string Action { get; set; } 
    public string Description { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    
    public User User { get; set; }
    public Guid UserId { get; set; }
    
    public string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}