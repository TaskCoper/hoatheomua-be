namespace HoaTheoMua.Repository.Entity;

public class FlowerGenerationJob
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string IdempotencyKey { get; set; } = "";
    public string RequestHash { get; set; } = "";
    public string Type { get; set; } = "Create";
    public string Status { get; set; } = "Queued";
    public string Stage { get; set; } = "Queued";
    public string DisplayName { get; set; } = "";
    public Guid ProductId { get; set; }
    public string? ProductImageUrl { get; set; }
    public string? MockupImageUrl { get; set; }
    public Guid? SourceFlowerId { get; set; }
    public string Snapshot { get; set; } = "{}";
    public string Checkpoint { get; set; } = "{}";
    public string? Result { get; set; }
    public Guid? GeneratedFlowerId { get; set; }
    public string? ErrorCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelRequestedAt { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTimeOffset? LeaseUntil { get; set; }
    public DateTimeOffset? LastDispatchedAt { get; set; }
    public int AttemptCount { get; set; }
    public long Revision { get; set; } = 1;
}
