using HoaTheoMua.Repository.Abtraction;
using Quartz.Impl.Calendar;

namespace HoaTheoMua.Repository.Entity;

public class ComboSpecification: BaseEntity<Guid>, IAuditableEntity
{
    public Guid SubId { get; set; }
    public Product Sub { get; set; } = null!;

    public Guid MatId { get; set; }
    public Product Mat { get; set; } = null!;

    public decimal? Quantity { get; set; }
    public bool IsCore { get; set; }
    public bool IsActive { get; set; }
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}