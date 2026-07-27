using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CategoryProductDetail : BaseEntity<Guid>, IAuditableEntity
{
    public CategoryProduct CategoryProduct { get; set; } = null!;
    public Guid CategoryProductId { get; set; }

    public Product Product { get; set; } = null!;
    public Guid ProductId { get; set; }
    
    public double? Position { get; set; }
    public bool IsPin { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
