using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class PlatformAccount : BaseEntity<Guid>, IAuditableEntity
{
    public PlatformType Platform { get; set; }
    public string ExternalAccountId { get; set; } = "";
    public string? PageId { get; set; }
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "Connected";
    public string[] GrantedScopes { get; set; } = [];
    public string? ProtectedCredentials { get; set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
    public DateTimeOffset? LastValidatedAt { get; set; }
    public string? LastErrorCode { get; set; }
    public Guid ConnectedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long Revision { get; set; }
    public Guid? RefreshLeaseId { get; set; }
    public DateTimeOffset? RefreshLeaseUntil { get; set; }
}

public class PlatformOAuthSession
{
    public Guid Id { get; set; }
    public PlatformType Platform { get; set; }
    public string StateHash { get; set; } = "";
    public string BrowserHash { get; set; } = "";
    public string? ProtectedVerifier { get; set; }
    public Guid RequestedBy { get; set; }
    public Guid? TargetAccountId { get; set; }
    public long? TargetRevision { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
