namespace HoaTheoMua.Service.CompanyAddress;

public class Response
{
    public class CompanyAddressResponse
    {
        public Guid Id { get; set; }

        public required string AddressLine { get; set; }

        public string? Ward { get; set; }

        public string? District { get; set; }

        public required string City { get; set; }

        public required string Country { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class GetCompanyAddressesResponse
    {
        public List<CompanyAddressResponse> Items { get; set; } = new();

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public int? PreviousPage { get; set; }

        public int? NextPage { get; set; }
    }
}