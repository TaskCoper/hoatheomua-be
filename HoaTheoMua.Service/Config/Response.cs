using System.Text.Json;

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
        public string? Group { get; set; }
        public string? Label { get; set; }
        public List<int>? Value { get; set; }
        public List<Option>? Options { get; set; }
        public bool? Required { get; set; } = true;
        public string? InputType { get; set; } = "multiSelect";
        public List<int>? Validations  { get; set; } = new();
     }

    public class Option
    {
        public string? Label { get; set; }
        public int? Value { get; set; }
    }
    
}