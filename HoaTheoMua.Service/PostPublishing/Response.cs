namespace HoaTheoMua.Service.PostPublishing;

public class Response
{
    public record Preview(string Revision, PublicationSnapshot Snapshot, bool CanPublish, IReadOnlyList<string> Reasons,
        string MediaVerification = "Unverified");
    public record Job(Guid PublicationId, string Status, long Revision, Guid RequestedBy, DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt, DateTimeOffset? CompletedAt, int AttemptCount, bool CanRetry,
        string? ExternalPostId, string? ExternalPostUrl, string? ErrorCode, bool PublishMayHaveOccurred, DateTimeOffset? ScheduledAt = null);
    public record Attempt(int Number, string Operation, string Status, DateTimeOffset StartedAt,
        DateTimeOffset? CompletedAt, string? ErrorCode, string? ProviderCode, string? ProviderRequestId, string? ExternalPostId);
    public record Review(Guid ActorId, string Action, string Note, string? ExternalPostId, DateTimeOffset CreatedAt);
    public record Detail(Job Job, PublicationSnapshot Snapshot, IReadOnlyList<Attempt> Attempts, IReadOnlyList<Review> Reviews);
}
