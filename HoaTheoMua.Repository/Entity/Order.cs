using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Order: BaseEntity<Guid>,IAuditableEntity
{
    
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public required int Ward { get; set; }
    public required int City { get; set; }
    public required string Address { get; set; }
    public required string ShortAddress { get; set; }
    public required string FullName {get; set; }
    public required string Phone { get; set; }
    public required string Remarks { get; set; }
    public required string Status { get; set; }
    public required string TrackNumber { get; set; }
    //public string Metadata { get; set; }
    public int? NhanhOrderId { get; set; }
    public  string? NhanhTrackingUrl { get; set; }
    public  string? AhahaMoveOrderId { get; set; }
    public  string? AhahaMoveTrackingUrl { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal ProductFree { get; set; }
    public decimal TotalFee { get; set; }
    
    // External user id (from taskcoper-be's token); no FK — this service has no User table.
    public Guid UserId { get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}