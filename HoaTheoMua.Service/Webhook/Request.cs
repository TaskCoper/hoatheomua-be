using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

public class Request
{
   public class SepayWebhookRequest
   {
      public string Gateway { get; set; } = null!;
      public string TransactionDate { get; set; } = null!;
      public string AccountNumber { get; set; } = null!;
      public string SubAccount { get; set; } = null!;
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