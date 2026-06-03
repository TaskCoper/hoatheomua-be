using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CategoryNewsDetail:BaseEntity<Guid>, IAuditableEntity
{
    public CategoryNews CategoryNews { get; set; } 
    public Guid CategoryNewsID { get; set; }
    public News news { get; set; }
    public Guid NewsID { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

