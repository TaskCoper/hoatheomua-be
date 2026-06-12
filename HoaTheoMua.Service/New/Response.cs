using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.New;

public class NewResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public string? PictureUrl { get; set; }
    public string Status { get; set; }
    public Guid UserId { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
