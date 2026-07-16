using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.BaoKim;

public class CreateAccessTokenRequest
{
    [JsonPropertyName("merchant_code")]
    public required string MerchantCode { get; set; }

    [JsonPropertyName("client_id")]
    public required string ClientId { get; set; }
    
    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; set; }
}

public class CreateOrderRequest
{
    [JsonPropertyName("request_id")]
    public required string RequestId { get; set; }

    [JsonPropertyName("request_time")]
    [JsonConverter(typeof(BaokimDateTimeConverter))]
    public DateTimeOffset RequestTime { get; set; }

    [JsonPropertyName("merchant_code")]
    public required string MerchantCode { get; set; }

    [JsonPropertyName("store_code")]
    public string? StoreCode { get; set; }

    [JsonPropertyName("branch_code")]
    public string? BranchCode { get; set; }

    [JsonPropertyName("staff_code")]
    public string? StaffCode { get; set; }

    [JsonPropertyName("mrc_order_id")]
    public required string MrcOrderId { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("total_amount")]
    public long TotalAmount { get; set; }

    [JsonPropertyName("url_success")]
    public required string UrlSuccess { get; set; }

    [JsonPropertyName("url_fail")]
    public required string UrlFail { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItem>? Items { get; set; }

    [JsonPropertyName("customer_info")]
    public CustomerInfo? CustomerInfo { get; set; }

    [JsonPropertyName("payment_method")]
    public PaymentMethod? PaymentMethod { get; set; }
}

public class OrderItem
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("link")]
    public required string Link { get; set; }
}

public class CustomerInfo
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("phone")]
    public required string Phone { get; set; }

    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("gender")]
    public int Gender { get; set; }
}