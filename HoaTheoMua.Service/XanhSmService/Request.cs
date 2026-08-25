using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.XanhSmService;

/// <summary>
/// DTO FE gửi vào và backend chuyển tiếp sang Xanh SM.
/// JsonPropertyName giữ đúng snake_case theo hợp đồng của Xanh SM.
/// </summary>
public static class Request
{
    public sealed class Coordinate
    {
        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }
    }

    #region Available services

    public sealed class GetAvailableServicesRequest
    {
        [JsonPropertyName("pickups")]
        public List<Coordinate> Pickups { get; init; } = [];

        [JsonPropertyName("dropoffs")]
        public List<Coordinate>? Dropoffs { get; init; }

        [JsonPropertyName("service_types")]
        public List<string>? ServiceTypes { get; init; }

        [JsonPropertyName("order_time")]
        public long? OrderTime { get; init; }
    }

    #endregion

    #region Shared route and addon objects

    public sealed class RoutePoint
    {
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        // Chỉ bắt buộc tại PICK UP; nullable để DROP OFF không bị yêu cầu gửi field này.
        [JsonPropertyName("package_weight")]
        public int? PackageWeight { get; init; }

        [JsonPropertyName("package_size")]
        public string? PackageSize { get; init; }

        [JsonPropertyName("package_types")]
        public List<string>? PackageTypes { get; init; }

        [JsonPropertyName("short_address")]
        public string? ShortAddress { get; init; }

        [JsonPropertyName("address")]
        public string? Address { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("address_note")]
        public string? AddressNote { get; init; }

        // Chỉ dùng khi payment_method = cash. Đúng một route point được phép là true.
        [JsonPropertyName("is_payment_check_point")]
        public bool? IsPaymentCheckPoint { get; init; }

        // COD phải là object tại DROP OFF, không được là array.
        [JsonPropertyName("addons")]
        public PointAddon? Addons { get; init; }
    }

    public sealed class PointAddon
    {
        [JsonPropertyName("collect_cod")]
        public int CollectCod { get; init; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; init; } = "VND";
    }

    public sealed class Addon
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("apply")]
        public bool Apply { get; init; }

        // Bắt buộc cho COD (addon 4 hoặc 11).
        [JsonPropertyName("remain_cod")]
        public int? RemainCod { get; init; }

        // Xanh SM yêu cầu giá trị 1 cho COD.
        [JsonPropertyName("value")]
        public int? Value { get; init; }
    }

    #endregion

    #region Estimate

    public sealed class EstimateService
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        // Estimate đặt addons bên trong từng service.
        [JsonPropertyName("addons")]
        public List<Addon>? Addons { get; init; }
    }

    public sealed class EstimateOrderRequest
    {
        [JsonPropertyName("service_ids")]
        public List<EstimateService> ServiceIds { get; init; } = [];

        [JsonPropertyName("order_time")]
        public long? OrderTime { get; init; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; init; } = string.Empty;

        [JsonPropertyName("points")]
        public List<RoutePoint> Points { get; init; } = [];
    }

    #endregion

    #region Create and cancel order

    // Tên DTO có tiền tố XanhSm để không trùng CreateOrderRequest của module Order trên Swagger.
    public sealed class CreateXanhSmOrderRequest
    {
        // Nullable giúp validator phân biệt field bị bỏ quên với giá trị số 0.
        [JsonPropertyName("order_time")]
        public long? OrderTime { get; init; }

        [JsonPropertyName("service_id")]
        public int ServiceId { get; init; }

        [JsonPropertyName("service_type")]
        public string ServiceType { get; init; } = string.Empty;

        [JsonPropertyName("fee_id")]
        public string FeeId { get; init; } = string.Empty;

        [JsonPropertyName("goods_value")]
        public int? GoodsValue { get; init; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; init; } = string.Empty;

        // Khác Estimate, Create Order đặt addons ở root request.
        [JsonPropertyName("addons")]
        public List<Addon>? Addons { get; init; }

        [JsonPropertyName("points")]
        public List<RoutePoint> Points { get; init; } = [];

        [JsonPropertyName("partner_trip_id")]
        public string? PartnerTripId { get; init; }
    }

    public sealed class CancelOrderRequest
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; init; } = string.Empty;

        [JsonPropertyName("cancel_code")]
        public int CancelCode { get; init; } = 9000;

        [JsonPropertyName("cancel_message")]
        public string CancelMessage { get; init; } = string.Empty;
    }

    #endregion
}
