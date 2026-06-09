using System.Security.Claims;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.MailService;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Auth;

public class Service:IService
{
    private readonly AppDbContext _dbContext;
    private readonly SecurityOptions _securityOptions = new();
    private readonly JwtService.IService _jwtService;
    private readonly MailService.IService _mailService;
    private  readonly IHttpContextAccessor _httpContext;
    
    public Service(AppDbContext dbContext, IConfiguration configuration,  
                    MailService.IService mailService, IHttpContextAccessor httpContextAccessor, 
                    JwtService.IService jwtService)
    {
        _dbContext = dbContext;
        configuration.GetSection("SecurityOptions").Bind(_securityOptions);
        _jwtService = jwtService;
        _mailService = mailService;
        _httpContext = httpContextAccessor;
    }
    
    public async Task<string> Register(Request.RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password không khớp!");
        }
        var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var isExistEmail = await _dbContext.Users
                .AnyAsync(x => x.Email == request.Email);
            if (isExistEmail)
            {
                throw new BadHttpRequestException($"Email {request.Email} đã tồn tại trong hệ thống");
            }

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
            await  _dbContext.SaveChangesAsync();
            
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

    public async Task<Response.VerifyEmailResponse?> VerifyEmail(Request.VerifyEmailRequest request)
    {
        
        var validateToken = _jwtService.ValidateToken(request.Token);
        if (validateToken == null)
        {
            throw new BadHttpRequestException("Liên kết xác thực đã hết hạn hoặc không hợp lệ.");
        }
        
        var userIdStr = validateToken.FindFirst("UserId")?.Value;
        var userId = Guid.Parse(userIdStr!);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new BadHttpRequestException("Người dùng không tồn tại trên hệ thống.");
        }

        if (user.IsVerify)
        {//dùng tạm chưa config API ApiResponse
            return new Response.VerifyEmailResponse
            {
                AccessToken = null!,
                RefreshToken = null!,
                Message = "Người dùng đã xác thực"
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
                RefreshTokenHash = _jwtService.GenerateRefreshToken(), 
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
}