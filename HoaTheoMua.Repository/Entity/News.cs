using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class News: BaseEntity<Guid>, IAuditableEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int ViewCount { get; set; } 
    public string PictureUrl { get; set; }
    public NewsStatus Status { get; set; }
    
    public ICollection<CategoryNewsDetail> CategoryNewsDetail { get; set; } = new List<CategoryNewsDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}