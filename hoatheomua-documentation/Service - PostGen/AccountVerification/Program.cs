using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Controllers;
using HoaTheoMua.Extentions;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Accounts = HoaTheoMua.Service.PlatformAccount;

if (args.Contains("--cookie-contract")) { await CookieContract.Run(); return; }

var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set a disposable local PostgreSQL connection."));
if (settings.Host is not ("127.0.0.1" or "localhost")) throw new InvalidOperationException("Local test database only.");
var database = "account_probe_" + Guid.NewGuid().ToString("N");
await using var admin = new NpgsqlConnection(settings.ConnectionString); await admin.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
var checks = 0; var requests = 0; var evidence = new List<object>(); var cleanup = false;
try
{
    settings.Database = database;
    var ds = new NpgsqlDataSourceBuilder(settings.ConnectionString); ds.EnableDynamicJson();
    ds.MapEnum<PlatformType>("PlatformType"); ds.MapEnum<SystemPromptType>("SystemPromptType"); ds.MapEnum<CardFormType>("CardFormType"); ds.MapEnum<ClientHistoryType>("ClientHistoryType");
    await using var source = ds.Build();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:55442");
    const string key = "postgen-account-local-jwt-test-only-64-characters-never-production";
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    { ["JwtOption:Issuer"] = "probe", ["JwtOption:Audience"] = "probe", ["JwtOption:AccessTokenKey"] = key });
    builder.Services.AddDbContext<AppDbContext>(x => x.UseNpgsql(source));
    builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
    builder.Services.AddControllers().AddApplicationPart(typeof(PlatformAccountController).Assembly);
    builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); builder.Services.AddJwtServices(builder.Configuration);
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.Configure<PlatformAccountOptions>(o =>
    {
        o.ActiveEncryptionKeyId = "probe"; o.EncryptionKeys["probe"] = Convert.ToBase64String(new byte[32]);
        o.Meta = new() { Enabled = true, AppId = "probe-app", AppSecret = "secret-app", GraphApiVersion = "v23.0",
            FacebookCallbackUrl = "http://127.0.0.1:55442/api/v1/post-gen/platform-accounts/facebook/callback",
            InstagramCallbackUrl = "http://127.0.0.1:55442/api/v1/post-gen/platform-accounts/instagram/callback" };
        o.Zalo = new() { Enabled = true, AppId = "probe-oa", AppSecret = "secret-oa", CallbackUrl = "http://127.0.0.1:55442/api/v1/post-gen/platform-accounts/zalo/callback" };
    });
    builder.Services.AddSingleton<TokenProtector>(); builder.Services.AddScoped<Accounts.IService, Accounts.Service>();
    var handler = new FakeProvider();
    builder.Services.AddHttpClient<IOAuthProvider, OAuthProvider>().ConfigurePrimaryHttpMessageHandler(() => handler).RemoveAllLoggers();
    await using var app = builder.Build();
    await using (var scope = app.Services.CreateAsyncScope()) await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
    using var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false }) { BaseAddress = new Uri("http://127.0.0.1:55442") };
    const string root = "/api/v1/post-gen/platform-accounts";
    var actor = Guid.NewGuid();
    string Jwt(string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("probe", "probe",
        new[] { new Claim("UserId", actor.ToString()), new Claim(ClaimTypes.Role, role), new Claim("IsVerified", "true") },
        expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
    void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    async Task<JsonElement> Call(string name, HttpMethod method, string path, string? role = "Admin", int expected = 200)
    {
        using var request = new HttpRequestMessage(method, path);
        if (role != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Jwt(role));
        using var response = await client.SendAsync(request); requests++;
        var raw = await response.Content.ReadAsStringAsync();
        Check((int)response.StatusCode == expected, $"{name}: expected {expected}, got {(int)response.StatusCode}");
        foreach (var secret in new[] { "secret-access", "secret-refresh", "secret-page", "secret-user", "secret-app" }) Check(!raw.Contains(secret), name + " response redacts " + secret);
        var json = string.IsNullOrWhiteSpace(raw) ? JsonSerializer.SerializeToElement(new { }) : JsonDocument.Parse(raw).RootElement.Clone();
        evidence.Add(new { name, status = (int)response.StatusCode, traceId = json.TryGetProperty("traceId", out var trace) ? trace.GetString() : null });
        return json;
    }
    string State(JsonElement result)
    {
        var q = QueryHelpers.ParseQuery(new Uri(result.GetProperty("value").GetProperty("authorizationUrl").GetString()!).Query);
        return q.ContainsKey("state") ? q["state"].ToString() : QueryHelpers.ParseQuery(new Uri(q["redirect_uri"].ToString()).Query)["state"].ToString();
    }
    await Call("anonymous list denied", HttpMethod.Get, root, null, 401);
    foreach (var role in new[] { "Staff", "Customer" })
    {
        await Call(role + " list denied", HttpMethod.Get, root, role, 403);
        await Call(role + " connect denied", HttpMethod.Post, root + "/facebook/connect", role, 403);
        await Call(role + " refresh denied", HttpMethod.Post, root + $"/{Guid.NewGuid()}/refresh", role, 403);
        await Call(role + " disconnect denied", HttpMethod.Delete, root + $"/{Guid.NewGuid()}", role, 403);
    }
    await Call("unknown platform", HttpMethod.Post, root + "/tiktok/connect", expected: 400);
    await Call("callback missing state", HttpMethod.Get, root + "/facebook/callback?code=fake", null, 400);
    var ids = new List<Guid>();
    foreach (var platform in new[] { "facebook", "instagram", "zalo" })
    {
        var connect = await Call(platform + " connect", HttpMethod.Post, root + $"/{platform}/connect");
        var state = State(connect);
        var callback = await Call(platform + " callback without JWT", HttpMethod.Get, root + $"/{platform}/callback?state={state}&code=fake", null);
        var account = callback.GetProperty("value")[0]; var id = account.GetProperty("id").GetGuid(); ids.Add(id);
        Check(account.GetProperty("platform").GetString() == platform, "correct platform");
        Check(account.GetProperty("status").GetString() == "Connected", "connected state");
        await Call(platform + " callback replay rejected", HttpMethod.Get, root + $"/{platform}/callback?state={state}&code=fake", null, 400);
        await Call(platform + " refresh", HttpMethod.Post, root + $"/{id}/refresh");
        await Call(platform + " disconnect", HttpMethod.Delete, root + $"/{id}");
        var reconnect = await Call(platform + " reconnect", HttpMethod.Post, root + $"/{id}/reconnect");
        var reconnected = await Call(platform + " reconnect callback", HttpMethod.Get, root + $"/{platform}/callback?state={State(reconnect)}&code=fake", null);
        Check(reconnected.GetProperty("value")[0].GetProperty("id").GetGuid() == id, "stable account ID");
    }
    var list = await Call("shared list", HttpMethod.Get, root);
    Check(list.GetProperty("value").GetArrayLength() == 3, "three accounts");
    var missingBrowser = await Call("new OAuth session", HttpMethod.Post, root + "/facebook/connect");
    using (var outsider = new HttpClient())
    {
        using var response = await outsider.GetAsync($"http://127.0.0.1:55442{root}/facebook/callback?state={State(missingBrowser)}&code=fake");
        requests++; Check(response.StatusCode == HttpStatusCode.BadRequest, "state stolen without browser cookie rejected");
    }
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accounts = await db.PlatformAccounts.AsNoTracking().ToListAsync();
        Check(accounts.Count == 3, "database identities unique");
        Check(accounts.All(x => x.ProtectedCredentials != null && !x.ProtectedCredentials.Contains("secret-")), "encrypted credentials in PostgreSQL");
        Check((await db.PlatformOAuthSessions.Where(x => x.UsedAt != null).ToListAsync()).All(x => x.ProtectedVerifier == null), "used verifiers cleared");
    }
    foreach (var id in ids) await Call("final disconnect", HttpMethod.Delete, root + $"/{id}");
    await app.StopAsync();
    Console.WriteLine($"PASS {requests} HTTP requests; {checks} assertions; {handler.Calls} fake provider calls; zero real provider/AI calls.");
}
finally
{
    await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
    await using var count = new NpgsqlCommand("SELECT count(*) FROM pg_database WHERE datname = @name", admin); count.Parameters.AddWithValue("name", database);
    cleanup = (long)(await count.ExecuteScalarAsync())! == 0;
    if (!cleanup) throw new Exception("Disposable database cleanup failed.");
}
var results = Path.Combine(Directory.GetCurrentDirectory(), "results"); Directory.CreateDirectory(results);
await File.WriteAllTextAsync(Path.Combine(results, "database-flow-validator.json"), JsonSerializer.Serialize(new
{
    skill = "database-flow-validator", status = "PASS", flow = "Admin connect -> anonymous browser-bound callback -> list -> refresh -> disconnect -> reconnect",
    assertions = new { requests, checks, encryptedAtRest = true, stableAccountIds = true, stateReplayDenied = true, realAiCalls = 0, realProviderCalls = 0 },
    queries = new[] { "SELECT account metadata and protected credentials in disposable database (values redacted)", "SELECT used OAuth sessions", "SELECT count(*) FROM pg_database WHERE datname = @disposable_name" },
    correlation_ids = evidence, cleanup = new { databaseDropped = cleanup }, failures = Array.Empty<string>()
}, new JsonSerializerOptions { WriteIndented = true }));

