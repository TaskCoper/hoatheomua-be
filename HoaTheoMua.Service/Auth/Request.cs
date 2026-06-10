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
        public required string Email { get; set; } 

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        
        public required string Password { get; set; } 
    }
}