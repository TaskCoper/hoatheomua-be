namespace HoaTheoMua.Service.Address;

public class Request
{
    public class CreateAddressRequest
    {
        public required string ReceiverName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string AddressLine { get; set; }
        public string? Ward { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public string? MapId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Đặt địa chỉ này làm mặc định ngay khi tạo.
        public bool IsDefault { get; set; }
    }

    public class UpdateAddressRequest
    {
        public required string ReceiverName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string AddressLine { get; set; }
        public string? Ward { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public string? MapId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // true => đặt làm mặc định. Bỏ mặc định trực tiếp không được phép —
        // hãy set một địa chỉ khác làm mặc định thay thế.
        public bool? IsDefault { get; set; }
    }
}
