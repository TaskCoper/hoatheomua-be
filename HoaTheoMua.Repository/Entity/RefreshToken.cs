using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class RefreshToken : BaseEntity<Guid>, IAuditableEntity
{
    public required string RefreshTokenHash { get; set; }
    public required string IpAddress { get; set; }
    public required string UserAgent { get; set; }
    public required string DeviceLabel { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}