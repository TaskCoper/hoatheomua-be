using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.XanhSm;

public class XanhSmCoordinatePoint
{
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }
}

public class XanhSmAvailableServicesRequest
{
    [JsonPropertyName("pickups")]
    public required List<XanhSmCoordinatePoint> Pickups { get; set; }

    [JsonPropertyName("dropoffs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<XanhSmCoordinatePoint>? Dropoffs { get; set; }

    [JsonPropertyName("service_types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ServiceTypes { get; set; }

    [JsonPropertyName("order_time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? OrderTime { get; set; }
}

public class XanhSmAddonRequest
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("apply")]
    public bool Apply { get; init; } = true;
}

public class XanhSmEstimateServiceRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("addons")]
    public IReadOnlyList<XanhSmAddonRequest> Addons { get; init; } = [];
}

public class XanhSmDeliveryPointRequest : XanhSmCoordinatePoint
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("phone_number")]
    public required string PhoneNumber { get; set; }

    [JsonPropertyName("short_address")]
    public required string ShortAddress { get; set; }

    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("address_note")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressNote { get; set; }

    [JsonPropertyName("package_weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PackageWeight { get; set; }

    [JsonPropertyName("package_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PackageSize { get; set; }

    [JsonPropertyName("package_types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? PackageTypes { get; set; }

}

public class XanhSmEstimateOrderRequest
{
    [JsonPropertyName("service_ids")]
    public required List<XanhSmEstimateServiceRequest> ServiceIds { get; set; }

    [JsonPropertyName("order_time")]
    public long OrderTime { get; set; }

    [JsonPropertyName("payment_method")]
    public string PaymentMethod => XanhSmConstants.PartnerPay;

    [JsonPropertyName("points")]
    public required List<XanhSmDeliveryPointRequest> Points { get; set; }
}

public class XanhSmBookDeliveryRequest
{
    [JsonPropertyName("order_time")]
    public long OrderTime { get; set; }

    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }

    [JsonPropertyName("service_type")]
    public required string ServiceType { get; set; }

    [JsonPropertyName("fee_id")]
    public required string FeeId { get; set; }

    [JsonPropertyName("payment_method")]
    public string PaymentMethod => XanhSmConstants.PartnerPay;

    [JsonPropertyName("points")]
    public required List<XanhSmDeliveryPointRequest> Points { get; set; }

    [JsonPropertyName("addon_ids")]
    public IReadOnlyList<int> AddonIds { get; init; } = [];

    [JsonPropertyName("goods_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? GoodsValue { get; set; }

    [JsonPropertyName("partner_trip_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PartnerTripId { get; set; }
}

public class XanhSmCancelDeliveryRequest
{
    [JsonPropertyName("order_id")]
    public required string OrderId { get; set; }

    [JsonPropertyName("cancel_code")]
    public int CancelCode { get; set; } = 9000;

    [JsonPropertyName("cancel_message")]
    public required string CancelMessage { get; set; }
}
