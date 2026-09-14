using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.PostGeneration;

public class Request
{
    public class RegenerateContentRequest
    {
        public Guid? SourceGenerationId { get; set; }
        public string? Feedback { get; set; }
    }
    public class EditContentRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Hashtag { get; set; }
    }
    public class SelectContentRequest
    {
        public Guid GenerationId { get; set; }
    }
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
