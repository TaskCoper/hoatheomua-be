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
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string? search { get; set; }
        public decimal? priceFrom { get; set; }
        public decimal? priceTo { get; set; }
        public List<string>? sortBy { get; set; }
        public List<Guid>? categoryIds { get; set; }
        public bool? isPin { get; set; }

        /// <summary>
        /// Filter: chỉ hiển thị sản phẩm có SellableQuantity = 0
        /// </summary>
        public bool? soldOutOnly { get; set; }

        /// <summary>
        /// Filter: chỉ hiển thị sản phẩm có SellableQuantity > 0
        /// </summary>
        public bool? inStockOnly { get; set; }
    }
    
    public class MoveProductRequest
    {
        public double Position { get; set; }
        public Guid? CategoryId { get; set; }
    }

    public class PinProductRequest
    {
        public Guid ProductId { get; set; }
        public bool IsPin { get; set; }
        public Guid? CategoryId { get; set; }
    }

    public class GetProductPinRequest
    {
        public Guid? CategoryId { get; set; }
    }

    public class UpdateProductRequest
    {
        public int? SourceRowNumber { get; set; }
        public string? NhanhProductId { get; set; }
        public string? ParentProductId { get; set; }
        public string? ParentProductCode { get; set; }
        public string? ParentProductName { get; set; }
        public string? Barcode { get; set; }
        public string? ProductType { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? OtherName { get; set; }
        public string? Unit { get; set; }
        public string? CoverImage { get; set; }

        public int? WarrantyMonths { get; set; }
        public decimal? Weight { get; set; }

        public decimal? ImportPrice { get; set; }
        public decimal? ImportVat { get; set; }
        public decimal? ImportPriceWithVat { get; set; }
        public string? ImportVatMode { get; set; }

        public decimal? Price { get; set; }
        public decimal? PriceWithVat { get; set; }
        public decimal? BranchPrice { get; set; }
        public decimal? BranchPriceWithVat { get; set; }
        public string? SaleVatMode { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? Vat { get; set; }

        public decimal? ProfitPercentage { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public decimal? WholesalePriceWithVat { get; set; }
        public decimal? WholesaleBranchPrice { get; set; }
        public decimal? WholesaleBranchPriceWithVat { get; set; }

        public decimal? TotalStock { get; set; }
        public decimal? StockTransferring { get; set; }
        public decimal? Stock { get; set; }
        public decimal? DeliveringStock { get; set; }
        public decimal? WarehouseStock { get; set; }
        public decimal? StockError { get; set; }
        public decimal? Held { get; set; }
        public decimal? AvailableForSale { get; set; }
        public decimal? WarrantyStock { get; set; }
        public decimal? HeldComponents { get; set; }
        public decimal? PendingImportStock { get; set; }
        public decimal? PreOrder { get; set; }

        public string? Status { get; set; }
        public string? CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? InternalCategoryCode { get; set; }
        public string? Brand { get; set; }
        public string? Supplier { get; set; }
        public string? WarrantyAddress { get; set; }
        public string? Slug { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int? WebOrder { get; set; }
        public DateTimeOffset? NhanhCreatedAt { get; set; }
        public string? ProductLabel { get; set; }
        public decimal? BranchPriceDuplicate { get; set; }
        public decimal? WholesaleBranchPriceDuplicate { get; set; }
        public string? PriceLabel { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Origin { get; set; }
        public double? Position { get; set; }
        public bool? IsPin { get; set; }
    }
}

