using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Product;

public class ResponseProductDetails
{
    public class NhanhVnProductDetailResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public ProductDetailData? Data { get; set; }
    }

    public class ProductDetailData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parentId")]
        public int ParentId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("category")]
        public ProductCategory? Category { get; set; }

        [JsonPropertyName("categoryId")] 
        public int CategoryId { get; set; }

        [JsonPropertyName("internalCategory")]
        public ProductCategory? InternalCategory { get; set; }

        [JsonPropertyName("prices")]
        public ProductPrices? Prices { get; set; }

        [JsonPropertyName("vat")]
        public int Vat { get; set; }

        [JsonPropertyName("images")]
        public ProductImages? Images { get; set; }

        [JsonPropertyName("warranty")]
        public object? Warranty { get; set; }

        [JsonPropertyName("brand")]
        public ProductCommonInfo? Brand { get; set; }

        [JsonPropertyName("brandId")] // Bổ sung để hứng thêm brandId dạng số lẻ
        public int? BrandId { get; set; }

        [JsonPropertyName("type")]
        public ProductCommonInfo? Type { get; set; }

        [JsonPropertyName("shipping")]
        public ProductShipping? Shipping { get; set; }

        [JsonPropertyName("countryName")]
        public string? CountryName { get; set; }

        [JsonPropertyName("units")]
        public ProductUnits? Units { get; set; }

        [JsonPropertyName("combos")]
        public List<ProductCombo> Combos { get; set; } = new List<ProductCombo>();

        [JsonPropertyName("inventory")]
        public ProductInventory? Inventory { get; set; }

        [JsonPropertyName("attributes")]
        public List<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("suppliers")]
        public List<ProductSupplier> Suppliers { get; set; } = new List<ProductSupplier>();

        [JsonPropertyName("videos")]
        public List<ProductVideo> Videos { get; set; } = new List<ProductVideo>();

        [JsonPropertyName("branchPrices")]
        public List<ProductBranchPrice> BranchPrices { get; set; } = new List<ProductBranchPrice>();

        [JsonPropertyName("childs")]
        public List<ProductDetailData> Childs { get; set; } = new List<ProductDetailData>();
    }

    public class ProductCategory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class ProductPrices
    {
        [JsonPropertyName("retail")]
        public decimal Retail { get; set; }

        [JsonPropertyName("retailVat")] 
        public decimal RetailVat { get; set; }

        [JsonPropertyName("import")]
        public decimal Import { get; set; }

        [JsonPropertyName("importVat")] 
        public decimal ImportVat { get; set; }

        [JsonPropertyName("old")]
        public decimal Old { get; set; }

        [JsonPropertyName("wholesale")]
        public decimal Wholesale { get; set; }

        [JsonPropertyName("avgCost")]
        public decimal AvgCost { get; set; }
    }

    public class ProductImages
    {
        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("others")]
        public List<string> Others { get; set; } = new List<string>();
    }

    public class ProductCommonInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class ProductShipping
    {
        [JsonPropertyName("length")]
        public double Length { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    public class ProductUnits
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        // ĐÃ FIX: Thêm Custom Converter để tự động gánh cả Array [] lẫn Object {} từ API
        [JsonPropertyName("list")]
        [JsonConverter(typeof(NhanhVnDataListConverter.NhanhVnUnitsListConverter))]
        public Dictionary<string, List<UnitDetail>>? List { get; set; }
    }

    public class UnitDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("price")]
        public UnitPrice? Price { get; set; }
    }

    public class UnitPrice
    {
        [JsonPropertyName("retail")]
        public decimal Retail { get; set; }

        [JsonPropertyName("import")]
        public decimal Import { get; set; }

        [JsonPropertyName("wholesale")]
        public decimal Wholesale { get; set; }
    }

    public class ProductCombo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; } 
    }

    public class ProductInventory
    {
        [JsonPropertyName("remain")]
        public double Remain { get; set; }

        [JsonPropertyName("shipping")]
        public double Shipping { get; set; }

        [JsonPropertyName("damaged")]
        public double Damaged { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }

        [JsonPropertyName("transfering")]
        public double Transfering { get; set; }

        // ĐÃ FIX: Chuyển sang JsonElement? để không bị sập khi API đổi từ Số (0) sang Object {"remain":0...}
        [JsonPropertyName("warranty")]
        public JsonElement? Warranty { get; set; }

        [JsonPropertyName("warrantyHolding")]
        public double WarrantyHolding { get; set; }

        [JsonPropertyName("holdingNew")]
        public double HoldingNew { get; set; }

        [JsonPropertyName("shippingExport")]
        public double ShippingExport { get; set; }

        [JsonPropertyName("available")]
        public double Available { get; set; }

        [JsonPropertyName("depots")]
        public List<DepotInventoryDetail> Depots { get; set; } = new List<DepotInventoryDetail>();
    }

    public class DepotInventoryDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("remain")]
        public double Remain { get; set; }

        [JsonPropertyName("shipping")]
        public double Shipping { get; set; }

        [JsonPropertyName("damaged")]
        public double Damaged { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }

        [JsonPropertyName("transfering")]
        public double Transfering { get; set; }

        // ĐÃ FIX: Đồng bộ JsonElement? tại đây
        [JsonPropertyName("warranty")]
        public JsonElement? Warranty { get; set; }

        [JsonPropertyName("warrantyHolding")]
        public double WarrantyHolding { get; set; }

        [JsonPropertyName("holdingNew")]
        public double HoldingNew { get; set; }

        [JsonPropertyName("shippingExport")]
        public double ShippingExport { get; set; }

        [JsonPropertyName("available")]
        public double Available { get; set; }
    }

    public class ProductAttribute
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class ProductSupplier
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }
    }

    public class ProductVideo
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; } 

        [JsonPropertyName("src")]
        public string? Src { get; set; } 
    }

    public class ProductBranchPrice
    {
        [JsonPropertyName("branch")]
        public ProductCommonInfo? Branch { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("wholesalePrice")]
        public decimal WholesalePrice { get; set; }
    }
}