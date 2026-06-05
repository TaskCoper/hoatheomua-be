using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CompanyAddress: BaseEntity<Guid>, IAuditableEntity
{
    public required string AddressLine { get; set; }
    public required string Ward { get; set; }//phuong
    public required string District { get; set; }//quan
    public required string City { get; set; }
    public required string Country { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
