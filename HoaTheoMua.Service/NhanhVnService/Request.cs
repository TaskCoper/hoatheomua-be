using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.NhanhVnService;

public class OrderLabelRequest
{
    [JsonPropertyName("filters")]
    public LabelFilters? Filters { get; set; }

    [JsonPropertyName("paginator")]
    public Paginator? Paginator { get; set; }
}

public class LabelFilters
{
    [JsonPropertyName("ids")]
    public List<int>? Ids { get; set; } 

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class Paginator
{
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    // Thêm trường sort (có thể là object hoặc dictionary tùy theo API cụ thể yêu cầu)
    [JsonPropertyName("sort")]
    public object? Sort { get; set; }

    // THÊM TRƯỜNG NEXT VÀO ĐÂY ĐỂ PHÂN TRANG
    [JsonPropertyName("next")]
    public RequestNextPage? Next { get; set; }
}

public class RequestNextPage
{
    // Dựa theo raw data, id của trang kế tiếp là kiểu int (Ví dụ: 262)
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}

public class OrderSourceRequest
{
    [JsonPropertyName("filters")]
    public OrderSourceFilters? Filters { get; set; }

    [JsonPropertyName("paginator")]
    public OrderSourcePaginator? Paginator { get; set; }
}

public class OrderSourceFilters
{
    // Mảng ID nguồn đơn hàng, tối đa 100 phần tử
    [JsonPropertyName("ids")]
    public List<int>? Ids { get; set; } 
}

public class OrderSourcePaginator
{
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    // Dùng object? để linh hoạt truyền dạng mảng [] hoặc object {} theo tài liệu Nhanh.vn
    [JsonPropertyName("sort")]
    public object? Sort { get; set; }

    [JsonPropertyName("next")]
    public OrderSourceNextPage? Next { get; set; }
}

public class OrderSourceNextPage
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}


//webhook-inventory-request
public class NhanhInventoryChangeWebhookRequest
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("businessId")]
        public long BusinessId { get; set; }

        // Map trực tiếp trường data thành List các sản phẩm thay đổi tồn kho
        [JsonPropertyName("data")]
        public List<NhanhInventoryItem> Data { get; set; } = new();
    }

    // Chi tiết từng sản phẩm trong mảng data
    public class NhanhInventoryItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } // ID sản phẩm trên Nhanh.vn

        [JsonPropertyName("appProductId")]
        public string AppProductId { get; set; } = string.Empty;

        [JsonPropertyName("typeId")]
        public int TypeId { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty; // Mã sản phẩm (SKU)

        [JsonPropertyName("remain")]
        public double Remain { get; set; } // Tổng tồn kho thực tế

        [JsonPropertyName("shipping")]
        public double Shipping { get; set; }

        [JsonPropertyName("damaged")]
        public double Damaged { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }

        [JsonPropertyName("transfering")]
        public double Transfering { get; set; }

        [JsonPropertyName("available")]
        public double Available { get; set; } // Số lượng có thể bán (Quan trọng nhất)

        [JsonPropertyName("warranty")]
        public NhanhWarranty Warranty { get; set; } = new();

        [JsonPropertyName("depots")]
        public List<NhanhDepot> Depots { get; set; } = new();
    }

    // Thông tin bảo hành
    public class NhanhWarranty
    {
        [JsonPropertyName("remain")]
        public double Remain { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }
    }

    // Chi tiết tồn kho theo từng Kho hàng (Depot)
    public class NhanhDepot
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } // ID của kho hàng

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
        public NhanhWarranty Warranty { get; set; } = new();
    }
    
//webhook-add-item-request
public class NhanhProductAddWebhookRequest
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty; // Sẽ là "productAdd"

    [JsonPropertyName("businessId")]
    public long BusinessId { get; set; }

    // Điểm khác biệt: "data" ở sự kiện này là một Object đơn lẻ, không phải Array
    [JsonPropertyName("data")]
    public List<NhanhProductDetailDto> Data { get; set; } = new();
}

