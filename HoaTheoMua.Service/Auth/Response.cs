namespace HoaTheoMua.Service.Auth;

public class Response
{
    public class VerifyEmailResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
    }

    public class AuthResponse : VerifyEmailResponse;
    
    public class GetMeResponse
    {
        public required String FirstName { get; set; }
        public required String LastName { get; set; }
        public required String Email { get; set; }
        public String? Avatar { get; set; }
    }

    public class LogoutResponse : VerifyEmailResponse;



    
    public class LoginResponse
    {
        public string? AccessToken { get; set; } 

        public string? RefreshToken { get; set; } 
        
        public string? Message { get; set; } 
        
    }
}