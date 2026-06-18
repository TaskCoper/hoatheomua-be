using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.New;

public class CategoryInfo
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
}

public class NewResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public string? PictureUrl { get; set; }
    public string? slug { get; set; }
    public string Status { get; set; }
    public Guid UserId { get; set; }
    public required string UserFirstName  { get; set; } = string.Empty;
    public required string UserLastName { get; set; } = string.Empty;
    public List<Guid> CategoryIds { get; set; } = new();
    public List<string> CategoryName { get; set; } = new();
    public List<CategoryInfo> Categories { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class NewsResponseV2
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public string? PictureUrl { get; set; }
    public string? slug { get; set; }
    public string Status { get; set; }
    public Guid UserId { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
    public List<CategoryInfo> Categories { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}