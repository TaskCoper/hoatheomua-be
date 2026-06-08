using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.JwtService;
using HoaTheoMua.Service.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.AuthService;

public class AuthService : IAuthService
{
    private const int RefreshTokenExpireDays = 30;

    private readonly AppDbContext _context;
    private readonly ILoginJwtService _loginJwtService;

    public AuthService(
        AppDbContext context,
        ILoginJwtService loginJwtService)
    {
        _context = context;
        _loginJwtService = loginJwtService;
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (user == null)
        {
            return LoginResult.Fail(401, "Email hoặc mật khẩu không đúng");
        }

        var isPasswordValid = VerifyPassword(request.Password, user.HashPassword);

        if (!isPasswordValid)
        {
            return LoginResult.Fail(401, "Email hoặc mật khẩu không đúng");
        }

        if (!user.IsVerify)
        {
            return LoginResult.Fail(403, "Vui lòng xác thực email trước khi đăng nhập");
        }

        var accessToken = _loginJwtService.GenerateAccessToken(user);

        var refreshToken = _loginJwtService.GenerateRefreshToken();

        var refreshTokenHash = _loginJwtService.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            RefreshTokenHash = refreshTokenHash,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceLabel = GetDeviceLabel(userAgent),
            IsActive = true,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpireDays),
            UserId = user.Id
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return LoginResult.Ok(accessToken, refreshToken);
    }

    private static bool VerifyPassword(string password, string hashPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }
        catch
        {
            return false;
        }
    }

    private static string GetDeviceLabel(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "Unknown";
        }

        var lowerUserAgent = userAgent.ToLower();

        if (lowerUserAgent.Contains("mobile") ||
            lowerUserAgent.Contains("android") ||
            lowerUserAgent.Contains("iphone"))
        {
            return "Mobile";
        }

        if (lowerUserAgent.Contains("ipad") ||
            lowerUserAgent.Contains("tablet"))
        {
            return "Tablet";
        }

        return "Desktop";
    }
}