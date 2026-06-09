using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.Auth;

public class SecurityOptions
{
    [Required] public string Pepper { get; set; } = null;

}