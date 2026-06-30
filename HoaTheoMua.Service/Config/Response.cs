using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HoaTheoMua.Service.Config;

public class Response
{
    public class ConfigResponse
    {
        public Guid Id { get; set; }
        public required string Key { get; set; }
        public string? Group { get; set; } = null!;
        public required bool IsPublic { get; set; }
        public required JsonElement Value { get; set; }
        public string? Kind { get; set; }
        public  Guid UserId { get; set; }
    }

    public class ConfigGroupResponse
    {
        public required string Key { get; set; }
        public required string Kind { get; set; }
        public required JsonElement Value { get; set; }
    }
    
    // public class ConfigResponseContent
    // {
    //     public Guid Id { get; set; }
    //     public required string Key { get; set; }
    //     public string? Group { get; set; } = null!;
    //     public required bool IsPublic { get; set; }
    //     public required JsonElement Value { get; set; }
    //     public string? Kind { get; set; }
    //     public  Guid UserId { get; set; }
    // }
    
    public class RevealResponse
    {
        public Guid Id { get; set; }
        public required string Key {get; set;}
        public required JsonElement Value { get; set; }
        public DateTimeOffset RevealedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
    
    public class ConfigItem
    {
        [JsonPropertyName("group")]
        public string? Group { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("value")]
        public List<int>? Value { get; set; }

        [JsonPropertyName("options")]
        public List<Option>? Options { get; set; }

        [JsonPropertyName("required")]
        public bool? Required { get; set; } = true;

        [JsonPropertyName("inputType")]
        public string? InputType { get; set; } = "multiSelect";

        [JsonPropertyName("validations")]
        public List<int>? Validations { get; set; } = new();
    }

    public class Option
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("value")]
        public int? Value { get; set; }
    }
    
}