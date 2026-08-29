using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Address : BaseEntity<Guid>, IAuditableEntity
{
    // External user id (from taskcoper-be's token); no FK — this service has no User table.
    public Guid UserId { get; set; }

    public required string ReceiverName { get; set; } // Tên người nhận
    public required string PhoneNumber { get; set; }  // Số điện thoại người nhận

    public required string AddressLine { get; set; }  // Số nhà, tên đường
    public string? Ward { get; set; }                 // Phường, Xã
    public required string City { get; set; }         // Tỉnh/ Thành phố
    public required string Country { get; set; }
    public string? Note  { get; set; }
    public string? MapId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Địa chỉ mặc định của user — mỗi user chỉ có tối đa một địa chỉ mặc định.
    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
