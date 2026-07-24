using System.Text.Json;
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
    public string? Event { get; set; }

    [JsonPropertyName("businessId")]
    public long? BusinessId { get; set; }

    // Map trực tiếp trường data thành List các sản phẩm thay đổi tồn kho
    [JsonPropertyName("data")]
    public List<NhanhInventoryItem>? Data { get; set; }
}

// Chi tiết từng sản phẩm trong mảng data
public class NhanhInventoryItem
{
    [JsonPropertyName("id")]
    public long? Id { get; set; } // ID sản phẩm trên Nhanh.vn

    [JsonPropertyName("appProductId")]
    public string? AppProductId { get; set; }

    [JsonPropertyName("typeId")]
    public int? TypeId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; } // Mã sản phẩm (SKU)

    [JsonPropertyName("remain")]
    public double? Remain { get; set; } // Tổng tồn kho thực tế

    [JsonPropertyName("shipping")]
    public double? Shipping { get; set; }

    [JsonPropertyName("damaged")]
    public double? Damaged { get; set; }

    [JsonPropertyName("holding")]
    public double? Holding { get; set; }

    [JsonPropertyName("transfering")]
    public double? Transfering { get; set; }

    [JsonPropertyName("available")]
    public double? Available { get; set; } // Số lượng có thể bán (Quan trọng nhất)

    [JsonPropertyName("warranty")]
    public NhanhWarranty? Warranty { get; set; }

    [JsonPropertyName("depots")]
    public List<NhanhDepot>? Depots { get; set; }
}

// Thông tin bảo hành
public class NhanhWarranty
{
    [JsonPropertyName("remain")]
    public double? Remain { get; set; }

    [JsonPropertyName("holding")]
    public double? Holding { get; set; }
}

// Chi tiết tồn kho theo từng Kho hàng (Depot)
public class NhanhDepot
{
    [JsonPropertyName("id")]
    public long? Id { get; set; } // ID của kho hàng

    [JsonPropertyName("remain")]
    public double? Remain { get; set; }

    [JsonPropertyName("shipping")]
    public double? Shipping { get; set; }

    [JsonPropertyName("damaged")]
    public double? Damaged { get; set; }

    [JsonPropertyName("holding")]
    public double? Holding { get; set; }

    [JsonPropertyName("transfering")]
    public double? Transfering { get; set; }

    [JsonPropertyName("available")]
    public double? Available { get; set; }

    [JsonPropertyName("warranty")]
    public NhanhWarranty? Warranty { get; set; }
}

//webhook-add-item-request
public class NhanhProductAddWebhookRequest
{
    [JsonPropertyName("event")]
    public string? Event { get; set; } // Sẽ là "productAdd"

    [JsonPropertyName("businessId")]
    public long? BusinessId { get; set; }

    // "data" ở sự kiện này là một object đơn lẻ, không phải array
    [JsonPropertyName("data")]
    public NhanhProductDetailDto? Data { get; set; }
}

public class NhanhProductDetailDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("parentId")]
    public long? ParentId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("otherName")]
    public string? OtherName { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

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
    public JsonElement? Warranty { get; set; }

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
    public List<NhanhComboItemDto>? Combos { get; set; }

    [JsonPropertyName("inventory")]
    public NhanhInventoryDto? Inventory { get; set; }

    [JsonPropertyName("attributes")]
    public List<NhanhAttributeDto>? Attributes { get; set; }

    [JsonPropertyName("updatedAt")]
    public long? UpdatedAt { get; set; }

    [JsonPropertyName("createdAt")]
    public long? CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("suppliers")]
    public List<NhanhSupplierDto>? Suppliers { get; set; }

    [JsonPropertyName("videos")]
    public List<NhanhVideoDto>? Videos { get; set; }

    [JsonPropertyName("branchPrices")]
    public List<NhanhBranchPriceDto>? BranchPrices { get; set; }

    [JsonPropertyName("childs")]
    public List<object>? Childs { get; set; }
}

