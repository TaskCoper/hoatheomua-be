using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Product;

public class Request
{
    public class ProductListRequest
    {
        [JsonPropertyName("filters")]
        public ProductFilters? Filters { get; set; } = null;

        [JsonPropertyName("paginator")]
        public ProductPaginator? Paginator { get; set; } = null;
    }

    public class ProductFilters
    {
        [JsonPropertyName("ids")] public List<int>? Ids { get; set; } = null;

        [JsonPropertyName("status")]
        public List<int>? Status { get; set; } = null;
    }

    public class ProductPaginator
    {
        [JsonPropertyName("size")]
        public int? Size { get; set; } = null;

        [JsonPropertyName("sort")]
        public ProductSort? Sort { get; set; } = null;

        [JsonPropertyName("next")]
        public ProductNextPage? Next { get; set; } = null;
    }
    
    public class ProductSort
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; } = null;
    }
    
    public class ProductNextPage
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; } = null;
    }
    
    public class ProductDetailRequest
    {
        [JsonPropertyName("filters")]
        public ProductDetailFilters? Filters { get; set; } = null;
    }

    public class ProductDetailFilters
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; } = null;
    }
}