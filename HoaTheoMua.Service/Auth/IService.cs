namespace HoaTheoMua.Service.Auth;

public interface IService
{
    public Task<string> Register(Request.RegisterRequest request);
    public Task<Response.VerifyEmailResponse> VerifyEmail(Request.VerifyEmailRequest request);
}