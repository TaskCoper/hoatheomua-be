using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.XanhSm;

public class XanhSmOptions
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string TokenUrl { get; set; } = string.Empty;

    public string? Scope { get; set; }

    [Range(1, int.MaxValue)]
    public int DoorToDoorAddonId { get; set; }

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    public string AcceptLanguage { get; set; } = "vi";
}
