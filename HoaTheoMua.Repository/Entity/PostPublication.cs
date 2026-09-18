namespace HoaTheoMua.Repository.Entity;

// Publication history is retained independently of soft-deleted source records.
public class PostPublication
{
    public Guid Id { get; set; }
    public Guid BasePostId { get; set; }
    public Guid PlatformPostId { get; set; }
    public Guid PlatformAccountId { get; set; }
    public Guid RequestedBy { get; set; }
    public string IdempotencyKey { get; set; } = "";
    public string RequestHash { get; set; } = "";
    public string Snapshot { get; set; } = "{}";
    public string Status { get; set; } = "Queued";
    public long Revision { get; set; } = 1;
    public int AttemptCount { get; set; }
    public int StepCount { get; set; }
    public string Checkpoint { get; set; } = "{}";
    public bool PublishMayHaveOccurred { get; set; }
    public bool CanRetry { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTimeOffset? LeaseUntil { get; set; }
    public DateTimeOffset? NextRunAt { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? LastDispatchedAt { get; set; }
    public string? ExternalPostId { get; set; }
    public string? ExternalPostUrl { get; set; }
    public string? ErrorCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class PostPublicationAttempt
{
    public Guid Id { get; set; }
    public Guid PublicationId { get; set; }
    public int Number { get; set; }
    public string Operation { get; set; } = "";
    public string Status { get; set; } = "Started";
    public string? ErrorCode { get; set; }
    public string? ProviderCode { get; set; }
    public string? ProviderRequestId { get; set; }
    public string? ExternalPostId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class PostPublicationReview
{
    public Guid Id { get; set; }
    public Guid PublicationId { get; set; }
    public Guid ActorId { get; set; }
    public string Action { get; set; } = "";
    public string Note { get; set; } = "";
    public string? ExternalPostId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
