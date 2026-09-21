using System.Text.Json.Serialization;
using System.Transactions;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Newtonsoft.Json;

namespace HoaTheoMua.Service.Order;

public class Response
{
    public class EstimateXanhSmShippingFeeResponse
    {
        public decimal ShippingFee { get; set; }
        public string CurrencyCode { get; set; } = "VND";
        public double? Distance { get; set; }
        public string? DistanceUnit { get; set; }
        public int? Duration { get; set; }
        public long OrderTime { get; set; }
        public long ExpiresAt { get; set; }
    }

    public class GetOrderResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = null!;
        public string TrackNumber { get; set; } = null!;
        //public int? NhanhOrderId { get; set; } 
        //public string NhanhTrackingUrl { get; set; } = string.Empty;
        public string ShippingPartyOrderId { get; set; } = string.Empty;
        public string ShippingTrackingUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public long OrderTime { get; set; }
        public string? Taxcode { get; set; }
        public List<long>? TagsId { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public InvoiceType? InvoiceType { get; set; }
        public PaymentGateway PaymentGateway { get; set; }
        public ShippingGateway ShippingGateway { get; set; }
        public bool? IsExportInvoice { get; set; }
        public string ReceiptAddress { get; set; } = string.Empty;
        public string ReceiptEmail { get; set; } = string.Empty;
        public string ReceiptFullName { get; set; } = string.Empty;
        public string ReceiptPhoneNumber { get; set; } = string.Empty;
        public string Address {get; set; } = string.Empty;
        public decimal TotalFee { get; set; }
        public List<OrderItemSummaryResponse> Items { get; set; } = new();

        public DateTimeOffset CreateAt { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
    }

    public class OrderItemSummaryResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        //public decimal LineTotal { get; set; }
        public ProductSummaryResponse? Parent { get; set; }
    }

