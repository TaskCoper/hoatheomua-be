using System;
using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Product : BaseEntity<Guid>, IAuditableEntity
{
    public required string NhanhProductId { get; set; }
    public string? ParentProductId { get; set; }
    public string? Barcode { get; set; }
    public string? ProductType { get; set; }
    public string? ProductCode { get; set; }
    public required string ProductName { get; set; }
    public string? Unit { get; set; }
    public string? CoverImage { get; set; }

    public int? WarrantyMonths { get; set; }
    public decimal? Weight { get; set; }

    public decimal? ImportPrice { get; set; }
    public decimal? ImportPriceWithVat { get; set; }
    public string? ImportVatMode { get; set; }

    public decimal? Price { get; set; }
    public decimal? PriceWithVat { get; set; }
    public string? SaleVatMode { get; set; }

    public decimal? ProfitPercentage { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? WholesalePriceWithVat { get; set; }

    public decimal? TotalStock { get; set; }
    public decimal? Stock { get; set; }
    public decimal? WarehouseStock { get; set; }
    public decimal? Held { get; set; }
    public decimal? AvailableForSale { get; set; }
    public decimal? PreOrder { get; set; }
    public decimal? DeliveringStock { get; set; }
    public int? PendingImportStock { get; set; }

    public string? Status { get; set; }
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public string? Supplier { get; set; }
    public string? Slug { get; set; }

    public DateTimeOffset? NhanhCreatedAt { get; set; }

    public string? PriceLabel { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
