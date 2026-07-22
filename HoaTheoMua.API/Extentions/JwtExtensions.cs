using System.Security.Claims;
using System.Text;
using HoaTheoMua.Constants;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.JwtService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace HoaTheoMua.Extentions;

public static class JwtExtensions
{
    public const string AdminPolicy = "AdminPolicy";
    public const string StaffPolicy = "StaffPolicy";
    public const string UserPolicy = "UserPolicy";
    public const string StaffOrAdminPolicy = "StaffOrAdminPolicy";
    public const string UserVerifiedPolicy = "UserVerifiedPolicy";
    public const string CustomerPolicy = "CustomerPolicy";
    public const string CustomerOrAdminPolicy = "CustomerOrAdminPolicy";

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
                    ClockSkew = TimeSpan.Zero, // match taskcoper-be (the issuer)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Match taskcoper-be: only fall back to the cookie when no bearer token
                        // was supplied, and ignore an empty/whitespace cookie value.
                        if (string.IsNullOrWhiteSpace(context.Token)
                            && context.Request.Cookies.TryGetValue(CookieExtensions.AccessTokenCookieName, out var token)
                            && !string.IsNullOrWhiteSpace(token))
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
                    },
                    OnChallenge = context =>
                    {
                        // Mirror taskcoper-be's OnChallenge: a 401 challenge means an [Authorize]
                        // endpoint was reached without a usable access token. Distinguish a *missing*
                        // access token (the caller still holds a refresh cookie, so the client should
                        // silently refresh) from an outright *invalid* one. Expired tokens never reach
                        // here — OnAuthenticationFailed already short-circuits them above.
                        var hasAccessTokenCookie = context.Request.Cookies.TryGetValue(CookieExtensions.AccessTokenCookieName, out var accessTokenCookie)
                            && !string.IsNullOrWhiteSpace(accessTokenCookie);
                        var hasRefreshTokenCookie = context.Request.Cookies.TryGetValue(CookieExtensions.RefreshTokenCookieName, out var refreshTokenCookie)
                            && !string.IsNullOrWhiteSpace(refreshTokenCookie);
                        var hasAuthorizationHeader = !string.IsNullOrWhiteSpace(context.Request.Headers.Authorization.ToString());
                        var hasTokenInRequest = hasAccessTokenCookie || hasAuthorizationHeader;

                        // Unlike taskcoper-be we do NOT clear cookies in the invalid case: this service
                        // only reads the forwarded auth cookies (it never issues or deletes them).
                        if (!hasTokenInRequest && hasRefreshTokenCookie)
                        {
                            throw new MissingAccessTokenException();
                        }

                        throw new InvalidAccessTokenException();
                    }
                };
            });
        services.AddAuthorization(options =>
        {
            //đây là cấu hình mặc định của Author có nghĩ là khi bạn đặt chữ [Authorize] vào bất kì endpoint nào thì nó sẽ lôi thằng này ra kiểm tra
            //và khi dùng nhiều lần khai báo thì nó sẽ kiểm tra nhiều điều kiện (AND) thỏa mãn hết thì người dùng mới được sử dụng endpoint đó
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("IsVerified", "true").Build();

            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireRole(RoleNames.Admin));
            // [Authorize(Policy = JwtExtensions.AdminPolicy)]

            options.AddPolicy(StaffPolicy, policy =>
                policy.RequireRole(RoleNames.Staff));
            // [Authorize(Policy = JwtExtensions.StaffPolicy)]

            options.AddPolicy(UserPolicy, policy =>
                policy.RequireRole(RoleNames.User));

            options.AddPolicy(StaffOrAdminPolicy, policy =>
                policy.RequireRole(RoleNames.Staff, RoleNames.Admin));
            // [Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]

            options.AddPolicy(UserVerifiedPolicy, policy =>
                policy.RequireRole(RoleNames.User)
                    .RequireClaim("IsVerified", "true"));

            // Customer is issued by taskcoper-be at registration; only the storefront (this service) consumes it.
            options.AddPolicy(CustomerPolicy, policy =>
                policy.RequireRole(RoleNames.Customer));
            // [Authorize(Policy = JwtExtensions.CustomerPolicy)]
            options.AddPolicy(CustomerOrAdminPolicy, policy =>
                policy.RequireRole(RoleNames.Customer, RoleNames.Admin));
        });
    }
}