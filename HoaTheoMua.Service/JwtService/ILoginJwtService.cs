using AppUser = HoaTheoMua.Repository.Entity.User;

namespace HoaTheoMua.Service.JwtService;

public interface ILoginJwtService
{
    string GenerateAccessToken(AppUser user);

    string GenerateRefreshToken();

    string HashToken(string token);
}