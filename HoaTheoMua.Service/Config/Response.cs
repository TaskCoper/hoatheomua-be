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
        public  Guid UserId { get; set; }
    }
}