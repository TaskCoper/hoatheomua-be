using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.BaoKim;

public class BaoKimResponse<T> where T : class
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public required string Message { get; set; }
    
    [JsonPropertyName("data")]
    public required T Data { get; set; }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }

    [JsonPropertyName("expired_at")]
    [JsonConverter(typeof(BaokimDateTimeConverter))]
    public DateTimeOffset ExpiredAt { get; set; }
}

public class CreateOrderResponse
{
    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("mrc_order_id")]
    public required string MrcOrderId { get; set; }

    [JsonPropertyName("payment_url")]
    public required string PaymentUrl { get; set; }
}