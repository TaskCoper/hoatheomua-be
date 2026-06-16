using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.AhamoveService;

public class Request
{
    public class EstimateFeeRequest
    {
        [JsonPropertyName("service_id")]
        public string ServiceId { get; set; } = "SGN-BIKE";

        [JsonPropertyName("path")]
        public List<OrderPoint> Points { get; set; } 
    }

    public class OrderPoint
    {
        public string Lat { get; set; } = null!;
        public string Lng { get; set; } = null!;    
        public string Address { get; set; }  = null!;
        public string Name { get; set; }     = null!;
        public string Mobile { get; set; }   = null!;
        public string Remarks { get; set; }  = null!;
    }
    
    public class CreateOrderRequestAhamove
    {
        [JsonPropertyName("order_time")] 
        public long OrderTime { get; set; } = 0;

        [JsonPropertyName("path")] 
        public List<CreateOrderPoint> Path { get; set; } = new();

        [JsonPropertyName("service_id")] 
        public string ServiceId { get; set; } = "SGN-BIKE";

        [JsonPropertyName("requests")] 
        public List<SpecialRequest>? Requests { get; set; }

        [JsonPropertyName("payment_method")] 
        public string PaymentMethod { get; set; } = "BALANCE";

        [JsonPropertyName("remarks")] 
        public string? Remarks { get; set; }

        [JsonPropertyName("promo_code")] 
        public string? PromoCode { get; set; }

        [JsonPropertyName("items")] 
        public List<AhamoveItem>? Items { get; set; } = new();

        [JsonPropertyName("package_detail")] 
        public List<PackageDetail>? PackageDetail { get; set; }
    }

    public class CreateOrderPoint
    {
        [JsonPropertyName("lat")] public object Lat { get; set; } = null!;
        [JsonPropertyName("lng")] public object Lng { get; set; } = null!;    
        [JsonPropertyName("address")] public string Address { get; set; } = null!;
        [JsonPropertyName("short_address")] public string? ShortAddress { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = null!;
        [JsonPropertyName("mobile")] public string Mobile { get; set; } = null!;
        [JsonPropertyName("cod")] public decimal Cod { get; set; } = 0;
        [JsonPropertyName("item_value")] public decimal ItemValue { get; set; } = 0;
        [JsonPropertyName("tracking_number")] public string? TrackingNumber { get; set; }
        [JsonPropertyName("remarks")] public string? Remarks { get; set; }
    }

    public class SpecialRequest
    {
        [JsonPropertyName("_id")] public string Id { get; set; } = null!;
        [JsonPropertyName("num")] public int? Num { get; set; }
        [JsonPropertyName("tier_code")] public string? TierCode { get; set; }
    }

    public class AhamoveItem
    {
        [JsonPropertyName("_id")] public string Id { get; set; } = null!;
        [JsonPropertyName("num")] public int Num { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = null!;
        [JsonPropertyName("price")] public decimal Price { get; set; }
    }

    public class PackageDetail
    {
        [JsonPropertyName("weight")] public double Weight { get; set; }
        [JsonPropertyName("length")] public double Length { get; set; }
        [JsonPropertyName("width")] public double Width { get; set; }
        [JsonPropertyName("height")] public double Height { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}