using System.Text.Json;

namespace HoaTheoMua.Service.Config;

public class Request
{
    public class CreateConfigRequest
    {
        public required string Key { get; set; }
        public required string Group { get; set; }
        public required string Kind { get; set; }
        public required JsonElement Value { get; set; }
        
    }
    public class UpdateConfigRequest
    {
        public bool? IsPublic { get; set; } = false;
        public required string Key { get; set; } 
        public required string Group { get; set; }
        public required string Kind { get; set; }
        public required JsonElement Value { get; set; }
    }

    public class UpdateDeliverySlotLimitsRequest
    {
        public required List<DeliverySlotLimitInput> Slots { get; set; }
    }

    public class DeliverySlotLimitInput
    {
        public int TagId { get; set; }
        public int Limit { get; set; }
    }
    
}
