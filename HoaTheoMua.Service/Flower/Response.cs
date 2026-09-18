using System.Text.Json;

namespace HoaTheoMua.Service.Flower;

public class Response
{
    public class GeneratedFlowerResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string ImageUrl { get; init; } = null!;
        public JsonElement Metadata { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
