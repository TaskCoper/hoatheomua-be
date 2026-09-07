namespace HoaTheoMua.Repository.Entity;

public class GeneratedFlower
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public Guid UserId { get; set; }
    public string InputSnapshot { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
