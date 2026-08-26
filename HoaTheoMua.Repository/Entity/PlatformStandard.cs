using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class PlatformStandard : BaseEntity<Guid>, IAuditableEntity
{
    public required string Description { get; set; }
    public required PlatformType Platform { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
