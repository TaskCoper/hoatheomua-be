using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class GeneratedPostImage
{
    public Guid Id { get; set; }
    public Guid BasePostId { get; set; }
    public Guid SourceBasePostImageId { get; set; }
    public AspectRatioType RatioCode { get; set; }
    public Guid? ParentGeneratedPostImageId { get; set; }
    public string SourceImageUrl { get; set; } = null!;
    public int PixelWidth { get; set; }
    public int PixelHeight { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset? ImageExpiresAt { get; set; }
    public string? StorageProvider { get; set; }
    public string? FeedbackInput { get; set; }
    public ImageStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AiModel { get; set; }
    public string? AiRequestId { get; set; }
    public int? DurationMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
