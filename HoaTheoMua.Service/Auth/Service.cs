using System.Security.Claims;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.MailService;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Auth;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly SecurityOption _securityOptions = new();
    private readonly JwtService.IService _jwtService;
    private readonly MailService.IService _mailService;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IConfiguration configuration,
        MailService.IService mailService, IHttpContextAccessor httpContextAccessor,
        JwtService.IService jwtService)
    {
        _dbContext = dbContext;
        configuration.GetSection(nameof(SecurityOption)).Bind(_securityOptions);
        _jwtService = jwtService;
        _mailService = mailService;
        _httpContext = httpContextAccessor;
    }
    
    private bool CheckExpiredAccessToken()
    {
        var check = _httpContext.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Expiration);
        if (check != null)
        {
            if (long.TryParse(check.Value, out long expUnixTime))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime);
                if (expirationTime > DateTimeOffset.UtcNow)
                {
                    // Token VẪN CÒN HẠN
                    Console.WriteLine("Token vẫn còn hiệu lực.");
                    return false;
                }
                return true;
            }
        }
        return true;
    }

    private string CheckRefreshToken()
    {
        _httpContext.HttpContext!.Request.Cookies.TryGetValue("HTM-Refresh-Token", out var check);
        if (check == null)
        {
            throw new BadRequestException("Refresh token is invalid or already revoked");
        }

        return check;
    }

    public async Task<string> Register(Request.RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new BadRequestException("Mật khẩu xác nhận không trùng khớp!");
        }

        var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var isExistEmail = await _dbContext.Users
                .AnyAsync(x => x.Email.ToLower() == request.Email.ToLower() );
            if (isExistEmail)
            {
                throw new ConflictException($"Email {request.Email} đã tồn tại trong hệ thống.");            }

            var pepperPassword = request.Password + _securityOptions.Pepper;
            var hashedPassword =
                BCrypt.Net.BCrypt.EnhancedHashPassword(pepperPassword, hashType: BCrypt.Net.HashType.SHA256);

            var newUser = new Repository.Entity.User()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                HashPassword = hashedPassword,
                Role = Role.User,
                IsVerify = false
            };
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            var newClaims = new List<Claim>()
            {
                new Claim("UserId", newUser.Id.ToString()),
                new Claim("IsVerify", newUser.IsVerify.ToString().ToLower()),
            };
            var emailToken = _jwtService.GenerateEmailVerificationToken(newClaims, 2);

            await _mailService.SendMail(new MailContent
            {
                To = request.Email,
                Subject = "Hoa Theo Mua",
                Body = MailTemplate.EmailContainToken(emailToken),
            });


            await transaction.CommitAsync();
            return "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Response.AuthResponse> RefreshToken()
    {
        // Trả về false tức là: CÒN ACCESS VÀ CÒN HẠN
        bool isMissingAccessToken = CheckExpiredAccessToken();

        if (!isMissingAccessToken)
        {// Nếu trả về true: Nghĩa là KHÔNG CÓ ACCESS TOKEN -> Luồng tự động trôi xuống bước 2
            throw new BadRequestException("Access token vẫn còn hiệu lực, không cần làm mới.");        } 

        var rawRefreshToken = CheckRefreshToken();

        var storedToken = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == rawRefreshToken);

        if (storedToken == null)
        {
            throw new ExpiredRefreshTokenException();
        }

        bool isActive = storedToken.RevokedAt == null && storedToken.ExpiredAt > DateTimeOffset.UtcNow;

        if (!isActive)
        {
            throw new ExpiredRefreshTokenException();
        }

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.IsActive = false;
        await _dbContext.SaveChangesAsync();

        string newRawRefreshToken = _jwtService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken()
        {
            RefreshTokenHash = newRawRefreshToken, // Lưu chuỗi thuần trực tiếp theo cấu hình hệ thống
            UserId = storedToken.User.Id,
            IpAddress = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown",
            DeviceLabel = storedToken.DeviceLabel,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true,
        };
        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        var claimsForNewToken = new List<Claim>
        {
            new Claim("UserId", storedToken.User.Id.ToString()),
            new Claim("IsVerify", storedToken.User.IsVerify.ToString().ToLower()),
        };
        string newAccessToken = _jwtService.GenerateAccessToken(claimsForNewToken);

        var result = new Response.AuthResponse()
        {
            RefreshToken = newRawRefreshToken,
            AccessToken = newAccessToken,
            Message = "Refresh token thành công"
        };

        return result;
    }

    public async Task<Response.VerifyEmailResponse?> VerifyEmail(Request.VerifyEmailRequest request)
    {
        var validateToken = _jwtService.ValidateToken(request.Token);
        
        if (validateToken == null)
        {
            throw new BadRequestException("Liên kết xác thực đã hết hạn hoặc không hợp lệ.");
        }

        var userIdStr = validateToken.FindFirst("UserId")?.Value;
        var userId = Guid.Parse(userIdStr!);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại trên hệ thống.");        }

        if (user.IsVerify)
        {
            return new Response.VerifyEmailResponse
            {
                AccessToken = null!,
                RefreshToken = null!,
                Message = "Người dùng đã xác thực trước đó rồi."
            };
        }

        var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            user.IsVerify = true;
            _dbContext.Users.Update(user);
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var authClaims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("IsVerify", user.IsVerify.ToString().ToLower()),
            };

            var accessToken = _jwtService.GenerateAccessToken(authClaims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var httpContext = _httpContext.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP";
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown Device";

            var refreshTokenEntity = new RefreshToken()
            {
                Id = Guid.NewGuid(),
                RefreshTokenHash = refreshToken,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RevokedAt = null,
                DeviceLabel = "",
                UserId = user.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
            };
            await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);


            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return new Response.VerifyEmailResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Message = "Xác thực tài khoản thành công! Chào mừng bạn đến với Hoa Theo Mùa."
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private Guid CheckAccessToken()
    {
        var accessToken = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;
        if (accessToken != null)
        {
            return Guid.Parse(accessToken);
        }

        return Guid.Empty;
    }

    public async Task<Response.GetMeResponse> GetMe()
    {
        var userId = CheckAccessToken();
        if (userId == Guid.Empty)
        {
            throw new MissingAccessTokenException();
        }

        var query = await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(y => new Response.GetMeResponse()
            {
                Email = y.Email,
                FirstName = y.FirstName,
                LastName = y.LastName,
                Avatar = y.Avatar,
            }).FirstOrDefaultAsync();

        if (query == null)
        {
            throw new NotFoundException("User not found");
        }

        return query;
    }

    public async Task<Response.LogoutResponse> Logout()
    {
        try
        {
            // var accessToken = GetAccessToken();

            var rtInCookie = CheckRefreshToken();

            var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x =>
                x.RefreshTokenHash == rtInCookie && x.IsActive == true && x.RevokedAt == null);

            if (refreshToken == null)
            {
                throw new UnauthorizedException("Refresh token is invalid");
            }

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.IsActive = false;
            _dbContext.RefreshTokens.Update(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new Response.LogoutResponse()
            {
                Message = "LOGOUT_SUCCESSFUL",
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            throw;
        }
        // catch (Exception ex)
        // {
        //     throw new Exception("SEVER_ERROR", ex);
        // }
    }
    
    public async Task<Response.LoginResponse> LoginAsync(
        Request.LoginRequest request)
    {
        var httpContext = _httpContext.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown Device";
        var email = request.Email.Trim();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower() && !x.IsDeleted);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
        }

        var pepperPassword = request.Password + _securityOptions.Pepper;

        var isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(
            pepperPassword,
            user.HashPassword,
            hashType: BCrypt.Net.HashType.SHA256
        );

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
        }

       

        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("isVerify", user.IsVerify.ToString())
        };

        var accessToken = _jwtService.GenerateAccessToken(claims);

        var refreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            RefreshTokenHash = refreshToken,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceLabel = "",
            IsActive = true,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        var result = new Response.LoginResponse()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Message = "Login successful!",
            
        };
        return result;

    }
}