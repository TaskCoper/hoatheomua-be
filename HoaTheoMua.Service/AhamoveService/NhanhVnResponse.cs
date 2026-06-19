namespace HoaTheoMua.Service.AhamoveService;

using System.Text.Json.Serialization;

public class NhanhProductListResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("paginator")]
    public NhanhResponsePaginator Paginator { get; set; }

    [JsonPropertyName("data")]
    public List<NhanhProductData> Data { get; set; }
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }
    [JsonPropertyName("messages")]
    public object Messages { get; set; }
}

public class NhanhResponsePaginator
{
    // Trường này trong JSON mẫu trả về object hoặc string ("next": {"id": 100} hoặc string)
    // Để linh hoạt nhất với Nhanh.vn, ta có thể dùng object hoặc cụ thể hóa nếu biết chắc kiểu dữ liệu
    [JsonPropertyName("next")]
    public object Next { get; set; }
}

public class NhanhProductData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("parentId")]
    public int? ParentId { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("barcode")]
    public string Barcode { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("otherName")]
    public string OtherName { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("priceVatMode")]
    public int PriceVatMode { get; set; }

    [JsonPropertyName("importPriceVatMode")]
    public int ImportPriceVatMode { get; set; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    [JsonPropertyName("internalCategoryId")]
    public int? InternalCategoryId { get; set; }

    [JsonPropertyName("prices")]
    public NhanhPrices Prices { get; set; }

    [JsonPropertyName("images")]
    public NhanhImages Images { get; set; }

    [JsonPropertyName("warranty")]
    [JsonConverter(typeof(NhanhWarrantyConverter))]
    public NhanhWarranty? Warranty { get; set; }

    [JsonPropertyName("brandId")]
    public int? BrandId { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("shipping")]
    public NhanhShipping Shipping { get; set; }

    [JsonPropertyName("countryName")]
    public string CountryName { get; set; }

    [JsonPropertyName("units")]
    public NhanhUnits Units { get; set; }

    [JsonPropertyName("combos")]
    public List<NhanhCombo> Combos { get; set; }

    [JsonPropertyName("inventory")]
    public NhanhInventory Inventory { get; set; }

    [JsonPropertyName("attributes")]
    public List<NhanhAttribute> Attributes { get; set; }

    [JsonPropertyName("updatedAt")]
    public long UpdatedAt { get; set; } // Sử dụng long vì thời gian thường là Unix Timestamp

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }
}

public class NhanhPrices
{
    [JsonPropertyName("retail")]
    public decimal Retail { get; set; } // Khuyên dùng decimal cho giá tiền để tránh sai số

    [JsonPropertyName("retailVat")]
    public decimal RetailVat { get; set; }

    [JsonPropertyName("import")]
    public decimal Import { get; set; }

    [JsonPropertyName("importVat")]
    public decimal ImportVat { get; set; }

    [JsonPropertyName("old")]
    public decimal? Old { get; set; }

    [JsonPropertyName("wholesale")]
    public decimal Wholesale { get; set; }

    [JsonPropertyName("avgCost")]
    public decimal AvgCost { get; set; }
}

public class NhanhImages
{
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("others")]
    public List<string> Others { get; set; }
}

public class NhanhWarranty
{
    [JsonPropertyName("month")]
    public int Month { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }
}

public class NhanhShipping
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

public class NhanhUnits
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("list")]
    public List<NhanhUnitItem> List { get; set; }
}

public class NhanhUnitItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("price")]
    public NhanhUnitPrice Price { get; set; }
}

public class NhanhUnitPrice
{
    [JsonPropertyName("retail")]
    public decimal Retail { get; set; }

    [JsonPropertyName("import")]
    public decimal Import { get; set; }

    [JsonPropertyName("wholesale")]
    public decimal Wholesale { get; set; }
}

public class NhanhCombo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
}

public class NhanhInventory
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
    public NhanhInventoryWarranty Warranty { get; set; }

    [JsonPropertyName("depots")]
    public List<NhanhDepot> Depots { get; set; }
}

public class NhanhInventoryWarranty
{
    [JsonPropertyName("remain")]
    public double Remain { get; set; }

    [JsonPropertyName("holding")]
    public double Holding { get; set; }
}

public class NhanhDepot
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
    public NhanhInventoryWarranty Warranty { get; set; }
}

public class NhanhAttribute
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