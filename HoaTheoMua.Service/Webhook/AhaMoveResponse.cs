using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

public class AhaMoveResponse
{
    public class ServiceDetailsResponse
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("city_id")]
    public string CityId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("rebroadcast_interval")]
    public int RebroadcastInterval { get; set; }

    [JsonPropertyName("broadcast_distance")]
    public int BroadcastDistance { get; set; }

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    [JsonPropertyName("map_icon_url")]
    public string MapIconUrl { get; set; }

    [JsonPropertyName("distance_fee")]
    public string DistanceFee { get; set; }

    [JsonPropertyName("partner_distance_fee")]
    public string PartnerDistanceFee { get; set; }

    [JsonPropertyName("commission_type")]
    public string CommissionType { get; set; }

    [JsonPropertyName("commission_value")]
    public double CommissionValue { get; set; }

    [JsonPropertyName("pool_timeout_commission_value")]
    public double PoolTimeoutCommissionValue { get; set; }

    [JsonPropertyName("max_distance")]
    public long MaxDistance { get; set; }

    [JsonPropertyName("stop_fee")]
    public decimal StopFee { get; set; }

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; }

    [JsonPropertyName("require_to")]
    public bool RequireTo { get; set; }

    [JsonPropertyName("auto_assign")]
    public bool AutoAssign { get; set; }

    [JsonPropertyName("auto_assign_distance")]
    public int AutoAssignDistance { get; set; }

    [JsonPropertyName("require_request")]
    public bool RequireRequest { get; set; }

    [JsonPropertyName("advance_broadcast_distance")]
    public int AdvanceBroadcastDistance { get; set; }

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    [JsonPropertyName("idle_timeout")]
    public int IdleTimeout { get; set; }

    [JsonPropertyName("cod")]
    public decimal Cod { get; set; }

    [JsonPropertyName("max_cod_per_stop_point")]
    public decimal MaxCodPerStopPoint { get; set; }

    [JsonPropertyName("first_order_cod")]
    public int FirstOrderCod { get; set; }

    [JsonPropertyName("max_stop_points")]
    public int MaxStopPoints { get; set; }

    [JsonPropertyName("max_concurrent_orders")]
    public int MaxConcurrentOrders { get; set; }

    [JsonPropertyName("fee_description_en_us")]
    public string FeeDescriptionEnUs { get; set; }

    [JsonPropertyName("description_en_us")]
    public string DescriptionEnUs { get; set; }

    [JsonPropertyName("description_vi_vn")]
    public string DescriptionViVn { get; set; }

    [JsonPropertyName("fee_description_vi_vn")]
    public string FeeDescriptionViVn { get; set; }

    [JsonPropertyName("parent_id")]
    public string ParentId { get; set; }

    [JsonPropertyName("supplier_description")]
    public string SupplierDescription { get; set; }

    [JsonPropertyName("notify_package_return")]
    public bool NotifyPackageReturn { get; set; }

    [JsonPropertyName("uniform_verify_message_vi_vn")]
    public string UniformVerifyMessageViVn { get; set; }

    [JsonPropertyName("uniform_verify_message_en_us")]
    public string UniformVerifyMessageEnUs { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; }

    [JsonPropertyName("request_list")]
    public string RequestList { get; set; }

    [JsonPropertyName("cross_service")]
    public CrossService CrossService { get; set; }

    [JsonPropertyName("delivery_type")]
    public string DeliveryType { get; set; }

    [JsonPropertyName("pool_min_score")]
    public int PoolMinScore { get; set; }

    [JsonPropertyName("redirect_url")]
    public string RedirectUrl { get; set; }

    [JsonPropertyName("redirect_service_id")]
    public string RedirectServiceId { get; set; }

    [JsonPropertyName("require_redirect")]
    public bool RequireRedirect { get; set; }

    [JsonPropertyName("set_idle_status_when_creating_order")]
    public bool SetIdleStatusWhenCreatingOrder { get; set; }

    [JsonPropertyName("distance_fee_v3")]
    public DistanceFeeV3 DistanceFeeV3 { get; set; }

    // Dùng object vì json trả về null, đề phòng sau này có thể là object khác
    [JsonPropertyName("partner_distance_fee_v3")]
    public object PartnerDistanceFeeV3 { get; set; }

    [JsonPropertyName("cod_commission")]
    public CodCommission CodCommission { get; set; }

    [JsonPropertyName("pending_period")]
    public int PendingPeriod { get; set; }

    [JsonPropertyName("need_confirm_before_pickup")]
    public bool NeedConfirmBeforePickup { get; set; }

    [JsonPropertyName("delay_time")]
    public int DelayTime { get; set; }

    [JsonPropertyName("limit_call")]
    public bool LimitCall { get; set; }

    [JsonPropertyName("RouteAPIUrl")]
    public string RouteAPIUrl { get; set; }

    [JsonPropertyName("is_saas")]
    public bool IsSaas { get; set; }

    [JsonPropertyName("group_name_en_us")]
    public string GroupNameEnUs { get; set; }

    [JsonPropertyName("group_name_vi_vn")]
    public string GroupNameViVn { get; set; }

    [JsonPropertyName("group_id")]
    public string GroupId { get; set; }

    [JsonPropertyName("enable_platform")]
    public List<string> EnablePlatform { get; set; }

    [JsonPropertyName("StopFailed")]
    public int StopFailed { get; set; }

    [JsonPropertyName("RebroadcastLimit")]
    public int RebroadcastLimit { get; set; }

    [JsonPropertyName("cross_city")]
    public CrossCity CrossCity { get; set; }

    [JsonPropertyName("available_districts")]
    public List<string> AvailableDistricts { get; set; }

    [JsonPropertyName("max_cod")]
    public decimal MaxCod { get; set; }

    [JsonPropertyName("saas_distance_fee_v3")]
    public object SaasDistanceFeeV3 { get; set; }

    [JsonPropertyName("saas_stop_fee")]
    public decimal SaasStopFee { get; set; }

    [JsonPropertyName("vat_rate")]
    public double VatRate { get; set; }

    [JsonPropertyName("priority_score")]
    public int PriorityScore { get; set; }

    [JsonPropertyName("is_pay_cod_discrepancy")]
    public bool IsPayCodDiscrepancy { get; set; }

    [JsonPropertyName("premium_service_fee")]
    public decimal PremiumServiceFee { get; set; }

    [JsonPropertyName("require_pof")]
    public bool RequirePof { get; set; }

    [JsonPropertyName("pof_type")]
    public string PofType { get; set; }

    [JsonPropertyName("enable_upload_pof")]
    public bool EnableUploadPof { get; set; }

    [JsonPropertyName("require_pop")]
    public bool RequirePop { get; set; }

    [JsonPropertyName("pop_type")]
    public string PopType { get; set; }

    [JsonPropertyName("require_pod")]
    public bool RequirePod { get; set; }

    [JsonPropertyName("pod_type")]
    public string PodType { get; set; }

    [JsonPropertyName("hour_fee")]
    public object HourFee { get; set; }

    [JsonPropertyName("boundary_restricted")]
    public BoundaryRestricted BoundaryRestricted { get; set; }

    [JsonPropertyName("requests")]
    public List<SpecialRequest> Requests { get; set; }
}

