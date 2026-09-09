namespace HoaTheoMua.Repository.Entity;

public class GeneratedPost
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
