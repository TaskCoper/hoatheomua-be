using System.ComponentModel.DataAnnotations;


namespace HoaTheoMua.Service.Auth;

public class Request
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 128 ký tự")]
    public string Password { get; set; } = string.Empty;
}