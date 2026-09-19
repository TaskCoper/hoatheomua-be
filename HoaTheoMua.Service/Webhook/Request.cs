using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace HoaTheoMua.Service.Webhook;

public class Request
{
   // Snapshot đã lưu trong OrderDetails.Metadata. Đây không phải payload từ
   // gateway; Webhook dùng class này để đọc lại dữ liệu đã chốt ở CreateOrder.
   public class OrderDetailMetadata
   {
      [JsonProperty("productMetadata")]
      public List<ProductMetadataSnapshot?>? ProductMetadata { get; set; }
   }

   public class ProductMetadataSnapshot
   {
      [JsonProperty("generatedFlower")]
      public Guid? GeneratedFlower { get; set; }

      [JsonProperty("snapShotCard")]
      public CardSnapshot? SnapShotCard { get; set; }
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
      public CardSourceSnapshot? Source { get; set; }

      [JsonProperty("cardTemplate")]
      public CardTemplateSnapshot? CardTemplate { get; set; }

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

      [JsonProperty("wordCount")]
      public int WordCount { get; set; }

      [JsonProperty("basePrice")]
      public decimal BasePrice { get; set; }

      [JsonProperty("extraPrice")]
      public decimal ExtraPrice { get; set; }

      [JsonProperty("totalPrice")]
      public decimal TotalPrice { get; set; }

      [JsonProperty("validatedAt")]
      public DateTimeOffset ValidatedAt { get; set; }
   }

   public class CardSourceSnapshot
   {
      [JsonProperty("type")]
      public string Type { get; set; } = string.Empty;

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

   public class SepayWebhookRequest
   {
      public string Gateway { get; set; } = null!;
      public string TransactionDate { get; set; } = null!;
      public string AccountNumber { get; set; } = null!;
      public string? SubAccount { get; set; }
      public string Code { get; set; } = null!;
      public string Content { get; set; } = null!;
      public string TransferType { get; set; } = null!;
      public string Description { get; set; } = null!;
      public decimal TransferAmount { get; set; }
      public string ReferenceCode { get; set; } = null!;
      public decimal Accumulated { get; set; }
      public long Id { get; set; }
   }

   public class Items
   {
      public int ItemId { get; set; }
      public int Number { get; set; }
   }

   public class AhaMoveOrderRequest
   {
      [JsonPropertyName("order_time")]
      public long OrderTime { get; set; } = 0;

      [JsonPropertyName("path")]
      public List<AhamoveLocationPoint> Paths { get; set; } = new();

      // THAY ĐỔI TẠI ĐÂY: Nhận mảng services thay vì một chuỗi đơn lẻ
      [JsonPropertyName("services")]
      public List<ServiceRequest>? Services { get; set; }

      [JsonPropertyName("payment_method")]
      public string PaymentMethod { get; set; } = "CASH";

      [JsonPropertyName("remarks")]
      public string? Remarks { get; set; } = "Ghi chú đơn hàng";

      [JsonPropertyName("items")]
      public List<AhamoveItem>? Items { get; set; } = new();

      [JsonPropertyName("package_detail")]
      public List<PackageDetail>? PackageDetail { get; set; } = null!;
   }

   public class AhamoveLocationPoint
   {
      [JsonPropertyName("lat")]
      public decimal Latitude { get; set; }
      [JsonPropertyName("lng")]
      public decimal Longitude { get; set; }
      [JsonPropertyName("address")]
      public string Address { get; set; } = null!;
      [JsonPropertyName("short_address")]
      public string ShortAddress { get; set; } = null!;
      [JsonPropertyName("name")]
      public string FullName { get; set; } = null!;
      [JsonPropertyName("mobile")]
      public string Phone { get; set; } = null!;
      [JsonPropertyName("tracking_number")]
      public string TrackingNumber { get; set; } = null!;
      [JsonPropertyName("remarks")]
      public string Remarks { get; set; } = null!;
   }

   public class AhamoveItem
   {
      [JsonPropertyName("_id")]
      public string Id { get; set; } = null!;
      [JsonPropertyName("price")]
      public int Price { get; set; }
      [JsonPropertyName("num")]
      public int Num { get; set; }
      [JsonPropertyName("name")]
      public string Name { get; set; } = null!;
   }

   public class PackageDetail
   {
      [JsonPropertyName("weight")]
      public double Weight { get; set; }
      [JsonPropertyName("length")]
      public double Length { get; set; }
      [JsonPropertyName("width")]
      public double Width { get; set; }
      [JsonPropertyName("height")]
      public double Height { get; set; }
      [JsonPropertyName("description")]
      public string Description { get; set; } = null!;
   }

   public class ServiceRequest
   {
      [JsonPropertyName("_id")]
      public required string Id { get; set; }
      [JsonPropertyName("requests")]
      public List<ItemsRequest> Requests { get; set; } = new List<ItemsRequest>();
   }

   public class ItemsRequest
   {
      [JsonPropertyName("_id")]
      public required string Id { get; set; }
      [JsonPropertyName("num")]
      public int? Num { get; set; }
      [JsonPropertyName("tier_code")]
      public string? TierCode { get; set; }
   }

}
