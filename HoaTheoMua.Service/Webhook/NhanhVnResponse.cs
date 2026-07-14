using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

public class OrderData
{
    [JsonPropertyName("id")]
    public int Id { get; set; } // ID đơn hàng  

    [JsonPropertyName("trackingUrl")]
    public string TrackingUrl { get; set; } = null!;// Lịch trình đơn hàng
}