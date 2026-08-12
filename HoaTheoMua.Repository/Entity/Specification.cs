using HoaTheoMua.Repository.Abtraction;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoaTheoMua.Repository.Entity;

[Table("Specification")]
public class Specification : BaseEntity<Guid>
{
    // NhanhProductId của Combo
    public string? SubId { get; set; }
    
    // NhanhProductId của Vật liệu
    public string? MatId { get; set; }
    
    // Số lượng vật liệu trong 1 combo
    public int Quantity { get; set; }
    
    // Role = 1 (Core/Hoa chính), 2 (Support/Phụ kiện)
    public int Role { get; set; }
    
    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation Property trỏ tới Product (vật liệu) 
    // EF Core sẽ cần Config cho foreign key này (MatId -> Product.NhanhProductId)
    public virtual Product? MaterialProduct { get; set; }
}