public class NhanhCategoryDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhPricesDto
{
    [JsonPropertyName("retail")]
    public double? Retail { get; set; }
    [JsonPropertyName("import")]
    public double? Import { get; set; }
    [JsonPropertyName("old")]
    public double? Old { get; set; }
    [JsonPropertyName("wholesale")]
    public double? Wholesale { get; set; }
    [JsonPropertyName("avgCost")]
    public double? AvgCost { get; set; }
}

public class NhanhImagesDto
{
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
    [JsonPropertyName("others")]
    public List<string>? Others { get; set; }
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
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhTypeDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhShippingDto
{
    [JsonPropertyName("length")]
    public double? Length { get; set; }
    [JsonPropertyName("width")]
    public double? Width { get; set; }
    [JsonPropertyName("height")]
    public double? Height { get; set; }
    [JsonPropertyName("weight")]
    public double? Weight { get; set; }
}

public class NhanhUnitsDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("list")]
    public List<NhanhUnitItemDto>? List { get; set; }
}

public class NhanhUnitItemDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("quantity")]
    public double? Quantity { get; set; }
    [JsonPropertyName("price")]
    public NhanhUnitPricesDto? Price { get; set; }
}

public class NhanhUnitPricesDto
{
    [JsonPropertyName("retail")]
    public double? Retail { get; set; }
    [JsonPropertyName("import")]
    public double? Import { get; set; }
    [JsonPropertyName("wholesale")]
    public double? Wholesale { get; set; }
}

public class NhanhComboItemDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("quantity")]
    public double? Quantity { get; set; }
}

public class NhanhInventoryDto
{
    [JsonPropertyName("remain")]
    public double? Remain { get; set; }

    [JsonPropertyName("shipping")]
    public double? Shipping { get; set; }

    [JsonPropertyName("damaged")]
    public double? Damaged { get; set; }

    [JsonPropertyName("holding")]
    public double? Holding { get; set; }

    [JsonPropertyName("transfering")]
    public double? Transfering { get; set; }

    [JsonPropertyName("available")]
    public double? Available { get; set; }

    [JsonPropertyName("warranty")]
    public NhanhWarranty? Warranty { get; set; }

    [JsonPropertyName("depots")]
    public List<NhanhDepotInventoryDto>? Depots { get; set; }
}

public class NhanhDepotInventoryDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("remain")]
    public double? Remain { get; set; }

    [JsonPropertyName("shipping")]
    public double? Shipping { get; set; }

    [JsonPropertyName("damaged")]
    public double? Damaged { get; set; }

    [JsonPropertyName("holding")]
    public double? Holding { get; set; }

    [JsonPropertyName("transfering")]
    public double? Transfering { get; set; }

    [JsonPropertyName("available")]
    public double? Available { get; set; }

    [JsonPropertyName("warranty")]
    public NhanhWarranty? Warranty { get; set; }
}

public class NhanhAttributeDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class NhanhSupplierDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
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
    public double? Price { get; set; }
    [JsonPropertyName("wholesalePrice")]
    public double? WholesalePrice { get; set; }
}

public class NhanhBranchDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

//webhook-order-add
public class NhanhOrderAddWebhookRequest
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("businessId")]
    public long? BusinessId { get; set; }

    [JsonPropertyName("data")]
    public NhanhOrderAddDataDto? Data { get; set; }
}

//webhook-order-update
public class NhanhOrderUpdateWebhookRequest
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("businessId")]
    public long? BusinessId { get; set; }

    [JsonPropertyName("data")]
    public NhanhOrderAddDataDto? Data { get; set; }
}

public class NhanhOrderAddDataDto
{
    [JsonPropertyName("info")]
    public NhanhOrderInfoDto? Info { get; set; }

    [JsonPropertyName("channel")]
    public NhanhOrderChannelDto? Channel { get; set; }

    [JsonPropertyName("shippingAddress")]
    public NhanhShippingAddressDto? ShippingAddress { get; set; }

    [JsonPropertyName("products")]
    public List<NhanhOrderProductDto>? Products { get; set; }

