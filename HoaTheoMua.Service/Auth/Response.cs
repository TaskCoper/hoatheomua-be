namespace HoaTheoMua.Service.Auth;

public class Response
{
    public class VerifyEmailResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
    }
    
}