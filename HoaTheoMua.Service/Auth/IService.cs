namespace HoaTheoMua.Service.Auth;

public interface IService
{
    public Task<string> Register(Request.RegisterRequest request);

    public Task<Response.AuthResponse> RefreshToken();
    public Task<Response.VerifyEmailResponse?> VerifyEmail(Request.VerifyEmailRequest request);
    public Task<Response.GetMeResponse> GetMe();
    public Task<Response.LogoutResponse> Logout();
}