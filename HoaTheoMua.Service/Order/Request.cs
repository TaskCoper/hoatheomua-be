using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhamoveService;
using Newtonsoft.Json;

namespace HoaTheoMua.Service.Order;

public class Request
{
    

    public class MaterialSnapshot
    {
        public Guid MatId { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public bool IsCore { get; set; }
    }


    public class CreateOrderRequest : AhaMoveRequest.EstimateShippingFeeRequest
    {
        public required string Gateway { get; set; }
        public string? TaxCode { get; set; }
        public List<long>? TagIds { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.VA;
        public required PaymentGateway PaymentGateway { get; set; }
        public required ShippingGateway ShippingGateway { get; set; }

        public string? IdentityNumber { get; set; }

        public string? ReceiptFullName { get; set; }

        public string? ReceiptPhoneNumber { get; set; }

        public string? ReceiptAddress { get; set; }

        public string? ReceiptEmail { get; set; }

        public bool? IsExportInvoice { get; set; }
        public InvoiceType? InvoiceType { get; set; }
        // public required Guid AddressId { get; set; }
        public required string PhoneReceiver { get; set; }
        public required string FullNameReceiver { get; set; }
        // public string? Email { get; set; }
        // public string? PhoneNumber { get; set; }
        // public string? IdentityNumber { get; set; }
        // public string FullName { get; set; }
        // public string Address { get; set; }
    }

    public class LocationRequest
    {
        [JsonPropertyName("filters")]
        public LocationFilters Filters { get; set; } = null!;
    }

    public class LocationFilters
    {
        [JsonPropertyName("locationVersion")]
        public string LocationVersion { get; set; } = "v2";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "CITY";

        [JsonPropertyName("parentId")]
        public int? ParentId { get; set; }
    }

    public class EstimateXanhSmShippingFeeRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public required string Address { get; set; }
        public required string ShortAddress { get; set; }
        public required string FullName { get; set; }
        public required string Phone { get; set; }
        public string? Remarks { get; set; }
        public long OrderTime { get; set; }
        public List<AhaMoveRequest.Items> Items { get; set; } = [];
    }

    public class UpdateAhamoveRequest
    {
        public required string OrderId { get; set; }
        public string? ShippingTrackingUrl { get; set; }
        public string? TrackingNumber { get; set; }
        public decimal? ShippingFee { get; set; }
    }
    
    public class CalculateCardPriceRequest  
    {  
        // History cũng là Ai; sourceGeneratedCardId không cần để tính tiền.  
        public GeneratedCardType CardType { get; set; }  
        public Guid CardTemplateId { get; set; }  
        public string? SizeName { get; set; }          // bắt buộc với AI  
        public CardFormType? FormType { get; set; }    // AI bắt buộc; Handmade luôn Calligraphy  
        public string? MessageContent { get; set; }  
    }
    //Request order Bouquet
    public class BouquetSelection
    {
        // null: Combo thường; có ID: mẫu hoa AI đã tạo.
        public Guid? GeneratedFlowerId { get; set; }
        
        // Hàm này giữ nguyên dữ liệu Card để các bước sau dùng, không validate Card ở đây.
        public BouquetCardSelection? Card { get; set; }
    }

    public class BouquetCardSelection
    {
        public GeneratedCardType CardType { get; set; }
        public Guid CardTemplateId { get; set; }
        public string? SizeName { get; set; }
        public CardFormType? FormType { get; set; }
        public string? MessageContent { get; set; }
        public string? AttachedImageUrl { get; set; }
        public Guid? SourceGeneratedCardId { get; set; }
    }
    
    //SNAPSHOT
    public class OrderDetailMetadata
    {
        [JsonProperty("productMetadata")]
        public List<ProductMetadataSnapshot> ProductMetadata { get; set; } = new();
    }

    public class ProductMetadataSnapshot
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("subId")]
        public Guid SubId { get; set; }

        [JsonProperty("generatedFlower")]
        public Guid? GeneratedFlower { get; set; }