    [JsonPropertyName("carrier")]
    public NhanhCarrierDto? Carrier { get; set; }

    [JsonPropertyName("payment")]
    public NhanhPaymentDto? Payment { get; set; }

    [JsonPropertyName("originalOrder")]
    public NhanhOriginalOrderDto? OriginalOrder { get; set; }
}

public class NhanhOrderInfoDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("depotId")]
    public long? DepotId { get; set; }

    [JsonPropertyName("pickupDepotId")]
    public long? PickupDepotId { get; set; }

    [JsonPropertyName("type")]
    public int? Type { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("createdBy")]
    public NhanhEmployeeDto? CreatedBy { get; set; }

    [JsonPropertyName("sale")]
    public NhanhEmployeeDto? Sale { get; set; }

    [JsonPropertyName("technicalStaff")]
    public NhanhEmployeeDto? TechnicalStaff { get; set; }

    [JsonPropertyName("customerCare")]
    public NhanhEmployeeDto? CustomerCare { get; set; }

    // Unix timestamp
    [JsonPropertyName("createdAt")]
    public long? CreatedAt { get; set; }

    // Unix timestamp
    [JsonPropertyName("updatedAt")]
    public long? UpdatedAt { get; set; }

    // Unix timestamp
    [JsonPropertyName("confirmedAt")]
    public long? ConfirmedAt { get; set; }

    // Unix timestamp
    [JsonPropertyName("packedAt")]
    public long? PackedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("privateDescription")]
    public string? PrivateDescription { get; set; }

    [JsonPropertyName("trackingUrl")]
    public string? TrackingUrl { get; set; }
}

public class NhanhEmployeeDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhOrderChannelDto
{
    [JsonPropertyName("appOrderId")]
    public string? AppOrderId { get; set; }

    [JsonPropertyName("appShopId")]
    public string? AppShopId { get; set; }

    [JsonPropertyName("appShopName")]
    public string? AppShopName { get; set; }

    [JsonPropertyName("saleChannel")]
    public int? SaleChannel { get; set; }

    [JsonPropertyName("trafficSource")]
    public NhanhTrafficSourceDto? TrafficSource { get; set; }

    [JsonPropertyName("facebook")]
    public NhanhFacebookChannelDto? Facebook { get; set; }

    [JsonPropertyName("utmCampaign")]
    public string? UtmCampaign { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
}

public class NhanhTrafficSourceDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class NhanhFacebookChannelDto
{
    [JsonPropertyName("pageId")]
    public string? PageId { get; set; }

    [JsonPropertyName("conversationId")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("adId")]
    public string? AdId { get; set; }

    [JsonPropertyName("postId")]
    public string? PostId { get; set; }
}

public class NhanhShippingAddressDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("cityId")]
    public long? CityId { get; set; }

    [JsonPropertyName("districtId")]
    public long? DistrictId { get; set; }

    [JsonPropertyName("wardId")]
    public long? WardId { get; set; }

    [JsonPropertyName("locationVersion")]
    public string? LocationVersion { get; set; }
}

public class NhanhOrderProductDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("typeId")]
    public long? TypeId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("originalPrice")]
    public decimal? OriginalPrice { get; set; }

    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    [JsonPropertyName("available")]
    public decimal? Available { get; set; }

    [JsonPropertyName("avgCost")]
    public decimal? AverageCost { get; set; }

    [JsonPropertyName("vat")]
    public NhanhProductVatDto? Vat { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("imei")]
    public NhanhProductImeiDto? Imei { get; set; }

    [JsonPropertyName("batch")]
    public NhanhProductBatchDto? Batch { get; set; }

    [JsonPropertyName("combos")]
    public List<NhanhProductRelationDto>? Combos { get; set; }

    [JsonPropertyName("unit")]
    public NhanhProductUnitDto? Unit { get; set; }

    [JsonPropertyName("gifts")]
    public List<NhanhProductRelationDto>? Gifts { get; set; }
}