    public class ProductSummaryResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
    }
    
    public class OrderProductLookup
    {
        public Guid Id { get; init; }
        public string? NhanhProductId { get; init; }
        public string? ProductName { get; init; }
        public string? CoverImage { get; init; }
        public string? ParentNhanhProductId { get; init; }
    }

    public class GetOrderAdminResponse
    {
        public  Guid OrderId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public required int Ward { get; set; }
        public required int City { get; set; }
        public required string Address { get; set; }
        public required string ShortAddress { get; set; }
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public required string Remarks { get; set; }
        public required string Status { get; set; }
        public required string TrackNumber { get; set; }
        public int? NhanhOrderId { get; set; }
        public string? NhanhTrackingUrl { get; set; }
        public string? ShippingPartyOrderId { get; set; }
        public string? ShippingTrackingUrl { get; set; }
        public string? PaymentUrl { get; set; }
        public string? QrUrl { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal ProductFee { get; set; }
        public decimal TotalFee { get; set; }
        public required long OrderTime { get; set; } = 0;
        public List<long>? TagIds { get; set; }
        public string? TaxCode { get; set; }
        public string? IdentityNumber { get; set; }
        public string? ReceiptFullName { get; set; }
        public string? ReceiptPhoneNumber { get; set; }
        public string? ReceiptAddress { get; set; }
        public string? ReceiptEmail { get; set; }
        public bool? IsExportInvoice { get; set; }
        public decimal? Vat { get; set; } = 1.08m;
        public InvoiceType? InvoiceType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentGateway PaymentGateway { get; set; }
        public ShippingGateway ShippingGateway { get; set; }
        public DateTimeOffset CreateAt { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetOrderByIdResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = null!;
        public string TrackNumber { get; set; } = null!;
        public required string PhoneReceiver { get; set; }
        // public int? NhanhOrderId { get; set; } 
        // public string? NhanhTrackingUrl { get; set; } = string.Empty;
        public string? ShippingPartyOrderId { get; set; } = string.Empty;
        public string? ShippingTrackingUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string? TaxCode { get; set; }
        public long OrderTime { get; set; }
        public List<long>? TagsId { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public InvoiceType? InvoiceType { get; set; }
        public bool? IsExportInvoice { get; set; }
        public string ReceiptAddress { get; set; } = string.Empty;
        public string ReceiptEmail { get; set; } = string.Empty;
        public string ReceiptFullName { get; set; } = string.Empty;
        public string ReceiptPhoneNumber { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public decimal ProductFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal? VatFee { get; set; }
        public DateTimeOffset CreateAt { get; set; }
        public string? PaymentUrl { get; set; }
        public string? SepayUrl { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentGateway PaymentGateway { get; set; }
        public ShippingGateway ShippingGateway { get; set; }
        
        public string? Address { get; set; } = string.Empty;
        public string? ShortAddress { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Ward { get; set; }
        public int? City { get; set; }
        public string? FullName { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string FullNameReceiver { get; set; } = string.Empty;
        
        
        public OrderTransactionResponse? Transaction { get; set; }
        public List<OrderDetailsResponse> Details { get; set; } = new List<OrderDetailsResponse>();
    }

    public class OrderTransactionResponse
    {
        public decimal TransferAmount { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Gateway { get; set; }
        public DateTime TransactionDate { get; set; }

    }

    public class OrderDetailsResponse
    {
        public Guid OrderDetailsId { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public Guid? ProductId { get; set; }
        public string? CoverImage { get; set; }
        public Guid? ParentProductId { get; set; }
        public string? ParentProductName { get; set; }
        
        public int ItemId { get; set; }
        public decimal Number { get; set; } //int
        public decimal Price { get; set; }
        public double Weight { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Request.OrderDetailMetadata? Snapshot { get; set; }
    }

    public class LimitCoreProductResponse
    {
        public decimal SellableQuantityCombo { get; set; }
        public List<LimitCoreMaterialResponse> Materials { get; set; } = new();
    }

    public class LimitCoreMaterialResponse
    {
        public Guid MatId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsCore { get; set; }
        public bool IsActive { get; set; }
        public decimal AvailableForSale { get; set; }
    }

    public class NhanhVnLocationResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public List<LocationData> Data { get; set; } = new List<LocationData>();
    }

    public class LocationData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class TaxcodeResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = null!;

        [JsonPropertyName("data")]
        public CompanyData Data { get; set; } = null!;

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; } = null!;
    }

    public class CompanyData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("internationalName")]
        public string InternationalName { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; } = null!;

        [JsonPropertyName("address")]
        public string Address { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    public class Metadata
    {
        [JsonPropertyName("disclaimer")]
        public string Disclaimer { get; set; } = null!;

        [JsonPropertyName("source")]
        public string Source { get; set; } = null!;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("contact")]
        public string Contact { get; set; } = null!;
    }
    //Card Price
    public class CalculateCardPriceResponse  
    {  
        public string CardType { get; set; } = string.Empty; // ai | handmade  
        public string FormType { get; set; } = string.Empty; // go_may | calligraphy  
        public CardTemplatePrice CardTemplate { get; set; } = new();  
        public CardSizePrice? CardSize { get; set; }    // Handmade: null  
        public CardWordPrice? CardConfig { get; set; } // AI go_may: null  
        public int WordCount { get; set; }  
        public decimal BasePrice { get; set; }  
        public decimal ExtraPrice { get; set; }  
        public decimal TotalPrice { get; set; }       // giá một thiệp  
    }  
  
    public class CardTemplatePrice  
    {  
        public Guid TemplateId { get; set; }  
        public string TemplateName { get; set; } = string.Empty;  
        public string TemplateType { get; set; } = string.Empty;  
        public string TemplateImageUrl { get; set; } = string.Empty;  
    }  
  
    public class CardSizePrice  
    {  
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; 
        [JsonPropertyName("width")]
        public decimal Width { get; set; }  
        [JsonPropertyName("height")]
        public decimal Height { get; set; }  
  
        [JsonPropertyName("base_price")]  
        public decimal BasePrice { get; set; }  
  
        [JsonPropertyName("max_words")]  
        public int MaxWords { get; set; }  
    }  
  
    public class CardWordPrice  
    {  
        [JsonPropertyName("min_words")]  
        public int MinWords { get; set; }  
  
        [JsonPropertyName("max_words")]  
        public int MaxWords { get; set; }  
  
        [JsonPropertyName("extra_price")]  
        public decimal ExtraPrice { get; set; }  
    }  
  
    public class CardConfigItems<T>  
    {  
        [JsonPropertyName("items")]  
        public List<T> Items { get; set; } = new();  
    }
    //
    
    //Prepare for Order
    public class PreparedOrder
    {
        public List<ResolvedBouquet> Bouquets { get; set; } = new();
        public List<PreparedOrderItem> PurchaseItems { get; set; } = new();
        public List<StockRequirement> StockRequirements { get; set; } = new();
    }
    
    public class StockRequirement
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
    
    public class ResolvedBouquet
    {
        public Guid OrderLineId { get; set; }
        public int ItemId { get; set; }
        public Guid ProductId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? GeneratedFlowerId { get; set; }
        public bool AiCustomize { get; set; }
        public List<ResolvedMaterial> Materials { get; set; } = new();
        public List<ResolvedMaterial> AdditionCustome { get; set; } = new();
        public ResolvedAccessory? Ribbon { get; set; }
        public ResolvedAccessory? WrappingPaper { get; set; }
        public List<Request.BouquetCardSelection> Cards { get; set; } = new();
    }

    public class ResolvedMaterial
    {
        [JsonProperty("matId")]
        public Guid MatId { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("isCore")]
        public bool IsCore { get; set; }
    }

    public class ResolvedAccessory
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("label")]
        public string? Label { get; set; }

        [JsonProperty("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }
    
    public class PreparedOrderItem
    {
        // Mỗi Product FE gửi có một LineId riêng. Item phát sinh (giấy, ribbon,
        // phần tăng thêm) không cần Metadata nên cũng có một LineId riêng.
        public Guid LineId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Number { get; set; } //int
        public int ItemId { get; set; }
        public bool IsSourceItem { get; set; }
    }
    
    public class FlowerOrderMetadata
    {
        [JsonProperty("subId")]
        public Guid SubId { get; set; }

        [JsonProperty("aiCustomize")]
        public bool AiCustomize { get; set; }

        [JsonProperty("materials")]
        public List<ResolvedMaterial>? Materials { get; set; }

        [JsonProperty("AdditionCustome")]
        public List<ResolvedMaterial>? AdditionCustome { get; set; }

        [JsonProperty("ribbon")]
        public ResolvedAccessory? Ribbon { get; set; }

        [JsonProperty("wrappingPaper")]
        public ResolvedAccessory? WrappingPaper { get; set; }
    }
    
    ///
    public class CurrentProductSnapshot
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? CoverImage { get; set; }
        public string? Label { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
