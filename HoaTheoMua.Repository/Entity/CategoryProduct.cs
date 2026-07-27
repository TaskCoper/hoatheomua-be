using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class CategoryProduct : BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Name_En { get; set; }
    public string? Description { get; set; }
    public string? Description_En { get; set; }
    public double? Position { get; set; }
    public string? MetaData { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public CategoryProductType Type { get; set; }
    public bool IsActive { get; set; }
    public Guid UserId { get; set; }

    public CategoryProduct? ParentCategoryProduct { get; set; }
    public Guid? ParentCategoryProductId { get; set; }

    public ICollection<CategoryProduct>? ChildCategoryProducts { get; set; } = new List<CategoryProduct>();
    public ICollection<CategoryProductDetail> CategoryProductDetails { get; set; } = new List<CategoryProductDetail>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
