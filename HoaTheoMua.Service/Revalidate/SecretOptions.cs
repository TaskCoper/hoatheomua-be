using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.Revalidate;

public class SecretOptions
{
    [Required] 
    public string SecretKey { get; set; } = null!;

    [Required] 
    public string BaseUrl { get; set; } = null!;
}