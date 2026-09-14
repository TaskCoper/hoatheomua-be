using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.PostPublishing;

public class Request
{
    public class Publish
    {
        public Guid AccountId { get; set; }
        [Required, StringLength(128, MinimumLength = 1)] public string IdempotencyKey { get; set; } = "";
        [Required, StringLength(64, MinimumLength = 64)] public string ExpectedRevision { get; set; } = "";
    }
    public class Revision
    {
        [Range(1, long.MaxValue)] public long ExpectedRevision { get; set; }
    }
    public class Schedule : Publish
    {
        [Required, StringLength(40)] public string ScheduledAt { get; set; } = "";
    }
    public class CancelSchedule : Revision
    {
        public Guid PublicationId { get; set; }
    }
    public class Reschedule : CancelSchedule
    {
        [Required, StringLength(40)] public string ScheduledAt { get; set; } = "";
    }
    public class Resolve : Revision
    {
        [Required] public string Action { get; set; } = ""; // ConfirmPublished, ConfirmNotPublished, Close
        [Required, StringLength(2000, MinimumLength = 1)] public string Note { get; set; } = "";
        [StringLength(200)] public string? ExternalPostId { get; set; }
    }
}
