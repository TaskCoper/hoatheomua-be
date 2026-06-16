using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

public class Request
{
    public class SepayWebhookRequest
    {
        public string Gateway { get; set; }
        public string  TransactionDate { get; set; }
        public string AccountNumber { get; set; }
        public string SubAccount { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public string TransferType { get; set; }
        public string Description { get; set; }
        public decimal TransferAmount { get; set; }
        public string ReferenceCode { get; set; }
        public decimal Accumulated { get; set; }
        public long Id {get; set;}
    }
    
    public class OrderRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; }
        public string ShortAddress { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Remarks { get; set; }
        public string TrackingNumber { get; set; }
        public List<Items> Items { get; set; } = new List<Items>();
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
      public List<PackageDetail>? PackageDetail { get; set; }
   }
   
   public class AhamoveLocationPoint
   {
      [JsonPropertyName("lat")] 
      public decimal Latitude { get; set; }
      [JsonPropertyName("lng")] 
      public decimal Longitude { get; set; }
      [JsonPropertyName("address")]
      public string Address { get; set; }
      [JsonPropertyName("short_address")]
      public string ShortAddress { get; set; }
      [JsonPropertyName("name")]
      public string FullName { get; set; }
      [JsonPropertyName("mobile")]
      public string Phone { get; set; }
      [JsonPropertyName("tracking_number")]
      public string TrackingNumber { get; set; }
      [JsonPropertyName("remarks")]
      public string Remarks { get; set; }
   }
   
   public class AhamoveItem
   {
      [JsonPropertyName("_id")] 
      public string Id { get; set; }
      [JsonPropertyName("price")]
      public int Price { get; set; }
      [JsonPropertyName("num")]
      public int Num {get; set;}
      [JsonPropertyName("name")]
      public string Name {get; set;}
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
      public string Description { get; set; }
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