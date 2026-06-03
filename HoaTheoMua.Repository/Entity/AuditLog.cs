using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class AuditLog: BaseEntity<Guid>, IAuditableEntity
{
    public User User { get; set; }
    public Guid UserID { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string Entity { get; set; }
    public Guid? EntityID { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}