using System.Net;
using HoaTheoMua.Controllers;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Accounts = HoaTheoMua.Service.PlatformAccount;

// Small controller contract probe; no database, HTTP listener or provider calls.
internal static class CookieContract
{
    public static async Task Run()
    {
        foreach (var prefix in new[] { "/api", "/ecommerce-api" })
        foreach (var platform in new[] { "facebook", "instagram" })
        {
            var callback = $"https://backend.example{prefix}/v1/post-gen/platform-accounts/{platform}/callback";
            var options = Options.Create(new PlatformAccountOptions
            {
                Meta = new() { FacebookCallbackUrl = callback, InstagramCallbackUrl = callback }
            });
            var service = new Stub(callback);
            var controller = new PlatformAccountController(service, options)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            // Request is HTTP behind ingress, while the browser-facing callback is HTTPS.
            controller.Request.Scheme = "http";
            await controller.Connect(platform, default);
            var header = controller.Response.Headers.SetCookie.ToString();
            Require(header.Contains("path=" + new Uri(callback).AbsolutePath), "public cookie path");
            Require(header.Contains("secure", StringComparison.OrdinalIgnoreCase), "HTTPS callback uses Secure");
            Require(header.Contains("httponly", StringComparison.OrdinalIgnoreCase), "HttpOnly");
            Require(header.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase), "SameSite Lax");
            var cookies = new CookieContainer();
            cookies.SetCookies(new Uri("https://backend.example"), header);
            Require(cookies.GetCookies(new Uri("https://backend.example/unrelated")).Count == 0, "cookie restricted to callback");
            var context = new DefaultHttpContext();
            context.Request.Headers.Cookie = cookies.GetCookieHeader(new Uri(callback));
            controller.ControllerContext = new ControllerContext { HttpContext = context };
            await controller.Callback(platform, "state", "code", null, default);
            Require(service.ReceivedProof == service.IssuedProof && !string.IsNullOrEmpty(service.ReceivedProof), "browser proof survives public path");
            cookies.SetCookies(new Uri(callback), controller.Response.Headers.SetCookie.ToString());
            Require(cookies.GetCookies(new Uri(callback)).Count == 0, "callback deletes same cookie path");
            Console.WriteLine($"PASS cookie connect/callback/delete: {platform}, prefix {prefix}");
        }
    }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
    private sealed class Stub(string callback) : Accounts.IService
    {
        public string? IssuedProof, ReceivedProof;
        public Task<Response.Connection> Connect(string platform, string browserProof, Guid? accountId = null, CancellationToken ct = default)
        {
            IssuedProof = browserProof;
            return Task.FromResult(new Response.Connection(Guid.NewGuid(),
                "https://www.facebook.com/dialog/oauth?state=state&redirect_uri=" + Uri.EscapeDataString(callback), DateTimeOffset.UtcNow.AddMinutes(10)));
        }
        public Task<List<Response.Account>> Callback(string platform, string? state, string? code, string? error, string? browserProof, CancellationToken ct = default)
        { ReceivedProof = browserProof; return Task.FromResult(new List<Response.Account>()); }
        public Task<List<Response.Account>> List(string? platform = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Response.Connection> Reconnect(Guid id, string browserProof, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Response.Account> Refresh(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Response.Account> Disconnect(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
