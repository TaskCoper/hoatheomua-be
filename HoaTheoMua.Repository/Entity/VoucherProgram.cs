using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class VoucherProgram : BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }

    public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
