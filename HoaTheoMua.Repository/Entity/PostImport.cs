using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class PostImportBatch
{
    public Guid Id { get; set; }
    public Guid RequestedBy { get; set; }
    public string RequestKey { get; set; } = "";
    public string FileHash { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Status { get; set; } = "Queued";
    public string Issues { get; set; } = "[]";
    public Guid? LeaseId { get; set; }
    public DateTimeOffset? LeaseUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}

public class PostImportRow
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public int RowNumber { get; set; }
    public string Code { get; set; } = "";
    // Immutable normalized input, including the exact ordered URL list and selected channels.
    public string Payload { get; set; } = "{}";
    public string Status { get; set; } = "Queued";
    public string Issues { get; set; } = "[]";
    public Guid? BasePostId { get; set; }
    public int Attempts { get; set; }
}

public class PostImportChannel
{
    public Guid Id { get; set; }
    public Guid RowId { get; set; }
    public PlatformType Platform { get; set; }
    // Snapshot of the selected default at upload time. Defaults never retarget accepted work.
    public Guid? PlatformAccountId { get; set; }
    public Guid? PublicationId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorCode { get; set; }
}

public class ImportedPostIdentity
{
    public Guid RequestedBy { get; set; }
    public string Code { get; set; } = "";
    public Guid BasePostId { get; set; }
    public string ContentHash { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public class PostImportAccount
{
    public Guid RequestedBy { get; set; }
    public PlatformType Platform { get; set; }
    public Guid PlatformAccountId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
