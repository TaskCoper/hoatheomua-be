
using System.Security.Claims;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.JwtService;
using HoaTheoMua.Service.MailService;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace HoaTheoMua.Service.Auth;

public class Service:IService
{
    private readonly AppDbContext _dbContext;
    private readonly SecurityOptions _securityOptions = new();
    private readonly JwtService.IService _jwtService;
    private readonly CacheService.IService _cacheService;
    private readonly MailService.IService _mailService;
    private  readonly IHttpContextAccessor _httpContext;
    
    public Service(AppDbContext dbContext, IConfiguration configuration,  
                    CacheService.IService cacheService,  MailService.IService mailService,
                    IHttpContextAccessor httpContextAccessor, JwtService.IService jwtService)
    {
        _dbContext = dbContext;
        configuration.GetSection("SecurityOptions").Bind(_securityOptions);
        _jwtService = jwtService;
        _cacheService = cacheService;
        _mailService = mailService;
        _httpContext = httpContextAccessor;
    }
    
    public async Task<string> Register(Request.RegisterRequest request)
    {

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password không khớp!");
        }

        var emailLockKey = $"EmailLock:{request.Email}";
        var isLocked = await _cacheService.TryGet<string>(emailLockKey);

        if (isLocked != null)
        {
            throw new BadHttpRequestException($"Email {request.Email} đã được đăng ký và đang chờ xác thực.");
        }
            
        var isExistEmail = await _dbContext.Users
            .AnyAsync(x => x.Email == request.Email && x.IsVerify);
        if (isExistEmail)
        {
            throw new BadHttpRequestException($"Email {request.Email} đã tồn tại trong hệ thống");
        }
        var pepperPassword = request.Password + _securityOptions.Pepper;
        var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(pepperPassword, hashType: BCrypt.Net.HashType.SHA256);
        
        var pendingUser = new PendingUserCache()
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = hashedPassword,
        };
        
        var token = Guid.NewGuid().ToString("N");
        var redisKey = $"VerifyToken:{token}";
        TimeSpan linkExpiration = TimeSpan.FromMinutes(15);
        
        await _cacheService.Set(redisKey, pendingUser, linkExpiration);
        await _cacheService.Set(emailLockKey, "Locked", linkExpiration);
        
        var verificationLink = $"https://hoatheomua.com/verify?token={token}";
        var body = "<!DOCTYPE html>\n<html lang=\"vi\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Xác thực tài khoản - Hoa Theo Mùa</title>\n    <style>\n        @media screen and (max-width: 600px) {\n            .email-container { width: 100% !important; padding: 15px !important; }\n            .content-wrapper { padding: 30px 20px !important; }\n            .btn-verify { display: block !important; text-align: center !important; padding: 16px 20px !important; }\n        }\n    </style>\n</head>\n<body style=\"margin: 0; padding: 0; background-color: #eef3f0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; color: #3f3f3f; -webkit-font-smoothing: antialiased;\">\n\n    <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background-color: #eef3f0; padding: 50px 0;\">\n        <tr>\n            <td align=\"center\">\n                \n                <table class=\"email-container\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"580\" style=\"background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #d6e2dc; box-shadow: 0 10px 25px -5px rgba(81, 108, 108, 0.05), 0 8px 10px -6px rgba(81, 108, 108, 0.05);\">\n                    \n                    <tr>\n                        <td align=\"center\" style=\"padding: 40px 40px 10px 40px;\">\n                            \n                            <h1 style=\"margin: 0; color: #7a9c59; font-size: 22px; font-weight: 640; letter-spacing: 2px; text-transform: uppercase;\">\n                                Hoa Theo Mùa\n                            </h1>\n                            <div style=\"height: 2px; width: 30px; background-color: #7a9c59; margin: 15px auto 0 auto; border-radius: 2px;\"></div>\n                        </td>\n                    </tr>\n\n                    <tr>\n                        <td class=\"content-wrapper\" style=\"padding: 30px 45px 40px 45px;\">\n                            <p style=\"margin-top: 0; margin-bottom: 16px; color: #516c6c; font-size: 18px; font-weight: 600; text-align: center;\">\n                                Xác thực tài khoản của bạn\n                            </p>\n                            \n                            <p style=\"margin: 0 0 24px 0; font-size: 15px; line-height: 1.6; color: #516c6c; text-align: center;\">\n                                Chào bạn, cảm ơn bạn đã ghé chơi khu vườn <strong>Hoa Theo Mùa</strong>. Để bắt đầu hành trình khám phá những hương sắc ngọt ngào nhất, bạn vui lòng xác nhận tài khoản bằng cách nhấn vào nút bên dưới nhé.\n                            </p>\n\n                            <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"margin: 32px 0;\">\n                                <tr>\n                                    <td align=\"center\">\n                                        <a href=\"{{verification_link}}\" class=\"btn-verify\" style=\"background-color: #516c6c; color: #ffffff; display: inline-block; padding: 14px 36px; font-size: 15px; font-weight: 500; text-decoration: none; border-radius: 8px; box-shadow: 0 4px 14px rgba(81, 108, 108, 0.25); letter-spacing: 0.5px; transition: all 0.2s ease;\">\n                                            Kích Hoạt Tài Khoản\n                                        </a>\n                                    </td>\n                                </tr>\n                            </table>\n\n                            <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background-color: #e3ece7; border-radius: 8px; border-left: 4px solid #7a9c59;\">\n                                <tr>\n                                    <td style=\"padding: 16px 20px;\">\n                                        <p style=\"margin: 0 0 6px 0; font-size: 13px; font-weight: 600; color: #2f4646;\">\n                                            Bạn gặp sự cố với nút bấm?\n                                        </p>\n                                        <p style=\"margin: 0; font-size: 12px; color: #6b8079; line-height: 1.5; word-break: break-all;\">\n                                            Sao chép liên kết này dán vào trình duyệt: <br>\n                                            <a href=\"{{verification_link}}\" style=\"color: #516c6c; text-decoration: underline; font-weight: 500;\">{{verification_link}}</a>\n                                        </p>\n                                    </td>\n                                </tr>\n                            </table>\n                        </td>\n                    </tr>\n\n                    <tr>\n                        <td style=\"background-color: #fafbfa; padding: 30px 45px; text-align: center; border-top: 1px solid #d6e2dc;\">\n                            <p style=\"margin: 0 0 12px 0; font-size: 12px; color: #6b8079; line-height: 1.5;\">\n                                Bạn nhận được email này vì đã đăng ký thành viên tại hoatheomua.com.<br>Nếu không phải bạn thực hiện, bạn có thể an tâm bỏ qua email này.\n                            </p>\n                            <p style=\"margin: 0; font-size: 11px; color: #bcd0c7; letter-spacing: 0.5px;\">\n                                &copy; 2026 Hoa Theo Mùa. All right reserved.\n                            </p>\n                        </td>\n                    </tr>\n\n                </table>\n                </td>\n        </tr>\n    </table>\n\n</body>\n</html>";
        body = body.Replace("{{verification_link}}", verificationLink);
        await _mailService.SendMail(new MailContent()
            {
                To = request.Email,
                Subject = "Welcome to Hoa Mua He",
                Body = body
            });
        
        return "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
    }

    
}