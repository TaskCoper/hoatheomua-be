using HoaTheoMua.Service.Models.Auth;

namespace HoaTheoMua.Service.AuthService;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent
    );
}