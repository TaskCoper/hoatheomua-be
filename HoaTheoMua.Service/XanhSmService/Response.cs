using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.XanhSmService;

public class GetAvailableServiceResponse
{

    public class ServiceResponse
    {
        [JsonPropertyName("data")]
        public List<ServiceData> Data { get; set; } = [];
    }

    public class ServiceData
    {
        [JsonPropertyName("service")]
        public Service Service { get; set; } = default!;

        [JsonPropertyName("service_hours")]
        public List<ServiceHour> ServiceHours { get; set; } = [];

        [JsonPropertyName("service_group")]
        public ServiceGroup ServiceGroup { get; set; } = default!;
    }

    public class Service
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ordering")]
        public int Ordering { get; set; }

        [JsonPropertyName("available_pickup_areas")]
        public List<Area> AvailablePickupAreas { get; set; } = [];

        [JsonPropertyName("available_dropoff_areas")]
        public List<Area> AvailableDropoffAreas { get; set; } = [];

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("service_type")]
        public string ServiceType { get; set; } = string.Empty;

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;

        [JsonPropertyName("available_areas")]
        public List<Area> AvailableAreas { get; set; } = [];

        [JsonPropertyName("max_extra_stop")]
        public int MaxExtraStop { get; set; }

        [JsonPropertyName("require_destination")]
        public bool RequireDestination { get; set; }

        [JsonPropertyName("auto_cancel_interval")]
        public int AutoCancelInterval { get; set; }

        [JsonPropertyName("map_icon_3d")]
        public MapIcon3D MapIcon3D { get; set; } = default!;

        [JsonPropertyName("max_distance")]
        public int MaxDistance { get; set; }

        [JsonPropertyName("min_distance")]
        public int MinDistance { get; set; }

        [JsonPropertyName("scheduling_enabled")]
        public bool SchedulingEnabled { get; set; }

        [JsonPropertyName("scheduling_interval")]
        public int SchedulingInterval { get; set; }

        [JsonPropertyName("scheduling_min_max")]
        public List<int> SchedulingMinMax { get; set; } = [];

        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        [JsonPropertyName("service_group")]
        public int ServiceGroup { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; } = string.Empty;

        [JsonPropertyName("max_passenger")]
        public int MaxPassenger { get; set; }

        [JsonPropertyName("travel_mode")]
        public string TravelMode { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_models")]
        public List<string> VehicleModels { get; set; } = [];

        [JsonPropertyName("country_id")]
        public int CountryId { get; set; }

        [JsonPropertyName("re_dispatch_counts")]
        public int ReDispatchCounts { get; set; }

        [JsonPropertyName("time_before_lock_driver")]
        public int TimeBeforeLockDriver { get; set; }

        // API trả về string Unix timestamp
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;

        [JsonPropertyName("sources")]
        public List<string> Sources { get; set; } = [];

        [JsonPropertyName("now_order_config")]
        public NowOrderConfig NowOrderConfig { get; set; } = default!;

        [JsonPropertyName("allow_surge_pricing")]
        public bool AllowSurgePricing { get; set; }

        [JsonPropertyName("need_dispatch_flow")]
        public bool NeedDispatchFlow { get; set; }

        [JsonPropertyName("pricing_mode")]
        public string PricingMode { get; set; } = string.Empty;

        [JsonPropertyName("express_package_types")]
        public List<string> ExpressPackageTypes { get; set; } = [];

        [JsonPropertyName("express_package_sizes")]
        public List<string> ExpressPackageSizes { get; set; } = [];

        [JsonPropertyName("express_package_max_weight")]
        public int ExpressPackageMaxWeight { get; set; }

        [JsonPropertyName("express_package_weight_unit")]
        public string ExpressPackageWeightUnit { get; set; } = string.Empty;

        [JsonPropertyName("express_pickup_sla")]
        public int ExpressPickupSla { get; set; }

        [JsonPropertyName("express_dropoff_sla")]
        public int ExpressDropoffSla { get; set; }

        [JsonPropertyName("express_optimize_sla")]
        public int ExpressOptimizeSla { get; set; }

        [JsonPropertyName("vertical_name")]
        public string VerticalName { get; set; } = string.Empty;

        [JsonPropertyName("sub_vertical_name")]
        public string SubVerticalName { get; set; } = string.Empty;

        [JsonPropertyName("during_service")]
        public bool DuringService { get; set; }
    }

    public class Area
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("country_id")]
        public int CountryId { get; set; }
    }

    public class MapIcon3D
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class NowOrderConfig
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        [JsonPropertyName("maximum_distance")]
        public int MaximumDistance { get; set; }

        [JsonPropertyName("maximum_order")]
        public int MaximumOrder { get; set; }
    }

    public class ServiceHour
    {
        [JsonPropertyName("day_of_week")]
        public int DayOfWeek { get; set; }

        [JsonPropertyName("start_time")]
        public int StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public int EndTime { get; set; }
    }

    public class ServiceGroup
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ordering")]
        public int Ordering { get; set; }
    }
}