namespace HoaTheoMua.Service.Address;

public class Response
{
    public class AddressResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string ReceiverName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string AddressLine { get; set; }
        public string? Ward { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public string? MapId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
