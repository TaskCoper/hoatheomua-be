namespace HoaTheoMua.Repository.Entity;

public class GeneratedFlower
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? FlowerWithMockupImageUrl { get; set; }
    public string? FlowerWithLogoImageUrl { get; set; }
    public string Metadata { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
