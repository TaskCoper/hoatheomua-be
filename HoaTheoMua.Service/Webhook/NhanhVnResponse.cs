using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

public class OrderResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public OrderData Data { get; set; }
}

public class OrderData
{
    [JsonPropertyName("id")]
    public int Id { get; set; } // ID đơn hàng

    [JsonPropertyName("trackingUrl")]
    public string TrackingUrl { get; set; } // Lịch trình đơn hàng
}