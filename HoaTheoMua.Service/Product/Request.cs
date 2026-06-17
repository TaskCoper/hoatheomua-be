using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Product;

public class Request
{
    public class ProductListRequest
    {
        [JsonPropertyName("filters")]
        public ProductFilters Filters { get; set; }

        [JsonPropertyName("paginator")]
        public ProductPaginator Paginator { get; set; }
    }

    public class ProductFilters
    {
        [JsonPropertyName("ids")]
        public List<int> Ids { get; set; }

        [JsonPropertyName("status")]
        public List<int> Status { get; set; }
    }

    public class ProductPaginator
    {
        [JsonPropertyName("size")]
        public int Size { get; set; } = 50;

        [JsonPropertyName("sort")]
        public Dictionary<string, string> Sort { get; set; } = new Dictionary<string, string> { { "id", "desc" } };

        [JsonPropertyName("next")]
        public ProductNextPage Next { get; set; }
    }

    public class ProductNextPage
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}