public class CrossService
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("services")]
    public List<string> Services { get; set; }
}

public class DistanceFeeV3
{
    [JsonPropertyName("blocks")]
    public List<FeeBlock> Blocks { get; set; }

    [JsonPropertyName("hour_rates")]
    public List<HourRate> HourRates { get; set; }
}

public class FeeBlock
{
    [JsonPropertyName("from")]
    public double From { get; set; }

    [JsonPropertyName("to")]
    public double To { get; set; }

    [JsonPropertyName("base")]
    public decimal Base { get; set; }

    [JsonPropertyName("price_multiplier")]
    public decimal PriceMultiplier { get; set; }
}

public class HourRate
{
    [JsonPropertyName("from")]
    public int From { get; set; }

    [JsonPropertyName("to")]
    public int To { get; set; }

    [JsonPropertyName("rate")]
    public double Rate { get; set; }
}

public class CodCommission
{
    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("none_commission_limit")]
    public decimal NoneCommissionLimit { get; set; }

    [JsonPropertyName("blocks")]
    public object Blocks { get; set; }
}

public class CrossCity
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("cities")]
    public List<string> Cities { get; set; }
}

public class BoundaryRestricted
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    // Tọa độ GeoJSON Polygon có cấu trúc mảng lồng nhau 3 cấp: [[[KinhĐộ, VĩĐộ], ...]]
    [JsonPropertyName("coordinates")]
    public List<List<List<double>>> Coordinates { get; set; }
}

public class SpecialRequest
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("description_en_us")]
    public string DescriptionEnUs { get; set; }

    [JsonPropertyName("description_vi_vn")]
    public string DescriptionViVn { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    [JsonPropertyName("img_url")]
    public string ImgUrl { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; }

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("original_price")]
    public decimal OriginalPrice { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("device_types")]
    public List<string> DeviceTypes { get; set; }

    [JsonPropertyName("service_group_id")]
    public string ServiceGroupId { get; set; }

    [JsonPropertyName("transfer_cod")]
    public bool TransferCod { get; set; }

    [JsonPropertyName("no_promotion")]
    public bool NoPromotion { get; set; }

    [JsonPropertyName("no_supplier_payout")]
    public bool NoSupplierPayout { get; set; }

    [JsonPropertyName("applied_stp_num")]
    public int? AppliedStpNum { get; set; }

    [JsonPropertyName("none_cod_price_blocks")]
    public object NoneCodPriceBlocks { get; set; }

    [JsonPropertyName("subtype")]
    public string Subtype { get; set; }

    [JsonPropertyName("max_pod")]
    public decimal? MaxPod { get; set; }

    [JsonPropertyName("cod_commission")]
    public CodCommission CodCommission { get; set; }

    [JsonPropertyName("group_id")]
    public string GroupId { get; set; }

    [JsonPropertyName("tier_list")]
    public List<TierItem> TierList { get; set; }

    [JsonPropertyName("support_remarks")]
    public bool? SupportRemarks { get; set; }

    [JsonPropertyName("supplier_description_url")]
    public string SupplierDescriptionUrl { get; set; }
}

public class TierItem
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; }

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; }

    [JsonPropertyName("selectable")]
    public bool Selectable { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("price_description_vi_vn")]
    public string PriceDescriptionViVn { get; set; }

    [JsonPropertyName("price_description_en_us")]
    public string PriceDescriptionEnUs { get; set; }

    [JsonPropertyName("description_vi_vn")]
    public string DescriptionViVn { get; set; }

    [JsonPropertyName("description_en_us")]
    public string DescriptionEnUs { get; set; }

    [JsonPropertyName("img_url")]
    public string ImgUrl { get; set; }
}
    
    
    
    
    
    
    /// <summary>
    /// ////////////////////
    /// </summary>
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