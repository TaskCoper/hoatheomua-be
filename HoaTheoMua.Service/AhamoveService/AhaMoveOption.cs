using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.AhamoveService;

public class AhaMoveOption
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required] public string BaseUrl { get; set; } = string.Empty;
}