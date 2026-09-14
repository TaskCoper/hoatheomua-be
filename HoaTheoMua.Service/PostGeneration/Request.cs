using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.PostGeneration;

public class Request
{
    public class GenerateRequest
    {
        public string Mode { get; set; } = "all";
        public List<PlatformInput>? Platforms { get; set; }
    }
    public class RatioInput
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AspectRatioType? RatioCode { get; set; }
        public List<Guid>? BasePostImageIds { get; set; }
    }
    public class PlatformInput
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlatformType? Platform { get; set; }
        public bool UsePreset { get; set; }
        public List<RatioInput>? Ratios { get; set; }
    }
}
