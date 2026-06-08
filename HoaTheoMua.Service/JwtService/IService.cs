using System.Security.Claims;

namespace HoaTheoMua.Service.JwtService;

public interface IService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public string GenerateRefreshToken();
    ClaimsPrincipal ValidateToken(string token);
}