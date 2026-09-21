using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class FlowerReference : BaseEntity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionPrompt {get; set;}
    public string ImageUrl { get; set; } = null!;
    public bool IsActive { get; set; }
    public FlowerReferenceType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}