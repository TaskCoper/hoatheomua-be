using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class News: BaseEntity<Guid>, IAuditableEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int ViewCount { get; set; } = 0;
    public string PictureUrl { get; set; }
    public string Status { get; set; }
    
    public ICollection<CategoryNewsDetail> CategoryNewsDetail { get; set; } = new List<CategoryNewsDetail>();

    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}