using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class RefreshToken : BaseEntity<Guid>, IAuditableEntity
{
    public User User { get; set; }
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string DeviceLabel { get; set; }
    public DateTimeOffset ExpiredAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}