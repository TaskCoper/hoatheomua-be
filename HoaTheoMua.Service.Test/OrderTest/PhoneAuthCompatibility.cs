using System.Security.Claims;
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HoaTheoMua.Service.Test.OrderTest;

public class PhoneAuthCompatibility
{
    private static ClaimsPrincipal Principal(bool verified, bool phone, bool forgot = false, string role = "Customer")
    {
        var claims = new List<Claim> { new("UserId", Guid.NewGuid().ToString()), new(ClaimTypes.Role, role),
            new("IsVerified", verified ? "true" : "false") };
        if (phone) claims.Add(new("CanUseStorefront", "true"));
        if (forgot) claims.Add(new("IsForgotPassword", "true"));
        return new(new ClaimsIdentity(claims, "test"));
    }

    // Shared User Authentication BR-42/43: phone claims grant storefront access, not email verification.
    [Theory]
    [InlineData(false, true, false, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, false, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    public void PhoneAndLegacyToken_ResolveStorefrontEligibility(bool verified, bool phone, bool forgot, bool allowed)
    {
        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = Principal(verified, phone, forgot) } });
        Assert.Equal(allowed, service.CanUseStorefront);
        Assert.Null(service.Email);
        Assert.NotEqual(Guid.Empty, service.GetRequiredUserId());
    }

    // BR-43: Customer policy remains compatible; default verified and privileged policies stay closed.
    [Fact]
    public async Task PhoneToken_DoesNotGainDefaultOrAdminPermissions()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["JwtOption:AccessTokenKey"] = "phone-auth-test-key-with-at-least-32-bytes",
            ["JwtOption:Issuer"] = "test", ["JwtOption:Audience"] = "test" }).Build();
        var services = new ServiceCollection();
        services.AddLogging(); services.AddJwtServices(config);
        await using var provider = services.BuildServiceProvider();
        var auth = provider.GetRequiredService<IAuthorizationService>();
        var principal = Principal(false, true);
        Assert.True((await auth.AuthorizeAsync(principal, null, JwtExtensions.CustomerPolicy)).Succeeded);
        Assert.False((await auth.AuthorizeAsync(principal, null, JwtExtensions.AdminPolicy)).Succeeded);
        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        Assert.False((await auth.AuthorizeAsync(principal, null, await policies.GetDefaultPolicyAsync())).Succeeded);
    }
}
