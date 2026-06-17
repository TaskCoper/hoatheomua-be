using System.Text.Json;

namespace HoaTheoMua.Service.Config;

public class Response
{
    public class ConfigResponse
    {
        public Guid Id { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
        public  Guid UserId { get; set; }
    }
}