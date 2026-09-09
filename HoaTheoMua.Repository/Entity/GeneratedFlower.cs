namespace HoaTheoMua.Repository.Entity;

public class GeneratedFlower
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
