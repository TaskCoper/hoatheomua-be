using System.Net;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace HoaTheoMua.Service.Test.PlatformAccountTest;

public class ProviderContracts
{
    [Theory]
    [InlineData(PlatformType.Facebook, "2703062643422200")]
    [InlineData(PlatformType.Instagram, "2703062643422200")]
    [InlineData(PlatformType.Facebook, "")]
    [InlineData(PlatformType.Instagram, "")]
    public void MetaAuthorizationUsesConfigurationOrScopesAndPreservesCallback(PlatformType platform, string configId)
    {
        var options = AccountLifecycle.Options();
        options.Value.Meta.ConfigId = configId;
        var callback = $"https://backend.example/ecommerce-api/v1/post-gen/platform-accounts/{platform.ToString().ToLowerInvariant()}/callback";
        if (platform == PlatformType.Facebook) options.Value.Meta.FacebookCallbackUrl = callback;
        else options.Value.Meta.InstagramCallbackUrl = callback;
        using var http = new HttpClient(new Handler(_ => throw new Exception("Authorization must not call the provider.")));
        var provider = new OAuthProvider(http, options, new AccountLifecycle.Clock());
        var query = QueryHelpers.ParseQuery(new Uri(provider.AuthorizeUrl(platform, "state+/=&", "verifier")).Query);
        Assert.Equal(callback, query["redirect_uri"]);
        Assert.Equal("state+/=&", query["state"]);
        Assert.Equal("code", query["response_type"]);
        Assert.False(query.ContainsKey("client_secret"));
        if (configId.Length > 0)
        {
            Assert.Equal(configId, query["config_id"]);
            Assert.False(query.ContainsKey("scope"));
        }
        else
        {
            Assert.False(query.ContainsKey("config_id"));
            Assert.Equal(string.Join(',', OAuthProvider.RequiredScopes(platform)), query["scope"]);
        }
    }

    [Theory]
    [InlineData("not-an-id")]
    [InlineData("123\n")]
    public void MetaRejectsInvalidConfigurationId(string configId)
    {
        var options = AccountLifecycle.Options(); options.Value.Meta.ConfigId = configId;
        Assert.Equal("PLATFORM_NOT_CONFIGURED", Assert.Throws<AccountException>(() => options.Value.RequireConfigured(PlatformType.Facebook)).MessageCode);
    }

    [Theory]
    [InlineData(PlatformType.Facebook)]
    [InlineData(PlatformType.Instagram)]
    public async Task MetaExchangesCodeChecksPermissionsAndDiscoversPageTokens(PlatformType platform)
    {
        var calls = new List<string>();
        var handler = new Handler(async r =>
        {
            calls.Add(r.RequestUri!.AbsolutePath);
            var q = QueryHelpers.ParseQuery(r.RequestUri.Query);
            Assert.Equal("graph.facebook.com", r.RequestUri.Host);
            if (r.RequestUri.AbsolutePath.EndsWith("oauth/access_token"))
            {
                Assert.Equal("test-secret", q["client_secret"]);
                if (q.ContainsKey("code")) { Assert.Equal("the-code", q["code"]); return Json(new { access_token = "short" }); }
                Assert.Equal("short", q["fb_exchange_token"]); return Json(new { access_token = "long", expires_in = 5000 });
            }
            Assert.Equal("long", r.Headers.Authorization!.Parameter);
            if (r.RequestUri.AbsolutePath.EndsWith("me/permissions")) return Json(new { data = OAuthProvider.RequiredScopes(platform).Select(x => new { permission = x, status = "granted" }) });
            Assert.EndsWith("me/accounts", r.RequestUri.AbsolutePath);
            return await Task.FromResult(Json(new { data = new[] { new { id = "123", name = "Page", access_token = "page-token", tasks = new[] { "CREATE_CONTENT" }, instagram_business_account = new { id = "456", username = "flowers" } } } }));
        });
        var provider = Client(handler);
        var result = Assert.Single(await provider.Exchange(platform, "the-code", "verifier", default));
        Assert.Equal(platform == PlatformType.Facebook ? "123" : "456", result.ExternalId);
        Assert.Equal("123", result.PageId); Assert.Equal("page-token", result.Credentials.AccessToken); Assert.Null(result.Credentials.RefreshToken);
        Assert.Equal(4, calls.Count); Assert.NotNull(result.ExpiresAt);
    }

