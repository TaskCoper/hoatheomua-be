namespace HoaTheoMua.Service.PostCore;

public class Response
{
    public class PostCoreSummary
    {
        public Guid BasePostId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Hashtag { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
    public class PostCoreDetail : PostCoreSummary
    {
        public List<BaseImage> BaseImages { get; set; } = new();
        public List<GeneratedImage> Images { get; set; } = new();
        public List<PlatformDetail> Platforms { get; set; } = new();
    }
    public class BaseImage
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int SortOrder { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
    public class GeneratedImage
    {
        public Guid Id { get; set; }
        public Guid SourceBasePostImageId { get; set; }
        public Guid? ParentGeneratedPostImageId { get; set; }
        public string RatioCode { get; set; } = null!;
        public int VersionNo { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    public class PlatformDetail
    {
        public Guid PlatformPostId { get; set; }
        public string Platform { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? LastError { get; set; }
        public ContentVersion? Content { get; set; }
        public List<ContentVersion> Versions { get; set; } = new();
        public List<ImageSlot> Images { get; set; } = new();
    }
    public class ContentVersion
    {
        public Guid GenerationId { get; set; }
        public Guid? ParentGeneratedPostId { get; set; }
        public int VersionNo { get; set; }
        public string Source { get; set; } = null!;
        public string? Title { get; set; }
        public string Content { get; set; } = null!;
        public string? Hashtag { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    public class ImageSlot
    {
        public Guid SourceBasePostImageId { get; set; }
        public string RatioCode { get; set; } = null!;
        public Guid? SelectedGeneratedPostImageId { get; set; }
        public int SortOrder { get; set; }
    }
    public record DeletedPost(Guid BasePostId, bool IsDeleted);
}
