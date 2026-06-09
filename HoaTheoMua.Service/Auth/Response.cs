namespace HoaTheoMua.Service.Auth;

public class Response
{
    public class VerifyEmailResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class AuthResponse : VerifyEmailResponse;
}