    [Fact]
    public async Task ZaloUsesFormSecretHeaderPkceAndReadsOaIdentityFromProvider()
    {
        var count = 0;
        var handler = new Handler(async r =>
        {
            count++;
            if (count == 1)
            {
                Assert.Equal("https://oauth.zaloapp.com/v4/oa/access_token", r.RequestUri!.ToString());
                Assert.Equal(HttpMethod.Post, r.Method); Assert.Equal("test-oa-secret", Assert.Single(r.Headers.GetValues("secret_key")));
                Assert.Equal("application/x-www-form-urlencoded", r.Content!.Headers.ContentType!.MediaType);
                var form = QueryHelpers.ParseQuery(await r.Content.ReadAsStringAsync());
                Assert.Equal("authorization_code", form["grant_type"]); Assert.Equal("verifier", form["code_verifier"]);
                return Json(new { access_token = "oa-access", refresh_token = "oa-refresh", expires_in = "90000" });
            }
            Assert.Equal("https://openapi.zalo.me/v2.0/oa/getoa", r.RequestUri!.ToString());
            Assert.Equal("oa-access", Assert.Single(r.Headers.GetValues("access_token")));
            return Json(new { error = 0, data = new { oa_id = 1234567890123456789L, name = "OA Flowers" } });
        });
        var provider = Client(handler);
        var auth = new Uri(provider.AuthorizeUrl(PlatformType.Zalo, "the-state", "verifier"));
        var authQuery = QueryHelpers.ParseQuery(auth.Query);
        Assert.NotEmpty(authQuery["code_challenge"].ToString());
        Assert.Equal("the-state", QueryHelpers.ParseQuery(new Uri(authQuery["redirect_uri"].ToString()).Query)["state"]);
        var result = Assert.Single(await provider.Exchange(PlatformType.Zalo, "code", "verifier", default));
        Assert.Equal("1234567890123456789", result.ExternalId); Assert.Equal("OA Flowers", result.Name);
        Assert.Equal("oa-refresh", result.Credentials.RefreshToken); Assert.NotNull(result.ExpiresAt); Assert.Null(result.RefreshExpiresAt);
    }

    [Fact]
    public async Task ZaloRefreshMakesOnlyOneRequestAndKeepsNewPair()
    {
        var calls = 0;
        var provider = Client(new Handler(async r =>
        {
            calls++; var form = QueryHelpers.ParseQuery(await r.Content!.ReadAsStringAsync());
            Assert.Equal("refresh_token", form["grant_type"]); Assert.Equal("previous", form["refresh_token"]);
            return Json(new { access_token = "new-access", refresh_token = "new-refresh", expires_in = 90000 });
        }));
        var result = await provider.Refresh(PlatformType.Zalo, "oa", null, new() { AccessToken = "old", RefreshToken = "previous" }, default);
        Assert.Equal(1, calls); Assert.Equal("oa", result.ExternalId); Assert.Equal("new-refresh", result.Credentials.RefreshToken);
    }

    [Theory]
    [InlineData(429, "PLATFORM_RATE_LIMITED")]
    [InlineData(503, "PLATFORM_PROVIDER_UNAVAILABLE")]
    [InlineData(401, "PLATFORM_REAUTH_REQUIRED")]
    public async Task HttpFailuresAreSanitizedAndNeverRetried(int status, string code)
    {
        var calls = 0;
        var provider = Client(new Handler(r => { calls++; return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent("SECRET BODY") }); }));
        var e = await Assert.ThrowsAsync<AccountException>(() => provider.Exchange(PlatformType.Zalo, "code", "verifier", default));
        Assert.Equal(code, e.MessageCode); Assert.DoesNotContain("SECRET", e.Message); Assert.Null(e.InnerException); Assert.Equal(1, calls);
    }

    [Fact]
    public async Task MissingPermissionsStopsBeforePageDiscovery()
    {
        var count = 0;
        var provider = Client(new Handler(r => Task.FromResult(++count <= 2 ? Json(new { access_token = "token" }) : Json(new { data = Array.Empty<object>() }))));
        Assert.Equal("PLATFORM_REAUTH_REQUIRED", (await Assert.ThrowsAsync<AccountException>(() => provider.Exchange(PlatformType.Facebook, "code", "verifier", default))).MessageCode);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task PaginationIgnoresUntrustedNextUrlAndUsesCursorOnFixedHost()
    {
        var pages = 0;
        var provider = Client(new Handler(r =>
        {
            Assert.Equal("graph.facebook.com", r.RequestUri!.Host);
            if (r.RequestUri.AbsolutePath.EndsWith("access_token")) return Task.FromResult(Json(new { access_token = "token" }));
            if (r.RequestUri.AbsolutePath.EndsWith("permissions")) return Task.FromResult(Json(new { data = OAuthProvider.RequiredScopes(PlatformType.Facebook).Select(x => new { permission = x, status = "granted" }) }));
            pages++;
            return Task.FromResult(pages == 1
                ? Json(new { data = Array.Empty<object>(), paging = new { next = "https://evil.example/steal?access_token=token", cursors = new { after = "safe-cursor" } } })
                : Json(new { data = new[] { new { id = "id", name = "page", access_token = "token", tasks = new[] { "CREATE_CONTENT" } } } }));
        }));
        Assert.Single(await provider.Exchange(PlatformType.Facebook, "code", "verifier", default)); Assert.Equal(2, pages);
    }

    internal static OAuthProvider Client(Handler h) => new(new HttpClient(h) { MaxResponseContentBufferSize = 1024 * 1024 }, AccountLifecycle.Options(), new AccountLifecycle.Clock());
    internal static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json") };
    internal sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => respond(request); }
}
