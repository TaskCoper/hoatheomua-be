using System.Security.Claims;
using System.Text;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JwtService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HoaTheoMua.Extentions;

public static class JwtExtensions
{
    public const string AdminPolicy = "AdminPolicy";
    public const string StaffPolicy = "StaffPolicy";
    public const string UserPolicy = "UserPolicy";
    public const string StaffOrAdminPolicy = "StaffOrAdminPolicy";
    public const string UserVerifiedPolicy = "UserVerifiedPolicy";
    
    public static void AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        JwtOption jwtOption = new JwtOption();
        configuration.GetSection(nameof(JwtOption)).Bind(jwtOption);
        var key = Encoding.UTF8.GetBytes(jwtOption.AccessTokenKey);
    
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,//dung signature, dung issure nua, dung server
                    ValidateAudience = true, // 
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOption.Issuer,
                    ValidAudience = jwtOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        
                        if (context.Request.Cookies.TryGetValue(CookieExtensions.AccessTokenCookieName, out var token))
                        {
                            context.Token = token; 
                        }

                        
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            throw new ExpiredAccessTokenException();
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization(options =>
        {
            // Độ lại IsVerify của Base Authorize của .NET luôn 
            
            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireRole("Admin"));
            // [Authorize(Policy = JwtExtensions.AdminPolicy)]
        
            options.AddPolicy(StaffPolicy, policy =>
                policy.RequireRole("Staff"));
            // [Authorize(Policy = JwtExtensions.StaffPolicy)]
        
            options.AddPolicy(UserPolicy, policy =>
                policy.RequireRole("User"));
        
            options.AddPolicy(StaffOrAdminPolicy, policy =>
                policy.RequireRole("Staff", "Admin"));
            // [Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]
            
            options.AddPolicy(UserVerifiedPolicy, policy =>
                policy.RequireRole("User")
                    .RequireClaim("IsVerify", "true"));
        });
    }
}