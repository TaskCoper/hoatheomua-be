using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.Fulfillment;

public class SystemOption
{
    [Required]
    public string ClientUrl { get; set; }
}