using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;
using Microsoft.AspNetCore.Http;

namespace HoaTheoMua.Repository.Entity;

public class News: BaseEntity<Guid>, IAuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int ViewCount { get; set; }
    public string CoverImage { get; set; } = null!;
    public NewsStatus Status { get; set; }
    
    public ICollection<CategoryNewsDetail> CategoryNewsDetail { get; set; } = new List<CategoryNewsDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}