using System.ComponentModel.DataAnnotations;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.New;

public class CreateNewRequest : UpdateNewRequest;


public class UpdateNewRequest
{
    [Required]
    public required string Title { get; set; } = string.Empty;

    [Required]
    public required string Description { get; set; } = string.Empty;
    
    public string? PictureUrl { get; set; }

    [Required]
    public required NewsStatus Status { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> CategoryIds { get; set; } = new();
}
