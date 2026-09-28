using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class PlatformPostImage
{
    public Guid Id { get; set; }
    public Guid PlatformPostId { get; set; }
    public Guid SourceBasePostImageId { get; set; }
    public AspectRatioType RatioCode { get; set; }
    public Guid? SelectedGeneratedPostImageId { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
