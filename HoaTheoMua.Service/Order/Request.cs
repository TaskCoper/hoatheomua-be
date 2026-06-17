using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Order;

public class Request
{
    public class CreateOrderRequest : AhamoveService.Request.EstimateShippingFeeRequest;

    public class UpdateOrderRequest
    {
        public Guid OrderId { get; set; }
        public string TrackNumber { get; set; }
    }
    
    public class UpdateOrderByIdRequest
    {
        //public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        
        public string? NhanhOrderId { get; set; } 
        public string? NhanhTrackingUrl { get; set; } 
        public string? AhahaMoveOrderId { get; set; }
        public string? AhahaMoveTrackingUrl { get; set; } 
        
        public object Metadata { get; set; } = string.Empty;
        
    }
    
    public class LocationRequest
    {
        [JsonPropertyName("filters")]
        public LocationFilters Filters { get; set; }
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
    
    
}