using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class FileMetadata : BaseEntity<Guid>, IAuditableEntity
{
    public required string FileName { get; set; }
    public required string StorageKey { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string BucketName { get; set; }
    public Guid UploadedByUserId { get; set; }
    public FileUploadStatus UploadStatus { get; set; } = FileUploadStatus.Pending;
    public DateTimeOffset? CompletedAt { get; set; }
    public User UploadedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
