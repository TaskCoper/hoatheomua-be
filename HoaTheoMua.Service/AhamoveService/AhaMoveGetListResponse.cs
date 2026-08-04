using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.AhamoveService;

public class AhaMoveGetListResponse
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accept_interval")]
    public int? AcceptInterval { get; set; }

    [JsonPropertyName("advance_broadcast_distance")]
    public int? AdvanceBroadcastDistance { get; set; }

    [JsonPropertyName("auto_assign")]
    public bool AutoAssign { get; set; }

    [JsonPropertyName("auto_assign_distance")]
    public int? AutoAssignDistance { get; set; }

    [JsonPropertyName("badge")]
    public AhaMoveBadge? Badge { get; set; }

    [JsonPropertyName("city_id")]
    public string CityId { get; set; } = string.Empty;

    [JsonPropertyName("cod")]
    public decimal Cod { get; set; }

    [JsonPropertyName("commission_type")]
    public string CommissionType { get; set; } = string.Empty;

    [JsonPropertyName("commission_value")]
    public decimal CommissionValue { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("delivery_type")]
    public string DeliveryType { get; set; } = string.Empty;

    [JsonPropertyName("description_en_us")]
    public string? DescriptionEnUs { get; set; }

    [JsonPropertyName("description_vi_vn")]
    public string? DescriptionViVn { get; set; }

    [JsonPropertyName("distance_fee")]
    public string DistanceFee { get; set; } = string.Empty;

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("enable_platform")]
    public List<string> EnablePlatform { get; set; } = new();

    [JsonPropertyName("fee_description_en_us")]
    public string FeeDescriptionEnUs { get; set; } = string.Empty;

    [JsonPropertyName("fee_description_vi_vn")]
    public string? FeeDescriptionViVn { get; set; }

    [JsonPropertyName("first_order_cod")]
    public decimal FirstOrderCod { get; set; }

    [JsonPropertyName("group_id")]
    public string GroupId { get; set; } = string.Empty;

    [JsonPropertyName("group_name_en_us")]
    public string? GroupNameEnUs { get; set; }

    [JsonPropertyName("group_name_vi_vn")]
    public string? GroupNameViVn { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; } = string.Empty;

    [JsonPropertyName("idle_timeout")]
    public int IdleTimeout { get; set; }

    [JsonPropertyName("is_saas")]
    public bool IsSaas { get; set; }

    [JsonPropertyName("is_select_vehicle_allowed")]
    public bool IsSelectVehicleAllowed { get; set; }

    [JsonPropertyName("is_tip_allowed")]
    public bool IsTipAllowed { get; set; }

    [JsonPropertyName("map_icon_3d")]
    public AhaMoveMapIcon3D MapIcon3D { get; set; } = new();

    [JsonPropertyName("map_icon_url")]
    public string MapIconUrl { get; set; } = string.Empty;

    [JsonPropertyName("max_cod")]
    public decimal MaxCod { get; set; }

    [JsonPropertyName("max_cod_per_stop_point")]
    public decimal? MaxCodPerStopPoint { get; set; }

    [JsonPropertyName("max_concurrent_orders")]
    public int MaxConcurrentOrders { get; set; }

    [JsonPropertyName("max_distance")]
    public long MaxDistance { get; set; }

    [JsonPropertyName("max_stop_points")]
    public int MaxStopPoints { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; } = string.Empty;

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("parent_id")]
    public string ParentId { get; set; } = string.Empty;

    [JsonPropertyName("partner")]
    public bool? Partner { get; set; }

    [JsonPropertyName("partner_distance_fee")]
    public string PartnerDistanceFee { get; set; } = string.Empty;

    [JsonPropertyName("pool_timeout_commission_value")]
    public decimal PoolTimeoutCommissionValue { get; set; }

    [JsonPropertyName("requests")]
    public List<AhaMoveSpecialRequest> Requests { get; set; } = new();

    [JsonPropertyName("require_request")]
    public bool RequireRequest { get; set; }

    [JsonPropertyName("require_to")]
    public bool RequireTo { get; set; }

    [JsonPropertyName("stop_fee")]
    public decimal StopFee { get; set; }

    [JsonPropertyName("supplier_description")]
    public string? SupplierDescription { get; set; }

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    [JsonPropertyName("vat_rate")]
    public decimal? VatRate { get; set; }

    [JsonPropertyName("vehicle_info")]
    public AhaMoveVehicleInfo? VehicleInfo { get; set; }

    // Giữ lại field mới nếu Ahamove bổ sung response sau này.
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveBadge
{
    [JsonPropertyName("background_color")]
    public string BackgroundColor { get; set; } = string.Empty;

    [JsonPropertyName("description_en_us")]
    public string DescriptionEnUs { get; set; } = string.Empty;

    [JsonPropertyName("description_vi_vn")]
    public string DescriptionViVn { get; set; } = string.Empty;

    [JsonPropertyName("text_color")]
    public string TextColor { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveMapIcon3D
{
    [JsonPropertyName("ads_icon_size")]
    public int AdsIconSize { get; set; }

    [JsonPropertyName("original_icon_url")]
    public string OriginalIconUrl { get; set; } = string.Empty;

    [JsonPropertyName("sprite_count")]
    public int SpriteCount { get; set; }

    [JsonPropertyName("sprite_size")]
    public int SpriteSize { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveVehicleInfo
{
    [JsonPropertyName("height")]
    public decimal Height { get; set; }

    [JsonPropertyName("length")]
    public decimal Length { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [JsonPropertyName("width")]
    public decimal Width { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveSpecialRequest
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("applied_stp_num")]
    public int? AppliedStopPointNumber { get; set; }

    [JsonPropertyName("benefit_url")]
    public string? BenefitUrl { get; set; }

    [JsonPropertyName("cod_commission")]
    public AhaMoveCodCommission? CodCommission { get; set; }

    [JsonPropertyName("delivery_option")]
    public bool? DeliveryOption { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("description_en_us")]
    public string? DescriptionEnUs { get; set; }

    [JsonPropertyName("description_url")]
    public string? DescriptionUrl { get; set; }

    [JsonPropertyName("description_vi_vn")]
    public string? DescriptionViVn { get; set; }

    [JsonPropertyName("device_types")]
    public List<string> DeviceTypes { get; set; } = new();

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("group_id")]
    public string? GroupId { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; } = string.Empty;

    [JsonPropertyName("img_url")]
    public string ImgUrl { get; set; } = string.Empty;

    [JsonPropertyName("max_distance")]
    public long? MaxDistance { get; set; }

    [JsonPropertyName("max_input")]
    public int? MaxInput { get; set; }

    [JsonPropertyName("max_item_value")]
    public decimal? MaxItemValue { get; set; }

    [JsonPropertyName("max_pod")]
    public decimal? MaxPod { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("name_en_us")]
    public string? NameEnUs { get; set; }

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; } = string.Empty;

    [JsonPropertyName("no_commission")]
    public bool? NoCommission { get; set; }

    [JsonPropertyName("no_promotion")]
    public bool NoPromotion { get; set; }

    [JsonPropertyName("no_supplier_payout")]
    public bool NoSupplierPayout { get; set; }

    [JsonPropertyName("none_cod_price_blocks")]
    public AhaMoveNoneCodPriceBlocks? NoneCodPriceBlocks { get; set; }

    [JsonPropertyName("options")]
    public List<AhaMoveRequestOption> Options { get; set; } = new();

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("original_price")]
    public decimal OriginalPrice { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("price_blocks")]
    public List<AhaMovePriceBlock> PriceBlocks { get; set; } = new();

    [JsonPropertyName("request_group")]
    public AhaMoveRequestGroup? RequestGroup { get; set; }

    [JsonPropertyName("required_item_value")]
    public bool? RequiredItemValue { get; set; }

    [JsonPropertyName("send_sms")]
    public bool? SendSms { get; set; }

    [JsonPropertyName("service_group_id")]
    public string? ServiceGroupId { get; set; }

    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [JsonPropertyName("sms_demo")]
    public string? SmsDemo { get; set; }

    [JsonPropertyName("sms_template_v3")]
    public string? SmsTemplateV3 { get; set; }

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    [JsonPropertyName("supplier_description_url")]
    public string? SupplierDescriptionUrl { get; set; }

    [JsonPropertyName("support_remarks")]
    public bool? SupportRemarks { get; set; }

    [JsonPropertyName("tier_list")]
    public List<AhaMoveTierItem> TierList { get; set; } = new();

    [JsonPropertyName("transfer_cod")]
    public bool TransferCod { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveTierItem
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("description_en_us")]
    public string DescriptionEnUs { get; set; } = string.Empty;

    [JsonPropertyName("description_vi_vn")]
    public string DescriptionViVn { get; set; } = string.Empty;

    [JsonPropertyName("img_url")]
    public string ImgUrl { get; set; } = string.Empty;

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; } = string.Empty;

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("price_description_en_us")]
    public string PriceDescriptionEnUs { get; set; } = string.Empty;

    [JsonPropertyName("price_description_vi_vn")]
    public string PriceDescriptionViVn { get; set; } = string.Empty;

    [JsonPropertyName("selectable")]
    public bool Selectable { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveRequestGroup
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("description_en_us")]
    public string DescriptionEnUs { get; set; } = string.Empty;

    [JsonPropertyName("description_vi_vn")]
    public string DescriptionViVn { get; set; } = string.Empty;

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; } = string.Empty;

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; } = string.Empty;

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveRequestOption
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name_en_us")]
    public string NameEnUs { get; set; } = string.Empty;

    [JsonPropertyName("name_vi_vn")]
    public string NameViVn { get; set; } = string.Empty;

    [JsonPropertyName("need_user_input")]
    public bool? NeedUserInput { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("selectable")]
    public bool Selectable { get; set; }

    [JsonPropertyName("short_name_en_us")]
    public string ShortNameEnUs { get; set; } = string.Empty;

    [JsonPropertyName("short_name_vi_vn")]
    public string ShortNameViVn { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveCodCommission
{
    [JsonPropertyName("blocks")]
    public List<AhaMoveCodCommissionBlock> Blocks { get; set; } = new();

    [JsonPropertyName("none_commission_limit")]
    public decimal NoneCommissionLimit { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveCodCommissionBlock
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMovePriceBlock
{
    [JsonPropertyName("from")]
    public decimal From { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("to")]
    public decimal To { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class AhaMoveNoneCodPriceBlocks
{
    [JsonPropertyName("drink")]
    public decimal Drink { get; set; }

    [JsonPropertyName("electronics")]
    public decimal Electronics { get; set; }

    [JsonPropertyName("fashion")]
    public decimal Fashion { get; set; }

    [JsonPropertyName("food")]
    public decimal Food { get; set; }

    [JsonPropertyName("grocery")]
    public decimal Grocery { get; set; }

    [JsonPropertyName("health & beauty")]
    public decimal HealthAndBeauty { get; set; }

    [JsonPropertyName("housing & lifestyle")]
    public decimal HousingAndLifestyle { get; set; }

    [JsonPropertyName("individual needs")]
    public decimal IndividualNeeds { get; set; }

    [JsonPropertyName("mother & baby")]
    public decimal MotherAndBaby { get; set; }

    [JsonPropertyName("other")]
    public decimal Other { get; set; }

    [JsonPropertyName("restaurant")]
    public decimal Restaurant { get; set; }

    [JsonPropertyName("sport")]
    public decimal Sport { get; set; }

    [JsonPropertyName("transportation")]
    public decimal Transportation { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}