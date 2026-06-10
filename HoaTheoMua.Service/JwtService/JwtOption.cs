using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.JwtService;

public class JwtOption
{
    [Required]public string Issuer { get; set; }
    [Required]public string Audience { get; set; }
    [Required]public string AccessTokenKey { get; set; }
    [Required]public int AccessTokenExpireMin { get; set; }
    [Required]public string RefreshTokenKey { get; set; }
    [Required]public int RefreshTokenExpireMin { get; set; }
}
