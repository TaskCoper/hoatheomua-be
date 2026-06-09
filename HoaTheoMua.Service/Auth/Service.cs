using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
//using HoaTheoMua.Service.Auth.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using JwtService = HoaTheoMua.Service.JwtService;

namespace HoaTheoMua.Service.Auth;

public class Service : IService
{
    private const int RefreshTokenExpireDays = 7;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private readonly JwtService.IService _jwtService;
    private readonly string _pepper;

    public Service(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context,
        JwtService.IService jwtService,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _jwtService = jwtService;
        _pepper = configuration["SecurityOptions:Pepper"] ?? string.Empty;
    }

    public async Task<Response> LoginAsync(
        Request request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown Device";
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email && !x.IsDeleted);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
        }

        var isPasswordValid = VerifyPassword(request.Password, user.HashPassword);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
        }

        if (!user.IsVerify)
        {
           throw new UnauthorizedAccessException("Vui lòng xác thực email trước khi đăng nhập");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // new Claim(ClaimTypes.Email, user.Email),
            // new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            // new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("isVerify", user.IsVerify.ToString())
        };

        var accessToken = _jwtService.GenerateAccessToken(claims);

        var refreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenHash = HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            RefreshTokenHash = refreshTokenHash,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceLabel = "",
            IsActive = true,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpireDays),
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return Response.Ok(accessToken, refreshToken);
    }

    private bool VerifyPassword(string password, string hashPassword)
    {
        try
        {
            if (BCrypt.Net.BCrypt.Verify(password, hashPassword))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(_pepper) &&
                BCrypt.Net.BCrypt.Verify(password + _pepper, hashPassword))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(bytes);
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