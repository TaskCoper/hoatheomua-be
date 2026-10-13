using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class Voucher : BaseEntity<Guid>, IAuditableEntity
{
    // null = standalone voucher; set = voucher generated into a program
    public Guid? ProgramId { get; set; }
    public VoucherProgram? Program { get; set; }

    public required string Code { get; set; }
    public string? Description { get; set; }
    public VoucherDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }      // only for Percent
    public VoucherVisibility Visibility { get; set; }
    public int MaxUsageCount { get; set; }
    // Display-only copy of the Paid-order count; may lag. Usage checks always count orders.
    public int UsedCount { get; set; }
    // Schedule and order range: null when ProgramId is set (read from the program)
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    public bool IsActive { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}