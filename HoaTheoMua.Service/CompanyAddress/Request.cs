namespace HoaTheoMua.Service.CompanyAddress;

public class Request
{
    public class CreateCompanyAddressRequest
    {
        public required string AddressLine { get; set; }
        public string? Ward { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public string? Remarks { get; set; } = null!;
        public required string MapId { get; set; }
    }

    public class UpdateCompanyAddressRequest
    {
        public required string AddressLine { get; set; }
        public required string Ward { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public string? Remarks { get; set; }
        public required string MapId { get; set; }
    }
}
