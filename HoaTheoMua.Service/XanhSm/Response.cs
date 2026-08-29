using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.XanhSm;

public class XanhSmApiResponse<T>
{
    [JsonPropertyName("data")]
    public required T Data { get; set; }
}

public class XanhSmTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

public class XanhSmAvailableServiceItem
{
    [JsonPropertyName("service")]
    public required XanhSmServiceInfo Service { get; set; }

    [JsonPropertyName("service_hours")]
    public List<XanhSmServiceHour> ServiceHours { get; set; } = [];

    [JsonPropertyName("service_group")]
    public XanhSmServiceGroup? ServiceGroup { get; set; }

    [JsonPropertyName("addons")]
    public List<XanhSmAvailableAddon>? Addons { get; set; }
}

public class XanhSmAvailableAddon
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }
}

public class XanhSmServiceInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("service_id")]
    public int? ServiceId { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("service_type")]
    public string? ServiceType { get; set; }

    [JsonPropertyName("travel_mode")]
    public string? TravelMode { get; set; }

    [JsonPropertyName("vehicle_models")]
    public List<string> VehicleModels { get; set; } = [];

    [JsonPropertyName("express_package_types")]
    public List<string> ExpressPackageTypes { get; set; } = [];

    [JsonPropertyName("express_package_sizes")]
    public List<string> ExpressPackageSizes { get; set; } = [];

    [JsonPropertyName("express_package_max_weight")]
    public double? ExpressPackageMaxWeight { get; set; }

    [JsonPropertyName("express_package_weight_unit")]
    public string? ExpressPackageWeightUnit { get; set; }

    [JsonPropertyName("max_extra_stop")]
    public int? MaxExtraStop { get; set; }

    [JsonPropertyName("max_distance")]
    public double? MaxDistance { get; set; }

    [JsonPropertyName("min_distance")]
    public double? MinDistance { get; set; }

    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("during_service")]
    public bool? DuringService { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class XanhSmServiceHour
{
    [JsonPropertyName("day_of_week")]
    public int DayOfWeek { get; set; }

    [JsonPropertyName("start_time")]
    public int StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public int EndTime { get; set; }
}

public class XanhSmServiceGroup
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("ordering")]
    public int? Ordering { get; set; }
}

public class XanhSmEstimateData
{
    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("order_time")]
    public string? OrderTime { get; set; }

    [JsonPropertyName("points")]
    public List<XanhSmDeliveryPoint> Points { get; set; } = [];

    [JsonPropertyName("service_estimates")]
    public List<XanhSmServiceEstimate> ServiceEstimates { get; set; } = [];
}

public class XanhSmDoorToDoorQuote
{
    public int ServiceId { get; init; }
    public int AddonId { get; init; }
    public required string ServiceType { get; init; }
    public required string FeeId { get; init; }
    public long TotalFee { get; init; }
    public long ExpiresAt { get; init; }
    public double? Distance { get; init; }
    public string? DistanceUnit { get; init; }
    public int? Duration { get; init; }
}

public class XanhSmServiceEstimate
{
    [JsonPropertyName("fee_unavailable")]
    public bool FeeUnavailable { get; set; }

    [JsonPropertyName("err_msg")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("service")]
    public XanhSmServiceInfo? Service { get; set; }

    [JsonPropertyName("addons")]
    public List<JsonElement>? Addons { get; set; }

    [JsonPropertyName("estimate_info")]
    public XanhSmEstimateInfo? EstimateInfo { get; set; }
}

public class XanhSmEstimateInfo
{
    [JsonPropertyName("fee_id")]
    public string? FeeId { get; set; }

    [JsonPropertyName("increase_price")]
    public bool IncreasePrice { get; set; }

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }

    [JsonPropertyName("estimate")]
    public XanhSmEstimateAmount? Estimate { get; set; }

    [JsonPropertyName("trip")]
    public XanhSmEstimatedTrip? Trip { get; set; }
}

public class XanhSmEstimateAmount
{
    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("total_fee_display")]
    public string? TotalFeeDisplay { get; set; }

    [JsonPropertyName("total_pay_display")]
    public string? TotalPayDisplay { get; set; }

    [JsonPropertyName("total_fee")]
    public long TotalFee { get; set; }

    [JsonPropertyName("total_pay")]
    public long TotalPay { get; set; }

    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("fee_breakdown")]
    public XanhSmFeeBreakdown? FeeBreakdown { get; set; }
}

