using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.XanhSmService;

/// <summary>
/// DTO trả về FE. Chỉ khai báo field frontend đang dùng;
/// System.Text.Json tự bỏ qua field mới do Xanh SM bổ sung.
/// </summary>
public static class Response
{
    public sealed class Envelope<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }

    #region Available services

    public sealed class AvailableService
    {
        [JsonPropertyName("service")]
        public ServiceInfo Service { get; init; } = new();

        [JsonPropertyName("service_hours")]
        public List<ServiceHour>? ServiceHours { get; init; }

        [JsonPropertyName("service_group")]
        public ServiceGroup? ServiceGroup { get; init; }

        [JsonPropertyName("addons")]
        public List<AvailableAddon>? Addons { get; init; }
    }

    public sealed class ServiceInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; init; }

        [JsonPropertyName("service_type")]
        public string? ServiceType { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("short_description")]
        public string? ShortDescription { get; init; }

        [JsonPropertyName("travel_mode")]
        public string? TravelMode { get; init; }

        [JsonPropertyName("vehicle_models")]
        public List<string>? VehicleModels { get; init; }

        [JsonPropertyName("express_package_sizes")]
        public List<string>? ExpressPackageSizes { get; init; }

        [JsonPropertyName("express_package_max_weight")]
        public int? ExpressPackageMaxWeight { get; init; }

        [JsonPropertyName("express_package_weight_unit")]
        public string? ExpressPackageWeightUnit { get; init; }

        [JsonPropertyName("during_service")]
        public bool DuringService { get; init; }

        [JsonPropertyName("enable")]
        public bool Enable { get; init; }
    }

    public sealed class ServiceHour
    {
        [JsonPropertyName("day_of_week")]
        public int DayOfWeek { get; init; }

        [JsonPropertyName("start_time")]
        public int StartTime { get; init; }

        [JsonPropertyName("end_time")]
        public int EndTime { get; init; }
    }

    public sealed class ServiceGroup
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    public sealed class AvailableAddon
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    #endregion

    #region Estimate

    public sealed class EstimateData
    {
        [JsonPropertyName("payment_method")]
        public string? PaymentMethod { get; init; }

        [JsonPropertyName("order_time")]
        public string? OrderTime { get; init; }

        [JsonPropertyName("service_estimates")]
        public List<ServiceEstimate> ServiceEstimates { get; init; } = [];
    }

    public sealed class ServiceEstimate
    {
        [JsonPropertyName("fee_unavailable")]
        public bool FeeUnavailable { get; init; }

        [JsonPropertyName("err_msg")]
        public string? ErrMsg { get; init; }

        [JsonPropertyName("service")]
        public ServiceInfo Service { get; init; } = new();

        [JsonPropertyName("addons")]
        public List<AvailableAddon>? Addons { get; init; }

        [JsonPropertyName("estimate_info")]
        public EstimateInfo? EstimateInfo { get; init; }
    }

    public sealed class EstimateInfo
    {
        [JsonPropertyName("fee_id")]
        public string? FeeId { get; init; }

        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; init; }

        [JsonPropertyName("increase_price")]
        public bool IncreasePrice { get; init; }

        [JsonPropertyName("estimate")]
        public FeeEstimate? Estimate { get; init; }

        [JsonPropertyName("trip")]
        public TripEstimate? Trip { get; init; }
    }

    public sealed class FeeEstimate
    {
        [JsonPropertyName("total_fee")]
        public int TotalFee { get; init; }

        [JsonPropertyName("total_pay")]
        public int TotalPay { get; init; }

        [JsonPropertyName("currency_code")]
        public string? CurrencyCode { get; init; }

        [JsonPropertyName("fee_breakdown")]
        public FeeBreakdown? FeeBreakdown { get; init; }
    }

    public sealed class FeeBreakdown
    {
        [JsonPropertyName("base_fee")]
        public int BaseFee { get; init; }

        [JsonPropertyName("surcharge_fee")]
        public int SurchargeFee { get; init; }

        [JsonPropertyName("addons_fee")]
        public int AddonsFee { get; init; }

        [JsonPropertyName("discount")]
        public int Discount { get; init; }

        [JsonPropertyName("surge_multiplier")]
        public double SurgeMultiplier { get; init; }
    }

    public sealed class TripEstimate
    {
        [JsonPropertyName("distance_estimate")]
        public double DistanceEstimate { get; init; }

        [JsonPropertyName("duration_estimate")]
        public int DurationEstimate { get; init; }

        [JsonPropertyName("distance_unit")]
        public string? DistanceUnit { get; init; }
    }

    #endregion

    #region Create and cancel order

    public sealed class CreateOrderData
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("partner_trip_id")]
        public string? PartnerTripId { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("order_time")]
        public string? OrderTime { get; init; }

        [JsonPropertyName("path")]
        public List<OrderPathPoint>? Path { get; init; }

        [JsonPropertyName("geofence_id")]
        public string? GeofenceId { get; init; }
    }

    public sealed class OrderPathPoint
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("address")]
        public string? Address { get; init; }

        [JsonPropertyName("short_address")]
        public string? ShortAddress { get; init; }
    }

    public sealed class CancelOrderData
    {
        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }

    #endregion

    #region Internal partner responses

    // Các model này chỉ dùng khi gọi Xanh SM, không trả trực tiếp cho FE.
    public sealed class Error
    {
        [JsonPropertyName("code")]
        public int? Code { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }

    public sealed class Token
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }

    #endregion
}
