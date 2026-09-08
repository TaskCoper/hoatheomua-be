namespace HoaTheoMua.Repository.Entity;

public class BasePost
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
}