public class XanhSmFeeBreakdown
{
    [JsonPropertyName("base_fee")]
    public long BaseFee { get; set; }

    [JsonPropertyName("surcharge_fee")]
    public long SurchargeFee { get; set; }

    [JsonPropertyName("addons_fee")]
    public long AddonsFee { get; set; }

    [JsonPropertyName("discount")]
    public long Discount { get; set; }

    [JsonPropertyName("surge_multiplier")]
    public double SurgeMultiplier { get; set; }
}

public class XanhSmEstimatedTrip
{
    [JsonPropertyName("distance_unit")]
    public string? DistanceUnit { get; set; }

    [JsonPropertyName("distance_estimate")]
    public double DistanceEstimate { get; set; }

    [JsonPropertyName("duration_estimate")]
    public int DurationEstimate { get; set; }
}

public class XanhSmDelivery
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }

    [JsonPropertyName("partner_trip_id")]
    public string? PartnerTripId { get; set; }

    [JsonPropertyName("tracking_url")]
    public string? TrackingUrl { get; set; }

    [JsonPropertyName("shared_link")]
    public string? SharedLink { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("service")]
    public XanhSmServiceInfo? Service { get; set; }

    [JsonPropertyName("path")]
    public List<XanhSmDeliveryPoint> Path { get; set; } = [];

    [JsonPropertyName("driver")]
    public XanhSmDriver? Driver { get; set; }

    [JsonPropertyName("vehicle")]
    public XanhSmVehicle? Vehicle { get; set; }

    [JsonPropertyName("order_time")]
    public string? OrderTime { get; set; }

    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("travel_mode")]
    public string? TravelMode { get; set; }

    [JsonPropertyName("fee_id")]
    public string? FeeId { get; set; }

    [JsonPropertyName("total_fee_display")]
    public string? TotalFeeDisplay { get; set; }

    [JsonPropertyName("total_pay_display")]
    public string? TotalPayDisplay { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class XanhSmDeliveryPoint
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("short_address")]
    public string? ShortAddress { get; set; }

    [JsonPropertyName("address_note")]
    public string? AddressNote { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("package_weight")]
    public double? PackageWeight { get; set; }

    [JsonPropertyName("package_size")]
    public string? PackageSize { get; set; }

    [JsonPropertyName("package_types")]
    public List<string>? PackageTypes { get; set; }

    [JsonPropertyName("arrive_time")]
    public string? ArriveTime { get; set; }

    [JsonPropertyName("complete_time")]
    public string? CompleteTime { get; set; }

    [JsonPropertyName("proof_of_work")]
    public XanhSmProofOfWork? ProofOfWork { get; set; }

    [JsonPropertyName("express_detail")]
    public XanhSmExpressDetail? ExpressDetail { get; set; }

    [JsonPropertyName("addon")]
    public XanhSmPointAddon? Addon { get; set; }
}

public class XanhSmDriver
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("is_device_active")]
    public bool? IsDeviceActive { get; set; }
}

public class XanhSmVehicle
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("vehicle_color_name")]
    public string? ColorName { get; set; }

    [JsonPropertyName("vehicle_image_url")]
    public string? ImageUrl { get; set; }
}

public class XanhSmProofOfWork
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("photos")]
    public List<string> Photos { get; set; } = [];
}

public class XanhSmExpressDetail
{
    [JsonPropertyName("transport_license")]
    public string? TransportLicense { get; set; }

    [JsonPropertyName("package_size")]
    public string? PackageSize { get; set; }

    [JsonPropertyName("package_types")]
    public List<XanhSmPackageType> PackageTypes { get; set; } = [];

    [JsonPropertyName("package_length")]
    public double? PackageLength { get; set; }

    [JsonPropertyName("package_width")]
    public double? PackageWidth { get; set; }

    [JsonPropertyName("package_height")]
    public double? PackageHeight { get; set; }

    [JsonPropertyName("package_weight")]
    public double? PackageWeight { get; set; }

    [JsonPropertyName("package_weight_unit")]
    public string? PackageWeightUnit { get; set; }
}

public class XanhSmPackageType
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class XanhSmPointAddon
{
    [JsonPropertyName("pay_cod_display")]
    public string? PayCodDisplay { get; set; }
}

public class XanhSmCancelDeliveryData
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

internal class XanhSmErrorResponse
{
    [JsonPropertyName("code")]
    public JsonElement Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}
