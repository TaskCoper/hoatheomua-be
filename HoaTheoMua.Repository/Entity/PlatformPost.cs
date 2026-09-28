using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class PlatformPost : BaseEntity<Guid>, IAuditableEntity
{
    public Guid BasePostId { get; set; }
    public PlatformType Platform { get; set; }
    public PlatformPostStatus Status { get; set; }
    public Guid? OfficialGenerationId { get; set; }
    public DateTimeOffset? SelectedAt { get; set; }
    public string? LastError { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? PlatformAccountId { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? ExternalPostId { get; set; }
    public string? PublishError { get; set; }
    public int PublishAttemptCount { get; set; }
}
