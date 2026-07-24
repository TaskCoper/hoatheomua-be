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
        [JsonPropertyName("ids")] 
        public List<int>? Ids { get; set; } = null;

        [JsonPropertyName("status")]
        public List<int>? Status { get; set; } = null;
        
        // Thêm trường name (Tên, mã, mã vạch sản phẩm)
        [JsonPropertyName("name")]
        public string? Name { get; set; } = null;

        [JsonPropertyName("gpid")]
        public int? Gpid { get; set; }
        
        [JsonPropertyName("parentId")]
        public int? ParentId { get; set; }
        
        // Thêm trường categoryIds (Mảng ID danh mục sản phẩm)
        [JsonPropertyName("categoryIds")]
        public List<int>? CategoryIds { get; set; } = null;
        
        [JsonPropertyName("price")]
        public List<decimal>? Price { get; set; } = null;
    }
    
    // public class PriceRange
    // {
    //     [JsonPropertyName("from")]
    //     public decimal? From { get; set; } = null;
    //
    //     [JsonPropertyName("to")]
    //     public decimal? To { get; set; } = null;
    // }

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
        public ProductDetailFilters Filters { get; set; } = null!;
    }

    public class ProductDetailFilters
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }

    public class LocalProductQueryRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public List<string>? SortBy { get; set; }
    }
    
    public class MoveProductRequest
    {
        public Guid? PreviousProductId { get; set; }

        public Guid? NextProductId { get; set; }
    }
}