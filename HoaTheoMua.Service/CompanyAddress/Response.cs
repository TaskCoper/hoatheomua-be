namespace HoaTheoMua.Service.CompanyAddress;

public class Response
{
    public class CompanyAddressResponse
    {
        public Guid Id { get; set; }

        public required string AddressLine { get; set; }

        public string? Ward { get; set; }
        
        public required string Phone { get; set; }
        public required string Remarks { get; set; }
        public required string FullName { get; set; }

        public required string City { get; set; }

        public required string Country { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? MapId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}