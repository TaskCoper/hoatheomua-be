using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.AhaMoveService;

public class AhamoveOption
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required] public string BaseUrl { get; set; } = string.Empty;
}