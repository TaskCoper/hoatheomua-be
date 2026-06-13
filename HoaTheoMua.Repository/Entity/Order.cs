using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Order: BaseEntity<Guid>,IAuditableEntity
{
    
    public string TrackNumber { get; set; }
    public string Status { get; set; }
    public string Metadata { get; set; }
    
    public Guid UserId { get; set; }
    public User User  { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}