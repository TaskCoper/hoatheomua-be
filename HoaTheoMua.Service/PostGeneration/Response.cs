namespace HoaTheoMua.Service.PostGeneration;

public class Response
{
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
