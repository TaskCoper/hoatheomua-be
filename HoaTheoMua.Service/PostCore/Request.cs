using HoaTheoMua.Repository.Enum;
namespace HoaTheoMua.Service.PostCore;

public class Request
{
    public class SavePostCoreRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Hashtag { get; set; }
        public List<ImageInput>? Images { get; set; }
    }
    public class CreatePostCoreRequest : SavePostCoreRequest { }
    public class UpdatePostCoreRequest : SavePostCoreRequest { }
    public class ImageInput
    {
        public Guid? Id { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
    public class GetPostCoreListRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public PlatformType? Platform { get; set; }
        public string? Keyword { get; set; }
    }
}
