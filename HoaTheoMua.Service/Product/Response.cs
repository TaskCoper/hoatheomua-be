using System;
using System.Collections.Generic;
using CategoryProductResponse = HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse;

namespace HoaTheoMua.Service.Product;

public class Response
{
    public class ProductDetailResponse
    {
        public Guid Id { get; set; }
        public bool? IsPinned { get; set; }
        public int SourceRowNumber { get; set; }
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
        public List<string>? OtherImages { get; set; }
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
        public int? NhanhCategoryId { get; set; }
        public int? NhanhBrandId { get; set; }
        public int? NhanhTypeId { get; set; }
        public string? Supplier { get; set; }
        public string? WarrantyAddress { get; set; }
        public string? Slug { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int? WebOrder { get; set; }
        public DateTimeOffset? NhanhCreatedAt { get; set; }
        public DateTimeOffset? NhanhUpdatedAt { get; set; }
        public string? ProductLabel { get; set; }
        public decimal? BranchPriceDuplicate { get; set; }
        public decimal? WholesaleBranchPriceDuplicate { get; set; }
        public string? PriceLabel { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Origin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public List<CategoryProductResponse> Categories { get; set; } = new List<CategoryProductResponse>();
    }

    public class PinnedProductResponse
    {
        public Guid ProductId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? CoverImage { get; set; }
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double? Position { get; set; }
    }
    
    public class LocalProductDetailResponse  
    {
        public Guid Id { get; set; }  
        public bool? IsPinned { get; set; }  
        public int SourceRowNumber { get; set; }  
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
        public List<string>? OtherImages { get; set; }  
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
        public int? NhanhCategoryId { get; set; }  
        public int? NhanhBrandId { get; set; }  
        public int? NhanhTypeId { get; set; }  
        public string? Supplier { get; set; }  
        public string? WarrantyAddress { get; set; }  
        public string? Slug { get; set; }  
        public decimal? Length { get; set; }  
        public decimal? Width { get; set; }  
        public decimal? Height { get; set; }  
        public int? WebOrder { get; set; }  
        public DateTimeOffset? NhanhCreatedAt { get; set; }  
        public DateTimeOffset? NhanhUpdatedAt { get; set; }  
        public string? ProductLabel { get; set; }  
        public decimal? BranchPriceDuplicate { get; set; }  
        public decimal? WholesaleBranchPriceDuplicate { get; set; }  
        public string? PriceLabel { get; set; }  
        public string? Size { get; set; }  
        public string? Color { get; set; }  
        public string? Origin { get; set; }  
        public DateTimeOffset CreatedAt { get; set; }  
        public DateTimeOffset? UpdatedAt { get; set; }  
      
        public List<CategoryProductResponse> Categories { get; set; } = new();
        public LocalProductDetailResponse? Parent { get; set; }
        public List<LocalProductDetailResponse> Children { get; set; } = new();  
    }
}
