using System.Security.Claims;

namespace HoaTheoMua.Service.JwtService;

public interface IService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public string GenerateRefreshToken();
    public string GenerateEmailVerificationToken(IEnumerable<Claim> claims, double expiration);
    ClaimsPrincipal ValidateToken(string token);
}