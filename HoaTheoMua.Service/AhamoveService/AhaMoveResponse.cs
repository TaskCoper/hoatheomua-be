using System.Text.Json.Nodes;

namespace HoaTheoMua.Service.AhaMoveService;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


 public class ServiceDetailsResponse
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("enable")]
        public bool? Enable { get; set; }

        [JsonPropertyName("order")]
        public int? Order { get; set; }

        [JsonPropertyName("city_id")]
        public string CityId { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("name_vi_vn")]
        public string NameViVn { get; set; } = null!;

        [JsonPropertyName("name_en_us")]
        public string NameEnUs { get; set; } = null!;

        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = null!;

        [JsonPropertyName("map_icon_url")]
        public string MapIconUrl { get; set; } = null!;

        [JsonPropertyName("distance_fee")]
        public string DistanceFee { get; set; } = null!;

        [JsonPropertyName("partner_distance_fee")]
        public string PartnerDistanceFee { get; set; } = null!;

        [JsonPropertyName("commission_type")]
        public string CommissionType { get; set; } = null!;

        [JsonPropertyName("commission_value")]
        public double? CommissionValue { get; set; }

        [JsonPropertyName("pool_timeout_commission_value")]
        public double? PoolTimeoutCommissionValue { get; set; }

        [JsonPropertyName("max_distance")]
        public long? MaxDistance { get; set; }

        [JsonPropertyName("stop_fee")]
        public decimal? StopFee { get; set; }

        [JsonPropertyName("rebroadcast_interval")]
        public int? RebroadcastInterval { get; set; }

        [JsonPropertyName("broadcast_distance")]
        public int? BroadcastDistance { get; set; }

        [JsonPropertyName("require_to")]
        public bool? RequireTo { get; set; }

        [JsonPropertyName("auto_assign")]
        public bool? AutoAssign { get; set; }

        [JsonPropertyName("auto_assign_distance")]
        public int? AutoAssignDistance { get; set; }

        [JsonPropertyName("require_request")]
        public bool? RequireRequest { get; set; }

        [JsonPropertyName("advance_broadcast_distance")]
        public int? AdvanceBroadcastDistance { get; set; }

        [JsonPropertyName("timeout")]
        public int? Timeout { get; set; }

        [JsonPropertyName("idle_timeout")]
        public int? IdleTimeout { get; set; }

        [JsonPropertyName("cod")]
        public decimal? Cod { get; set; }

        [JsonPropertyName("max_cod_per_stop_point")]
        public decimal? MaxCodPerStopPoint { get; set; }

        [JsonPropertyName("first_order_cod")]
        public int? FirstOrderCod { get; set; }

        [JsonPropertyName("max_stop_points")]
        public int? MaxStopPoints { get; set; }

        [JsonPropertyName("max_concurrent_orders")]
        public int? MaxConcurrentOrders { get; set; }

        [JsonPropertyName("fee_description_en_us")]
        public string FeeDescriptionEnUs { get; set; } = null!;

        [JsonPropertyName("description_en_us")]
        public string DescriptionEnUs { get; set; } = null!;

        [JsonPropertyName("description_vi_vn")]
        public string DescriptionViVn { get; set; } = null!;

        [JsonPropertyName("fee_description_vi_vn")]
        public string FeeDescriptionViVn { get; set; } = null!;

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; } = null!;

        [JsonPropertyName("supplier_description")]
        public string SupplierDescription { get; set; } = null!;

        [JsonPropertyName("notify_package_return")]
        public bool? NotifyPackageReturn { get; set; }

        [JsonPropertyName("uniform_verify_message_vi_vn")]
        public string UniformVerifyMessageViVn { get; set; } = null!;

        [JsonPropertyName("uniform_verify_message_en_us")]
        public string UniformVerifyMessageEnUs { get; set; } = null!;

        [JsonPropertyName("note")]
        public string Note { get; set; } = null!;

        [JsonPropertyName("request_list")]
        public string RequestList { get; set; } = null!;

        [JsonPropertyName("cross_service")]
        public CrossService CrossService { get; set; } = null!;

        [JsonPropertyName("delivery_type")]
        public string DeliveryType { get; set; } = null!;

        [JsonPropertyName("pool_min_score")]
        public int? PoolMinScore { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; } = null!;

        [JsonPropertyName("redirect_service_id")]
        public string RedirectServiceId { get; set; } = null!;

        [JsonPropertyName("require_redirect")]
        public bool? RequireRedirect { get; set; }

        [JsonPropertyName("set_idle_status_when_creating_order")]
        public bool? SetIdleStatusWhenCreatingOrder { get; set; }

        [JsonPropertyName("distance_fee_v3")]
        public DistanceFeeV3 DistanceFeeV3 { get; set; } = null!;

        [JsonPropertyName("partner_distance_fee_v3")]
        public object PartnerDistanceFeeV3 { get; set; } = null!;

        [JsonPropertyName("cod_commission")]
        public CodCommission CodCommission { get; set; } = null!;

        [JsonPropertyName("pending_period")]
        public int? PendingPeriod { get; set; }

        [JsonPropertyName("need_confirm_before_pickup")]
        public bool? NeedConfirmBeforePickup { get; set; }

        [JsonPropertyName("delay_time")]
        public int? DelayTime { get; set; }

        [JsonPropertyName("limit_call")]
        public bool? LimitCall { get; set; }

        [JsonPropertyName("RouteAPIUrl")]
        public string RouteAPIUrl { get; set; } = null!;

        [JsonPropertyName("is_saas")]
        public bool? IsSaas { get; set; }

        [JsonPropertyName("group_name_en_us")]
        public string GroupNameEnUs { get; set; } = null!;

        [JsonPropertyName("group_name_vi_vn")]
        public string GroupNameViVn { get; set; } = null!;

        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = null!;

        [JsonPropertyName("enable_platform")]
        public List<string> EnablePlatform { get; set; } = null!;

        [JsonPropertyName("StopFailed")]
        public int? StopFailed { get; set; }

        [JsonPropertyName("RebroadcastLimit")]
        public int? RebroadcastLimit { get; set; }

        [JsonPropertyName("cross_city")]
        public CrossCity CrossCity { get; set; } = null!;

        [JsonPropertyName("available_districts")]
        public List<string> AvailableDistricts { get; set; } = null!;

        [JsonPropertyName("max_cod")]
        public decimal? MaxCod { get; set; }

        [JsonPropertyName("saas_distance_fee_v3")]
        public object SaasDistanceFeeV3 { get; set; } = null!;

        [JsonPropertyName("saas_stop_fee")]
        public decimal? SaasStopFee { get; set; }

        [JsonPropertyName("vat_rate")]
        public double? VatRate { get; set; }

        [JsonPropertyName("priority_score")]
        public int? PriorityScore { get; set; }

        [JsonPropertyName("is_pay_cod_discrepancy")]
        public bool? IsPayCodDiscrepancy { get; set; }

        [JsonPropertyName("premium_service_fee")]
        public decimal? PremiumServiceFee { get; set; }

        [JsonPropertyName("require_pof")]
        public bool? RequirePof { get; set; }

        [JsonPropertyName("pof_type")]
        public string PofType { get; set; } = null!;

        [JsonPropertyName("enable_upload_pof")]
        public bool? EnableUploadPof { get; set; }

        [JsonPropertyName("require_pop")]
        public bool? RequirePop { get; set; }

        [JsonPropertyName("pop_type")]
        public string PopType { get; set; } = null!;

        [JsonPropertyName("require_pod")]
        public bool? RequirePod { get; set; }

        [JsonPropertyName("pod_type")]
        public string PodType { get; set; } = null!;
 
        [JsonPropertyName("hour_fee")]
        public object HourFee { get; set; } = null!;

        [JsonPropertyName("boundary_restricted")]
        public BoundaryRestricted BoundaryRestricted { get; set; } = null!;

        // ✅ FIX: Đổi từ JsonArray → List<SpecialRequest> để deserialize đúng
        [JsonPropertyName("requests")]
        public List<SpecialRequest> Requests { get; set; } = null!;

        // ✅ THÊM MỚI: Có trong JSON nhưng class cũ thiếu
        [JsonPropertyName("vehicle_info")]
        public VehicleInfo VehicleInfo { get; set; } = null!;

        [JsonPropertyName("weight_fee")]
        public object WeightFee { get; set; } = null!;

        [JsonPropertyName("partner_weight_fee")]
        public object PartnerWeightFee { get; set; } = null!;

        [JsonPropertyName("map_icon_3d")]
        public MapIcon3d MapIcon3d { get; set; } = null!;

        [JsonPropertyName("is_tip_allowed")]
        public bool? IsTipAllowed { get; set; }

        [JsonPropertyName("is_select_vehicle_allowed")]
        public bool? IsSelectVehicleAllowed { get; set; }

        // key là "0"–"6" (ngày trong tuần), value là list surge
        [JsonPropertyName("surge_hours")]
        public Dictionary<string, List<SurgeHour>> SurgeHours { get; set; } = null!;
 
        [JsonPropertyName("partner_stop_fee")]
        public decimal? PartnerStopFee { get; set; }

        [JsonPropertyName("max_item_value")]
        public decimal? MaxItemValue { get; set; }

        // key là trạng thái đơn (vd: "ACCEPTED"), value là số giây
        [JsonPropertyName("user_cancellation_window_in_seconds")]
        public Dictionary<string, int> UserCancellationWindowInSeconds { get; set; } = null!;

        [JsonPropertyName("surge_rate_by_days_from_booking")]
        public object SurgeRateByDaysFromBooking { get; set; } = null!;
    }

    // =====================================================================
    // Nested classes
    // =====================================================================

    public class CrossService
    {
        [JsonPropertyName("enable")]
        public bool? Enable { get; set; }

        [JsonPropertyName("services")]
        public List<string> Services { get; set; } = null!;
    }

    public class CrossCity
    {
        [JsonPropertyName("enable")]
        public bool? Enable { get; set; }

        [JsonPropertyName("cities")]
        public List<string> Cities { get; set; } = null!;
    }

    public class DistanceFeeV3
    {
        [JsonPropertyName("blocks")]
        public List<FeeBlock> Blocks { get; set; } = null!;

        [JsonPropertyName("hour_rates")]
        public List<HourRate> HourRates { get; set; } = null!;
    }

    public class FeeBlock
    {
        [JsonPropertyName("from")]
        public double? From { get; set; }

        [JsonPropertyName("to")]
        public double? To { get; set; }

        [JsonPropertyName("base")]
        public decimal? Base { get; set; }

        [JsonPropertyName("price_multiplier")]
        public decimal? PriceMultiplier { get; set; }
    }

    public class HourRate
    {
        [JsonPropertyName("from")]
        public int? From { get; set; }

        [JsonPropertyName("to")]
        public int? To { get; set; }

        [JsonPropertyName("rate")]
        public double? Rate { get; set; }
    }

    public class CodCommission
    {
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [JsonPropertyName("none_commission_limit")]
        public decimal? NoneCommissionLimit { get; set; }

        // null hoặc array of CodCommissionBlock — dùng JsonArray cho an toàn
        [JsonPropertyName("blocks")]
        public JsonArray Blocks { get; set; } = null!;
    }

    public class CodCommissionBlock
    {
        [JsonPropertyName("from")]
        public decimal? From { get; set; }

        [JsonPropertyName("to")]
        public decimal? To { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [JsonPropertyName("min_fee")]
        public decimal? MinFee { get; set; }
    }

    public class BoundaryRestricted
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        // Mảng lồng nhau phức tạp — giữ JsonArray
        [JsonPropertyName("coordinates")]
        public JsonArray Coordinates { get; set; } = null!;
    }

    public class VehicleInfo
    {
        [JsonPropertyName("weight")]
        public int? Weight { get; set; }

        [JsonPropertyName("length")]
        public int? Length { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    public class MapIcon3d
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        [JsonPropertyName("sprite_size")]
        public int? SpriteSize { get; set; }

        [JsonPropertyName("sprite_count")]
        public int? SpriteCount { get; set; }

        [JsonPropertyName("original_icon_url")]
        public string OriginalIconUrl { get; set; } = null!;

        [JsonPropertyName("ads_icon_url")]
        public string AdsIconUrl { get; set; } = null!;

        [JsonPropertyName("ads_icon_size")]
        public int? AdsIconSize { get; set; }
    }

    public class SurgeHour
    {
        [JsonPropertyName("start_time")]
        public int? StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public int? EndTime { get; set; }

        [JsonPropertyName("surge_type")]
        public string SurgeType { get; set; } = null!;

        [JsonPropertyName("surge_value")]
        public decimal? SurgeValue { get; set; }
    }

    // =====================================================================
    // SpecialRequest — requests[] trong ServiceDetailsResponse
    // =====================================================================
    public class SpecialRequest
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("description_en_us")]
        public string DescriptionEnUs { get; set; } = null!;

        [JsonPropertyName("description_vi_vn")]
        public string DescriptionViVn { get; set; } = null!;

        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = null!;

        [JsonPropertyName("img_url")]
        public string ImgUrl { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("name_en_us")]
        public string NameEnUs { get; set; } = null!;

        [JsonPropertyName("name_vi_vn")]
        public string NameViVn { get; set; } = null!;

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("original_price")]
        public decimal OriginalPrice { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("service_id")]
        public string ServiceId { get; set; } = null!;

        // "BOOLEAN" | "TIER" | "COD_COMMISSION" | "COD_INSURANCE" | "INPUT" | "PER_UNIT" | "COMMISSION_DISTANCE_PERCENTAGE"
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("device_types")]
        public List<string> DeviceTypes { get; set; } = null!;

        [JsonPropertyName("service_group_id")]
        public string ServiceGroupId { get; set; } = null!;

        [JsonPropertyName("transfer_cod")]
        public bool TransferCod { get; set; }

        [JsonPropertyName("no_promotion")]
        public bool NoPromotion { get; set; }

        [JsonPropertyName("no_supplier_payout")]
        public bool NoSupplierPayout { get; set; }

        [JsonPropertyName("applied_stp_num")]
        public int? AppliedStpNum { get; set; }

        // null hoặc object tùy loại request — giữ object
        [JsonPropertyName("none_cod_price_blocks")] 
        public object NoneCodPriceBlocks { get; set; } = null!;

        [JsonPropertyName("subtype")]
        public string Subtype { get; set; } = null!;

        [JsonPropertyName("max_pod")]
        public decimal? MaxPod { get; set; }

        [JsonPropertyName("cod_commission")]
        public CodCommission CodCommission { get; set; } = null!;

        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = null!;

        // Chỉ có khi type = "TIER"
        [JsonPropertyName("tier_list")]
        public List<TierItem> TierList { get; set; } = null!;

        [JsonPropertyName("support_remarks")]
        public bool? SupportRemarks { get; set; }

        [JsonPropertyName("supplier_description_url")]
        public string SupplierDescriptionUrl { get; set; } = null!;

        [JsonPropertyName("no_commission")]
        public bool? NoCommission { get; set; }

        [JsonPropertyName("reimbursement")]
        public bool? Reimbursement { get; set; }

        [JsonPropertyName("max_input")]
        public int? MaxInput { get; set; }

        // Chỉ có ở SGN-BIKE-SMS
        [JsonPropertyName("send_sms")]
        public bool? SendSms { get; set; }

        [JsonPropertyName("sms_template_v3")]
        public string SmsTemplateV3 { get; set; } = null!;

        [JsonPropertyName("sms_demo")]
        public string SmsDemo { get; set; } = null!;

        // Chỉ có ở SGN-BIKE-INSURANCE
        [JsonPropertyName("price_blocks")]
        public List<InsurancePriceBlock> PriceBlocks { get; set; } = null!;

        [JsonPropertyName("description_url")]
        public string DescriptionUrl { get; set; } = null!;

        [JsonPropertyName("benefit_url")]
        public string BenefitUrl { get; set; } = null!;

        // Chỉ có ở SGN-BIKE-FRAGILE
        [JsonPropertyName("options")]
        public List<FragileOption> Options { get; set; } = null!;

        [JsonPropertyName("max_distance")]
        public long? MaxDistance { get; set; }

        // POD/POR fields
        [JsonPropertyName("require_pod")]
        public bool? RequirePod { get; set; }

        [JsonPropertyName("pod_type")]
        public string PodType { get; set; } = null!;

        [JsonPropertyName("require_por")]
        public bool? RequirePor { get; set; }

        [JsonPropertyName("por_type")]
        public string PorType { get; set; } = null!;

        [JsonPropertyName("delivery_option")]
        public bool? DeliveryOption { get; set; }
    }

    public class TierItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("name_vi_vn")]
        public string NameViVn { get; set; } = null!;

        [JsonPropertyName("name_en_us")]
        public string NameEnUs { get; set; } = null!;

        [JsonPropertyName("selectable")]
        public bool Selectable { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("price_description_vi_vn")]
        public string PriceDescriptionViVn { get; set; } = null!;

        [JsonPropertyName("price_description_en_us")]
        public string PriceDescriptionEnUs { get; set; } = null!;
 
        [JsonPropertyName("description_vi_vn")]
        public string DescriptionViVn { get; set; } = null!;

        [JsonPropertyName("description_en_us")]
        public string DescriptionEnUs { get; set; } = null!;

        [JsonPropertyName("img_url")]
        public string ImgUrl { get; set; } = null!;
    }

    public class InsurancePriceBlock
    {
        [JsonPropertyName("from")]
        public decimal? From { get; set; }

        [JsonPropertyName("to")]
        public decimal? To { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }

    public class FragileOption
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("name_vi_vn")]
        public string NameViVn { get; set; } = null!;

        [JsonPropertyName("name_en_us")]
        public string NameEnUs { get; set; } = null!;

        [JsonPropertyName("selectable")]
        public bool Selectable { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("short_name_vi_vn")]
        public string ShortNameViVn { get; set; } = null!;

        [JsonPropertyName("short_name_en_us")]
        public string ShortNameEnUs { get; set; } = null!;

        [JsonPropertyName("need_user_input")]
        public bool? NeedUserInput { get; set; }
    }















/// ////////////////////
public class ServiceResponse
{
    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public ServiceData Data { get; set; } = new();
}

public class ServiceData
{
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

    [JsonPropertyName("total_fee")]
    public decimal TotalFee { get; set; }

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }

    // Initialized as an empty list to avoid null reference issues on services without requests
    [JsonPropertyName("requests")]
    public List<ServiceRequest> Requests { get; set; } = new();
}

public class ServiceRequest
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("num")]
    public int Num { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("tier_code")]
    public string? TierCode { get; set; }
}