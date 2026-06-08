using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CompanyAddress: BaseEntity<Guid>, IAuditableEntity
{
    public required string AddressLine { get; set; }// Số nhà, tên đường
    public string? Ward { get; set; }// Phường, Xã
    public string? District { get; set; }// Quận. Huyện
    public required string City { get; set; } // Tỉnh/ Thành Phố
    public required string Country { get; set; }
    
    public decimal? Latitude { get; set; }           
    public decimal? Longitude { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
