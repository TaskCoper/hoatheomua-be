using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Product;

public class ResponseProductList
{
    public class NhanhVnProductResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("paginator")]
        public ProductResponsePaginator Paginator { get; set; }

        [JsonPropertyName("data")]
        public List<ProductDetailData> Data { get; set; } = new List<ProductDetailData>();
    }

    // 1. ĐÃ SỬA: Thay đổi cấu trúc Paginator từ string thành Object khớp 100% với JSON thực tế
    public class ProductResponsePaginator
    {
        [JsonPropertyName("next")]
        public PaginatorNext Next { get; set; }
    }

    public class PaginatorNext
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    public class ProductDetailData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parentId")]
        public int ParentId { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("barcode")]
        public string Barcode { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("priceVatMode")]
        public int PriceVatMode { get; set; }

        [JsonPropertyName("importPriceVatMode")]
        public int ImportPriceVatMode { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("internalCategoryId")]
        public int InternalCategoryId { get; set; }

        [JsonPropertyName("prices")]
        public ProductPrices Prices { get; set; }

        [JsonPropertyName("images")]
        public ProductImages Images { get; set; }

        // 2. ĐÃ SỬA: Chuyển sang kiểu dữ liệu 'object?' linh hoạt để nuốt trọn cả mảng [] lẫn Object từ Nhanh.vn
        [JsonPropertyName("warranty")]
        public object? Warranty { get; set; } 

        [JsonPropertyName("brandId")]
        public int BrandId { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("shipping")]
        public ProductShipping Shipping { get; set; }

        [JsonPropertyName("countryName")]
        public string CountryName { get; set; }

        [JsonPropertyName("units")]
        public ProductUnits Units { get; set; }

        [JsonPropertyName("combos")]
        public List<ProductCombo> Combos { get; set; } = new List<ProductCombo>();

        [JsonPropertyName("inventory")]
        public ProductInventory Inventory { get; set; }

        [JsonPropertyName("attributes")]
        public List<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }
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
        public string Avatar { get; set; }

        [JsonPropertyName("others")]
        public List<string> Others { get; set; } = new List<string>();
    }

    public class ProductShipping
    {
        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("length")]
        public double Length { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    public class ProductUnits
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("list")]
        public List<UnitDetail> List { get; set; } = new List<UnitDetail>();
    }

    public class UnitDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("price")]
        public UnitPrice Price { get; set; }
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
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; } // Sửa sang double vì JSON thực tế có trả về số thập phân (ví dụ: 0.2)
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

        [JsonPropertyName("available")]
        public double Available { get; set; }

        [JsonPropertyName("warranty")]
        public InventoryWarranty Warranty { get; set; }

        [JsonPropertyName("depots")]
        public List<DepotInventory> Depots { get; set; } = new List<DepotInventory>();
    }

    public class InventoryWarranty
    {
        [JsonPropertyName("remain")]
        public double Remain { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }
    }

    public class DepotInventory
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

        [JsonPropertyName("available")]
        public double Available { get; set; }

        [JsonPropertyName("warranty")]
        public InventoryWarranty Warranty { get; set; }
    }

    public class ProductAttribute
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nameOther")]
        public string NameOther { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("codeOther")]
        public string CodeOther { get; set; }
    }
}