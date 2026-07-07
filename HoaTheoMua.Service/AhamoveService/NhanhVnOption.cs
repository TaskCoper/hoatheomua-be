using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.AhaMoveService;

public class NhanhVnOption
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required] public int AppId { get; set; } 
    [Required] public int BusinessId { get; set; } 
    [Required] public string BaseUrl { get; set; } = string.Empty;

}