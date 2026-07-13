using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.JwtService;

// This service only VALIDATES tokens issued by taskcoper-be — it no longer issues them.
// Validation needs Issuer, Audience and the shared access-token signing key only.
public class JwtOption
{
    [Required]public string Issuer { get; set; } = null!;
    [Required]public string Audience { get; set; } = null!;
    [Required]public string AccessTokenKey { get; set; } = null!;
}
