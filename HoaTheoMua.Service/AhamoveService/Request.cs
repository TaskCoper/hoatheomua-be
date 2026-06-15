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
}