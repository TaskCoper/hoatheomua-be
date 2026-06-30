using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Order;

public class Request
{
    public class CreateOrderRequest : AhaMoveService.AhaMoveRequest.EstimateShippingFeeRequest
    {
        public string TaxCode { get; set; }
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