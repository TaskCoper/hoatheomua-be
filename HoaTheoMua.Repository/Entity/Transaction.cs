using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Transaction: BaseEntity<Guid>, IAuditableEntity
{
    
    public decimal SePayId { get; set; } 
    public string Gateway { get; set; } 
    public DateTime TransactionDate { get; set; } 
    public string AccountNumber { get; set; }
    public string SubAccount { get; set; }
    public string Code { get; set; }
    public string Content { get; set; } 
    public string TransferType { get; set; } 
    public string Description { get; set; }
    public decimal TransferAmount { get; set; }
    public decimal Accumulated { get; set; } 
    public string ReferenceCode { get; set; }
    public string Status { get; set; } = "Success";
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}