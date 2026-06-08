using System.Security.Claims;
using HoaTheoMua.Repository;
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
                .AnyAsync(x =>
                    x.Email == request.Email && x.IsVerify);
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
                IsVerify = false
            };
            await _dbContext.Users.AddAsync(newUser);
           // await  _dbContext.SaveChangesAsync();
            var newClaims = new List<Claim>()
            {
                new Claim("UserId", newUser.Id.ToString()),
                new Claim("IsVerify", newUser.IsVerify.ToString()),
            };
            var emailToken = _jwtService.GenerateEmailVerificationToken(newClaims, 2);

            await _mailService.SendMail(new MailContent
            {
                To = request.Email,
                Subject = "Hoa Theo Mua",
                Body = MailTemplate.EmailContainToken(emailToken),
            });

            return "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }
}