using System.Text.Json;

namespace HoaTheoMua.Service.Config;

public class Request
{
    public class CreateConfigRequest
    {
        public required string Key { get; set; }
        public required JsonElement Value { get; set; }
        
    }
    public class UpdateConfigRequest
    {
        public bool? IsPublic { get; set; } = false;
        public required JsonElement Value { get; set; }
    }
    
}