namespace HoaTheoMua.Service.PostGeneration;

public class Response
{
    public class OfficialVersion
    {
        public Guid BasePostId { get; set; }
        public Guid PlatformPostId { get; set; }
        public string Platform { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsReady { get; set; }
        public List<string> NotReadyReasons { get; set; } = new();
        public Guid? GenerationId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Hashtag { get; set; }
        public DateTimeOffset? SelectedAt { get; set; }
        public List<PostCore.Response.OfficialImage> Images { get; set; } = new();
    }
    public class ImageHistory
    {
        public Guid BasePostId { get; set; }
        public List<ImageGroup> Groups { get; set; } = new();
    }
    public class ImageGroup
    {
        public Guid SourceBasePostImageId { get; set; }
        public string OriginalImageUrl { get; set; } = null!;
        public bool SourceIsDeleted { get; set; }
        public string RatioCode { get; set; } = null!;
        public List<ImageVersion> Versions { get; set; } = new();
    }
    public class ImageVersion
    {
        public Guid GeneratedPostImageId { get; set; }
        public Guid SourceBasePostImageId { get; set; }
        public string RatioCode { get; set; } = null!;
        public Guid? ParentGeneratedPostImageId { get; set; }
        public int VersionNo { get; set; }
        public string SourceImageUrl { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public string Status { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public string? Feedback { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<string> SelectedPlatforms { get; set; } = new();
    }
    public class ContentHistory
    {
        public Guid PlatformPostId { get; set; }
        public string Platform { get; set; } = null!;
        public Guid? OfficialGenerationId { get; set; }
        public List<ContentRevision> Versions { get; set; } = new();
    }
    public class ContentRevision : PostCore.Response.ContentVersion
    {
        public string? Feedback { get; set; }
        public bool IsOfficial { get; set; }
    }
}