sealed class FakeProvider : HttpMessageHandler
{
    public int Calls;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        Calls++; object value; var path = r.RequestUri!.AbsolutePath;
        if (r.RequestUri.Host == "oauth.zaloapp.com") value = new { access_token = "secret-access", refresh_token = "secret-refresh", expires_in = "90000" };
        else if (path.EndsWith("getoa")) value = new { error = 0, data = new { oa_id = "oa1", name = "OA" } };
        else if (path.EndsWith("oauth/access_token")) value = new { access_token = "secret-user", expires_in = 5000 };
        else if (path.EndsWith("permissions")) value = new { data = new[] { "pages_show_list", "pages_read_engagement", "pages_manage_posts", "instagram_basic", "instagram_content_publish" }.Select(x => new { permission = x, status = "granted" }) };
        else if (path.EndsWith("me/accounts")) value = new { data = new[] { new { id = "page1", name = "Page", access_token = "secret-page", tasks = new[] { "CREATE_CONTENT" }, instagram_business_account = new { id = "ig1", username = "flowers" } } } };
        else if (path.EndsWith("debug_token")) value = new { data = new { is_valid = true, app_id = "probe-app", expires_at = 0, scopes = new[] { "pages_read_engagement", "pages_manage_posts", "instagram_basic", "instagram_content_publish" } } };
        else if (path.EndsWith("page1")) value = new { id = "page1", name = "Page", instagram_business_account = new { id = "ig1", username = "flowers" } };
        else throw new Exception("Unexpected fake provider route");
        return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json") });
    }
}
