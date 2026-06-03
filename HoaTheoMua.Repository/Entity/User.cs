using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class User: BaseEntity<Guid>, IAuditableEntity
{
    public String Name { get; set; }
    public String Email { get; set; }
    public String HashPassword { get; set; }
    public String PhoneNumber { get; set; }
    public Role Role { get; set; }
    
    public ICollection<CategoryNews> CategoryNews { get; set; } = new List<CategoryNews>();
    public ICollection<AuditLog> AuditLog { get; set; } = new List<AuditLog>();
    public ICollection<ResetPassword> ResetPassword { get; set; } = new List<ResetPassword>();
    public ICollection<RefreshToken> RefreshToken { get; set; } = new List<RefreshToken>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}