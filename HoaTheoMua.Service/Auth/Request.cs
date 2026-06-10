using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.Auth;

public class Request
{
    public class RegisterRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
    public class VerifyEmailRequest
    {
        public required string Token { get; set; }
        
    }
    
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        // [StringLength(128, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 128 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}