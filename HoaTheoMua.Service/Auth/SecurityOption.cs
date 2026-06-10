using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.Auth;

public class SecurityOption
{
    [Required]public string Pepper { get; set; } = null!;

}
