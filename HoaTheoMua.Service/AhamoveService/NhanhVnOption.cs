using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.AhamoveService;

public class NhanhVnOption
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required] public string AppId { get; set; } = string.Empty;
    [Required] public string BusinessId { get; set; } = string.Empty;
    [Required] public string BaseUrl { get; set; } = string.Empty;

}