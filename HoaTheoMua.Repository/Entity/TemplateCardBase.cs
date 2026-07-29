using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class TemplateCardBase : BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int LimitString { get; set; }
    public required string PictureUrl { get; set; } 
    public required string Metadata { get; set; }
    
    public ICollection<History> Histories { get; set; } = new List<History>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
