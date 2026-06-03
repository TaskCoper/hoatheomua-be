using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class ResetPassword: BaseEntity<Guid>, IAuditableEntity
{
    public User User { get; set; }
    public Guid UserID { get; set; }
    public string TokenHash { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}