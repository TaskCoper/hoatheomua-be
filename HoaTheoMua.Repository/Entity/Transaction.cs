using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class Transaction : BaseEntity<Guid>, IAuditableEntity
{
    public required string RequestId { get; set; }
    public required string Gateway { get; set; }
    public DateTime TransactionDate { get; set; }
    public required string AccountNumber { get; set; }
    public required string SubAccount { get; set; }
    public required string Code { get; set; }
    public required string Content { get; set; }
    public required string TransferType { get; set; }
    public required string Description { get; set; }
    public decimal TransferAmount { get; set; }
    public decimal Accumulated { get; set; }
    public required string ReferenceCode { get; set; }

    public PaymentMethod paymentMethod { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}