using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class BasePostImage : BaseEntity<Guid>
{
    public Guid BasePostId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public int SortOrder { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
