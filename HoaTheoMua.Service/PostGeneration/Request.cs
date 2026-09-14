using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.PostGeneration;

public class Request
{
    public class GenerateRequest
    {
        public List<PlatformInput>? Platforms { get; set; }
    }
    public class PlatformInput
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlatformType? Platform { get; set; }
        public bool UsePreset { get; set; }
        public JsonElement? Ratios { get; set; }
    }
}
