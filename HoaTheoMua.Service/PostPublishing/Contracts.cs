using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.PlatformAccount;

namespace HoaTheoMua.Service.PostPublishing;

public sealed class PublicationException(int status, string code, string message) : AppException("Post publication", status, code, message);
public record PublicationImage(Guid GenerationId, string Url, int Order);
public record PublicationSnapshot(Guid BasePostId, Guid PlatformPostId, Guid AccountId, string ExternalAccountId,
    string? PageId, PlatformType Platform, string ApiVersion, string FormatterVersion, Guid GenerationId,
    string? Title, string Content, string? Hashtag, string Text, PublicationImage[] Images);

// Only provider identifiers belong here. Never store raw provider responses or credentials.
public record MediaCheckpoint(int Order, string ProviderId, int? Width = null, int? Height = null, bool Ready = false);
public record PublicationCheckpoint
{
    public MediaCheckpoint[] Media { get; init; } = [];
    public string? ContainerId { get; init; }
    public bool ReadyToPublish { get; init; }
    public DateTimeOffset? PreparedAt { get; init; }
}
public enum PublicationOperation { Prepare, Publish, Reconcile }
public enum PublicationOutcome { Continue, Pending, Succeeded, Rejected, Unknown }
public record PublicationResult(PublicationOutcome Outcome)
{
    public PublicationCheckpoint? Checkpoint { get; init; }
    public string? ExternalPostId { get; init; }
    public string? ExternalPostUrl { get; init; }
    public string? ErrorCode { get; init; }
    public string? ProviderCode { get; init; }
    public string? ProviderRequestId { get; init; }
    public bool Retryable { get; init; }
    public bool RequiresReauth { get; init; }
    public bool PublishMayHaveOccurred { get; init; }
    // True only with positive provider evidence, never based on absence from a feed.
    public bool ConfirmedNotPublished { get; init; }
    public int RetryAfterSeconds { get; init; } = 30;
}

// Persistence/dispatch is independent of the provider HTTP implementations.
public interface IPublishingAdapter
{
    PlatformType Platform { get; }
    IReadOnlyList<string> Validate(PublicationSnapshot snapshot);
    Task<IReadOnlyList<string>> Preflight(PublicationSnapshot snapshot, Credentials credentials, CancellationToken ct);
    Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot snapshot,
        PublicationCheckpoint checkpoint, Credentials credentials, string? candidateExternalId, CancellationToken ct);
}
public record PublicationRequested(Guid PublicationId, long Revision);

public class PublishingOptions
{
    public bool FacebookEnabled { get; set; }
    public bool InstagramEnabled { get; set; }
    public int OperationTimeoutSeconds { get; set; } = 30;
    public int LeaseSeconds { get; set; } = 120;
    public int MaxSteps { get; set; } = 80;
    public int MaxAttempts { get; set; } = 200;
    public int RecoveryIntervalSeconds { get; set; } = 15;
    public int MinScheduleLeadSeconds { get; set; } = 60;
    public int MaxScheduleDays { get; set; } = 30;
    public int ScheduleLatenessSeconds { get; set; } = 900;
    public bool Enabled(PlatformType p) => p == PlatformType.Facebook ? FacebookEnabled : p == PlatformType.Instagram && InstagramEnabled;
    public void Validate()
    {
        if (OperationTimeoutSeconds is < 1 or > 60 || LeaseSeconds < OperationTimeoutSeconds + 30 || LeaseSeconds > 600
            || MaxSteps is < 1 or > 200 || MaxAttempts is < 1 or > 1000 || RecoveryIntervalSeconds is < 1 or > 60
            || MinScheduleLeadSeconds is < 1 or > 86400 || MaxScheduleDays is < 1 or > 365 || ScheduleLatenessSeconds is < 0 or > 86400)
            throw new PublicationException(503, "PUBLISHING_CONFIG_INVALID", "Cấu hình tác vụ đăng không hợp lệ.");
    }
}
