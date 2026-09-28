using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Accounts = HoaTheoMua.Service.PlatformAccount;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/post-gen/platform-accounts")]
[Authorize(Policy = JwtExtensions.AdminPolicy)]
public class PlatformAccountController(Accounts.IService service, IOptions<Accounts.PlatformAccountOptions> settings) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? platform, CancellationToken ct)
        => Result(await service.List(platform, ct));

    [HttpPost("{platform}/connect")]
    public async Task<IActionResult> Connect(string platform, CancellationToken ct)
    {
        var proof = Accounts.Service.Random();
        var result = await service.Connect(platform, proof, ct: ct);
        SetBrowserCookie(result, proof, platform);
        return Result(result);
    }

    [HttpPost("{id:guid}/reconnect")]
    public async Task<IActionResult> Reconnect(Guid id, CancellationToken ct)
    {
        var proof = Accounts.Service.Random();
        var result = await service.Reconnect(id, proof, ct);
        var callback = QueryHelpers.ParseQuery(new Uri(result.AuthorizationUrl).Query)["redirect_uri"].ToString();
        var platform = new Uri(callback).Segments[^2].TrimEnd('/');
        SetBrowserCookie(result, proof, platform);
        return Result(result);
    }

    [AllowAnonymous]
    [HttpGet("{platform}/callback")]
    public async Task<IActionResult> Callback(string platform, [FromQuery] string? state, [FromQuery] string? code,
        [FromQuery] string? error, CancellationToken ct)
    {
        NoCache();
        var name = CookieName(state ?? "");
        var proof = Request.Cookies[name];
        try { return Result(await service.Callback(platform, state, code, error, proof, ct)); }
        finally { Response.Cookies.Delete(name, CookieOptions(platform)); }
    }

    [HttpPost("{id:guid}/refresh")]
    public async Task<IActionResult> Refresh(Guid id, CancellationToken ct) => Result(await service.Refresh(id, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Disconnect(Guid id, CancellationToken ct) => Result(await service.Disconnect(id, ct));

    private void SetBrowserCookie(Accounts.Response.Connection connection, string proof, string platform)
    {
        var query = QueryHelpers.ParseQuery(new Uri(connection.AuthorizationUrl).Query);
        var state = query.TryGetValue("state", out var stateValue) ? stateValue.ToString() : "";
        if (state.Length == 0)
            state = QueryHelpers.ParseQuery(new Uri(query["redirect_uri"].ToString()).Query)["state"].ToString();
        var options = CookieOptions(platform); options.Expires = connection.ExpiresAt;
        Response.Cookies.Append(CookieName(state), proof, options);
    }
    private CookieOptions CookieOptions(string platform)
    {
        var parsedPlatform = Accounts.Service.Parse(platform);
        // Error callbacks must still return the service's error when provider settings are absent.
        Uri.TryCreate(settings.Value.Callback(parsedPlatform), UriKind.Absolute, out var callback);
        return new()
        {
            HttpOnly = true, Secure = callback?.Scheme == Uri.UriSchemeHttps || Request.IsHttps, SameSite = SameSiteMode.Lax,
            Path = callback?.AbsolutePath ?? $"/api/v1/post-gen/platform-accounts/{parsedPlatform.ToString().ToLowerInvariant()}/callback",
            IsEssential = true
        };
    }
    private static string CookieName(string state) => "postgen_oauth_" + Accounts.Service.Hash(state)[..24];
    private void NoCache()
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
    }
    private IActionResult Result<T>(T value)
    {
        NoCache();
        return Ok(ApiResponseFactory.Base(value, true, "", HttpContext.TraceIdentifier));
    }
}
