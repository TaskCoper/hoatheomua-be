using System.Text.Json;

namespace HoaTheoMua.Service.Config;

public class Response
{
    public class ConfigResponse
    {
        public Guid Id { get; set; }
        public required string Key { get; set; }
        public JsonElement Value { get; set; }
        public  Guid UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } 
    }
}