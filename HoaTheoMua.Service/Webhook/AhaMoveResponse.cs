using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

public class AhaMoveResponse
{
    public class CreateAhaMoveOrderResponse
{
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("shared_link")]
    public string SharedLink { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public OrderDetail Order { get; set; } = new();
}

public class OrderDetail
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("user_main_account")]
    public decimal UserMainAccount { get; set; }

    [JsonPropertyName("user_bonus_account")]
    public decimal UserBonusAccount { get; set; }

    [JsonPropertyName("total_pay")]
    public decimal TotalPay { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("distance_fee")]
    public decimal DistanceFee { get; set; }

    [JsonPropertyName("request_fee")]
    public decimal RequestFee { get; set; }

    [JsonPropertyName("stop_fee")]
    public decimal StopFee { get; set; }

    [JsonPropertyName("vat_fee")]
    public decimal VatFee { get; set; }

    [JsonPropertyName("discount")]
    public decimal Discount { get; set; }

    [JsonPropertyName("path")]
    public List<OrderPath> Path { get; set; } = new();

    [JsonPropertyName("requests")]
    public List<OrderRequest> Requests { get; set; } = new();

    [JsonPropertyName("items")]
    public List<object> Items { get; set; } = new(); // Left as object since it's empty in payload

    [JsonPropertyName("total_fee")]
    public decimal TotalFee { get; set; }

    [JsonPropertyName("remarks")]
    public string Remarks { get; set; } = string.Empty;

    [JsonPropertyName("app")]
    public string App { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [JsonPropertyName("city_id")]
    public string CityId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("user_imei")]
    public string? UserImei { get; set; }

    [JsonPropertyName("create_time")]
    public double CreateTime { get; set; }

    [JsonPropertyName("order_time")]
    public double OrderTime { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("partner")]
    public string Partner { get; set; } = string.Empty;

    [JsonPropertyName("polylines")]
    public string Polylines { get; set; } = string.Empty;

    [JsonPropertyName("online_pay")]
    public decimal OnlinePay { get; set; }

    [JsonPropertyName("notify_package_return")]
    public bool NotifyPackageReturn { get; set; }

    [JsonPropertyName("uniform_user_feedback")]
    public int UniformUserFeedback { get; set; }

    [JsonPropertyName("pending_period")]
    public int PendingPeriod { get; set; }

    [JsonPropertyName("stoppoint_price")]
    public decimal StoppointPrice { get; set; }

    [JsonPropertyName("special_request_price")]
    public decimal SpecialRequestPrice { get; set; }

    [JsonPropertyName("vat")]
    public decimal Vat { get; set; }

    [JsonPropertyName("distance_price")]
    public decimal DistancePrice { get; set; }

    [JsonPropertyName("voucher_discount")]
    public decimal VoucherDiscount { get; set; }

    [JsonPropertyName("subtotal_price")]
    public decimal SubtotalPrice { get; set; }

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }
}

public class OrderPath
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("short_address")]
    public string ShortAddress { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; } = string.Empty;

    [JsonPropertyName("remarks")]
    public string Remarks { get; set; } = string.Empty;

    [JsonPropertyName("cod")]
    public decimal? Cod { get; set; }

    [JsonPropertyName("item_value")]
    public decimal? ItemValue { get; set; }

    [JsonPropertyName("tracking_number")]
    public string? TrackingNumber { get; set; }
}

public class OrderRequest
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("num")]
    public int? Num { get; set; }

    [JsonPropertyName("tier_code")]
    public string? TierCode { get; set; }
}
}