        [JsonProperty("aiCustomize")]
        public bool AiCustomize { get; set; }

        [JsonProperty("ribbon")]
        public AccessorySnapshot? Ribbon { get; set; }

        [JsonProperty("wrappingPaper")]
        public AccessorySnapshot? WrappingPaper { get; set; }

        [JsonProperty("AdditionCustome")]
        public List<MaterialSnapshot> AdditionCustome { get; set; } = new();

        [JsonProperty("materials")]
        public List<MaterialSnapshot> Materials { get; set; } = new();

        [JsonProperty("snapShotCard")]
        public CardSnapshot? SnapShotCard { get; set; }
    }
    public class AccessorySnapshot
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("label")]
        public string? Label { get; set; }
        [JsonProperty("imageUrl")]
        public string? ImageUrl { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }
    
    public class CardSnapshot
    {
        [JsonProperty("virtualId")]
        public Guid VirtualId { get; set; }
        [JsonProperty("cardId")]
        public Guid? CardId { get; set; }
        [JsonProperty("cardType")]
        public string CardType { get; set; } = string.Empty;
        [JsonProperty("source")]
        public CardSourceSnapshot Source { get; set; } = new();
        [JsonProperty("rawImage")]
        public string? RawImage { get; set; }
        [JsonProperty("cardTemplate")]
        public CardTemplateSnapshot CardTemplate { get; set; } = new();
        [JsonProperty("cardSize")]
        public CardSizeSnapshot? CardSize { get; set; }
        [JsonProperty("cardConfig")]
        public CardConfigSnapshot? CardConfig { get; set; }
        [JsonProperty("messageContent")]
        public string MessageContent { get; set; } = string.Empty;
        [JsonProperty("formType")]
        public string FormType { get; set; } = string.Empty;
        [JsonProperty("attachedImageUrl")]
        public string? AttachedImageUrl { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; } = 1;
        [JsonProperty("wordCount")]
        public int WordCount { get; set; }
        [JsonProperty("basePrice")]
        public decimal BasePrice { get; set; }
        [JsonProperty("extraPrice")]
        public decimal ExtraPrice { get; set; }
        [JsonProperty("totalPrice")]
        public decimal TotalPrice { get; set; }
        [JsonProperty("systemPrompt")]
        public SystemPromptSnapshot? SystemPrompt { get; set; }
        [JsonProperty("validatedAt")]
        public DateTimeOffset ValidatedAt { get; set; }
        [JsonProperty("status")]
        public string? Status { get; set; }
        [JsonProperty("idResult")]
        public List<Guid>? IdResult { get; set; }
        [JsonProperty("finalGeneratedCardId")]
        public Guid? FinalGeneratedCardId { get; set; }
    }
    
    public class CardSourceSnapshot
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "product";
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
    }
    
    public class CardTemplateSnapshot
    {
        [JsonProperty("templateId")]
        public Guid TemplateId { get; set; }
        [JsonProperty("templateName")]
        public string TemplateName { get; set; } = string.Empty;
        [JsonProperty("templateType")]
        public string TemplateType { get; set; } = string.Empty;
        [JsonProperty("templateImageUrl")]
        public string TemplateImageUrl { get; set; } = string.Empty;
    }
    
    public class CardSizeSnapshot
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("width")]
        public decimal Width { get; set; }
        [JsonProperty("height")]
        public decimal Height { get; set; }
        [JsonProperty("base_price")]
        public decimal BasePrice { get; set; }
        [JsonProperty("max_words")]
        public int MaxWords { get; set; }
    }
    
    public class CardConfigSnapshot
    {
        [JsonProperty("min_words")]
        public int MinWords { get; set; }
        [JsonProperty("max_words")]
        public int MaxWords { get; set; }
        [JsonProperty("extra_price")]
        public decimal ExtraPrice { get; set; }
    }
    
    public class SystemPromptSnapshot
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "card";
        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }
}
