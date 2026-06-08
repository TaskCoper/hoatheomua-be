using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class User: BaseEntity<Guid>, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string HashPassword { get; set; }
    public required bool IsVerify = false;
    public String? PhoneNumber { get; set; }
    public Role Role { get; set; }
    //
    public ICollection<CategoryNews> CategoryNews { get; set; } = new List<CategoryNews>();
    public ICollection<AuditLog> AuditLog { get; set; } = new List<AuditLog>();
    public ICollection<ForgotPassword> ResetPassword { get; set; } = new List<ForgotPassword>();
    public ICollection<RefreshToken> RefreshToken { get; set; } = new List<RefreshToken>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}