public class NhanhProductVatDto
{
    [JsonPropertyName("value")]
    public int? Value { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}

public class NhanhProductImeiDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

public class NhanhProductBatchDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("manufactureDate")]
    public string? ManufactureDate { get; set; }

    [JsonPropertyName("expiredDate")]
    public string? ExpiredDate { get; set; }
}

public class NhanhProductRelationDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }
}

public class NhanhProductUnitDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }
}

public class NhanhCarrierDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("orderPackageSize")]
    public NhanhOrderPackageSizeDto? OrderPackageSize { get; set; }

    [JsonPropertyName("service")]
    public NhanhCarrierServiceDto? Service { get; set; }

    [JsonPropertyName("carrierCode")]
    public string? CarrierCode { get; set; }

    [JsonPropertyName("isPartDelivery")]
    public int? IsPartDelivery { get; set; }

    [JsonPropertyName("sendCarrier")]
    public NhanhSendCarrierDto? SendCarrier { get; set; }

    [JsonPropertyName("deliveryDate")]
    public string? DeliveryDate { get; set; }

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }

    [JsonPropertyName("shipFee")]
    public decimal? ShipFee { get; set; }

    [JsonPropertyName("codFee")]
    public decimal? CodFee { get; set; }

    [JsonPropertyName("declaredFee")]
    public decimal? DeclaredFee { get; set; }

    [JsonPropertyName("returnFee")]
    public decimal? ReturnFee { get; set; }

    [JsonPropertyName("customerShipFee")]
    public decimal? CustomerShipFee { get; set; }
}

public class NhanhOrderPackageSizeDto
{
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("merge")]
    public int? Merge { get; set; }

    [JsonPropertyName("length")]
    public int? Length { get; set; }

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

public class NhanhCarrierServiceDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

public class NhanhSendCarrierDto
{
    [JsonPropertyName("type")]
    public int? Type { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

public class NhanhPaymentDto
{
    [JsonPropertyName("couponCode")]
    public string? CouponCode { get; set; }

    [JsonPropertyName("credit")]
    public NhanhCreditPaymentDto? Credit { get; set; }

    [JsonPropertyName("deposit")]
    public NhanhDepositPaymentDto? Deposit { get; set; }

    [JsonPropertyName("discount")]
    public NhanhOrderDiscountDto? Discount { get; set; }

    [JsonPropertyName("transfer")]
    public NhanhTransferPaymentDto? Transfer { get; set; }

    [JsonPropertyName("usedPoints")]
    public NhanhUsedPointsDto? UsedPoints { get; set; }

    [JsonPropertyName("vat")]
    public NhanhPaymentVatDto? Vat { get; set; }
}

public class NhanhCreditPaymentDto
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("accountId")]
    public long? AccountId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

public class NhanhDepositPaymentDto
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("accountId")]
    public long? AccountId { get; set; }
}

public class NhanhOrderDiscountDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}

public class NhanhTransferPaymentDto
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("accountId")]
    public long? AccountId { get; set; }
}

public class NhanhUsedPointsDto
{
    [JsonPropertyName("points")]
    public int? Points { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}

public class NhanhPaymentVatDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public decimal? Value { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("taxCode")]
    public string? TaxCode { get; set; }

    [JsonPropertyName("taxDate")]
    public string? TaxDate { get; set; }
}

public class NhanhOriginalOrderDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("channel")]
    public NhanhOriginalOrderChannelDto? Channel { get; set; }
}

public class NhanhOriginalOrderChannelDto
{
    [JsonPropertyName("appOrderId")]
    public string? AppOrderId { get; set; }

    [JsonPropertyName("appShopId")]
    public string? AppShopId { get; set; }

    [JsonPropertyName("saleChannel")]
    public int? SaleChannel { get; set; }
}

//webhook-order-delete
public class NhanhOrderDeleteWebhookRequest
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("businessId")]
    public long? BusinessId { get; set; }

    [JsonPropertyName("data")]
    public List<long>? Data { get; set; }
}

public class NhanhProductDeleteWebhookRequest
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("businessId")]
    public long? BusinessId { get; set; }

    [JsonPropertyName("data")]
    public List<string>? Data { get; set; }
}