public class NhanhProductDetailDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("parentId")]
    public long ParentId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("otherName")]
    public string? OtherName { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("category")]
    public NhanhCategoryDto? Category { get; set; }

    [JsonPropertyName("internalCategory")]
    public NhanhCategoryDto? InternalCategory { get; set; }

    [JsonPropertyName("prices")]
    public NhanhPricesDto? Prices { get; set; }

    [JsonPropertyName("vat")]
    public int? Vat { get; set; }

    [JsonPropertyName("images")]
    public NhanhImagesDto? Images { get; set; }

    [JsonPropertyName("warranty")]
    public NhanhWarrantyDto? Warranty { get; set; }

    [JsonPropertyName("brand")]
    public NhanhBrandDto? Brand { get; set; }

    [JsonPropertyName("type")]
    public NhanhTypeDto? Type { get; set; }

    [JsonPropertyName("shipping")]
    public NhanhShippingDto? Shipping { get; set; }

    [JsonPropertyName("countryName")]
    public string? CountryName { get; set; }

    [JsonPropertyName("units")]
    public NhanhUnitsDto? Units { get; set; }

    [JsonPropertyName("combos")]
    public List<NhanhComboItemDto> Combos { get; set; } = new();

    [JsonPropertyName("inventory")]
    public NhanhInventoryDto? Inventory { get; set; }

    [JsonPropertyName("attributes")]
    public List<NhanhAttributeDto>? Attributes { get; set; } = new();

    [JsonPropertyName("updatedAt")]
    public long UpdatedAt { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("suppliers")]
    public NhanhSupplierDto? Suppliers { get; set; }

    [JsonPropertyName("videos")]
    public List<NhanhVideoDto> Videos { get; set; } = new();

    [JsonPropertyName("branchPrices")]
    public List<NhanhBranchPriceDto>? BranchPrices { get; set; } = new();

    [JsonPropertyName("childs")]
    public List<object> Childs { get; set; } = new();
}

public class NhanhCategoryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhPricesDto
{
    [JsonPropertyName("retail")]
    public double Retail { get; set; }
    [JsonPropertyName("import")]
    public double Import { get; set; }
    [JsonPropertyName("old")]
    public double Old { get; set; }
    [JsonPropertyName("wholesale")]
    public double Wholesale { get; set; }
    [JsonPropertyName("avgCost")]
    public double AvgCost { get; set; }
}

public class NhanhImagesDto
{
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
    [JsonPropertyName("others")]
    public List<string> Others { get; set; } = new();
}

public class NhanhWarrantyDto
{
    [JsonPropertyName("month")]
    public int? Month { get; set; }
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

public class NhanhBrandDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhTypeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhShippingDto
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

public class NhanhUnitsDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("list")]
    public List<NhanhUnitItemDto> List { get; set; } = new();
}

public class NhanhUnitItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }
    [JsonPropertyName("price")]
    public NhanhUnitPricesDto? Price { get; set; }
}

public class NhanhUnitPricesDto
{
    [JsonPropertyName("retail")]
    public double Retail { get; set; }
    [JsonPropertyName("import")]
    public double Import { get; set; }
    [JsonPropertyName("wholesale")]
    public double Wholesale { get; set; }
}

public class NhanhComboItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }
}

public class NhanhInventoryDto
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
    [JsonPropertyName("warranty")]
    public double Warranty { get; set; }
    [JsonPropertyName("warrantyHolding")]
    public double WarrantyHolding { get; set; }
    [JsonPropertyName("holdingNew")]
    public double HoldingNew { get; set; }
    [JsonPropertyName("shippingExport")]
    public double ShippingExport { get; set; }
    [JsonPropertyName("available")]
    public double Available { get; set; }
    [JsonPropertyName("depots")]
    public List<NhanhDepotInventoryDto> Depots { get; set; } = new();
}

public class NhanhDepotInventoryDto
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
    [JsonPropertyName("warranty")]
    public double Warranty { get; set; }
    [JsonPropertyName("warrantyHolding")]
    public double WarrantyHolding { get; set; }
    [JsonPropertyName("holdingNew")]
    public double HoldingNew { get; set; }
    [JsonPropertyName("shippingExport")]
    public double ShippingExport { get; set; }
    [JsonPropertyName("available")]
    public double Available { get; set; }
}

public class NhanhAttributeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class NhanhSupplierDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }
}

public class NhanhVideoDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("src")]
    public string? Src { get; set; }
}

public class NhanhBranchPriceDto
{
    [JsonPropertyName("branch")]
    public NhanhBranchDto? Branch { get; set; }
    [JsonPropertyName("price")]
    public double Price { get; set; }
    [JsonPropertyName("wholesalePrice")]
    public double WholesalePrice { get; set; }
}

public class NhanhBranchDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhProductDeleteWebhookRequest
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("businessId")]
    public long BusinessId { get; set; }

    [JsonPropertyName("data")]
    public List<long> Data { get; set; } = new();
}